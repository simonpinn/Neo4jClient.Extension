using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Extension.Cypher
{
    public static partial class CypherExtension
    {
        private static readonly CypherTypeItemHelper CypherTypeItemHelper = new CypherTypeItemHelper();
        public static CypherExtensionContext DefaultExtensionContext = new CypherExtensionContext();
        private static readonly Dictionary<Type, string> EntityLabelCache = new Dictionary<Type, string>();
        
        public static ICypherFluentQuery MatchEntity<T>(this ICypherFluentQuery query, T entity, string identifier = null, string preCql = "", string postCql = "", List<CypherProperty> propertyOverride = null) where T : class
        {
            var options = new MatchOptions
            {
                Identifier = identifier,
                PreCql = preCql,
                PostCql = postCql,
                MatchOverride = propertyOverride
            };
            return MatchEntity(query, entity, options);
        }

        public static ICypherFluentQuery MatchEntity<T>(this ICypherFluentQuery query, T entity, MatchOptions options)
            where T : class
        {
            return MatchWorker(query, entity, options, (q, s) => q.Match(s));
        }

        public static ICypherFluentQuery OptionalMatchEntity<T>(this ICypherFluentQuery query, T entity, MatchOptions options= null)
           where T : class
        {
            if (options == null)
            {
                options = new MatchOptions();
            }
            return MatchWorker(query, entity, options, (q, s) => q.OptionalMatch(s));
        }

        private static ICypherFluentQuery MatchWorker<T>(this ICypherFluentQuery query, T entity, MatchOptions options, Func<ICypherFluentQuery, string, ICypherFluentQuery> matchFunction) where T : class
        {
            var identifier = entity.EntityParamKey(options.Identifier);
            var matchCypher = entity.ToCypherString<T, CypherMatchAttribute>(CypherExtensionContext.Create(query), identifier, options.MatchOverride);
            var cql = string.Format("{0}({1}){2}", options.PreCql, matchCypher, options.PostCql);
            dynamic cutdown = entity.CreateDynamic(options.MatchOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMatchAttribute>(entity));

            var matchKey = GetMatchParamName(identifier);

            return matchFunction(query,cql)
                    .WithParam(matchKey, cutdown);
        }

        public static ICypherFluentQuery CreateEntity<T>(this ICypherFluentQuery query, T entity, string identifier = null, List<CypherProperty> onCreateOverride = null, string preCql = "", string postCql = "") where T : class
        {
            var options = new CreateOptions
            {
                PostCql = postCql,
                PreCql = preCql,
                Identifier = identifier,
                CreateOverride = onCreateOverride
            };

            // Pass the intermediate CQL without adding "CREATE" here
            query = CommonCreate(query, entity, options, intermediateCql => intermediateCql);

            return query;
        }



        public static ICypherFluentQuery CreateEntity<T>(this ICypherFluentQuery query, T entity, CreateOptions options) where T : class
        {
            Func<string, string> getFinalCql = intermediateCql => WithPrePostWrap(intermediateCql, options);

            query = CommonCreate(query, entity, options, getFinalCql);

            return query;
        }

        public static ICypherFluentQuery CreateRelationship<T>(this ICypherFluentQuery query, T entity, CreateOptions options = null) where T : BaseRelationship
        {
            if (options == null)
            {
                options = new CreateOptions { Identifier = entity.Key };
            }

            // Generate a unique parameter name for the relationship properties
            var uniqueParamName = $"{options.Identifier}_RelationshipParams";

            // Call CommonCreate and pass the unique parameter name correctly
            query = CommonCreate(query, entity, options, intermediateCql =>
            {
                // Get the relationship type, ensuring it’s correctly formatted for Neo4j syntax
                var relationshipType = entity.GetType().Name.ToUpper(); // Or a specific property if type is stored differently

                // Format the relationship pattern and add the SET statement with the unique parameter name
                var relationshipCql = $"({entity.FromKey})-[{options.Identifier}:{relationshipType}]->({entity.ToKey})";
                return $"{relationshipCql} SET {options.Identifier} = ${uniqueParamName}";
            });

            // Ensure WithParam is called with the generated unique parameter name
            query = query.WithParam(uniqueParamName, entity.CreateDynamic(options.CreateOverride));

            return query;
        }



        private static ICypherFluentQuery CommonCreate<T>(
            this ICypherFluentQuery query,
            T entity,
            CreateOptions options,
            Func<string, string> getFinalCql) where T : class
        {
            if (options == null)
            {
                options = new CreateOptions();
            }

            var createProperties = GetCreateProperties(entity, options.CreateOverride);
            var identifier = entity.EntityParamKey(options.Identifier);

            // Construct the node or relationship creation pattern
            var intermediateCreateCql = $"({identifier}:{entity.EntityLabel()})";
            var createCql = getFinalCql(intermediateCreateCql);

            // Use SET only if there are properties to apply
            var dynamicOptions = new CreateDynamicOptions { IgnoreNulls = true };
            var cutdownEntity = entity.CreateDynamic(createProperties, dynamicOptions);

            query = query.Create(createCql);

            if (createProperties.Count > 0)
            {
                // Ensure unique parameter names for relationships
                var paramName = $"{identifier}_CreateParams";
                query = query.Set($"{identifier} = ${paramName}")
                    .WithParam(paramName, cutdownEntity);
            }

            return query;
        }




        public static ICypherFluentQuery MergeEntity<T>(
            this ICypherFluentQuery query,
            T entity,
            MergeOptions options = null,
            List<CypherProperty> mergeOverride = null,
            List<CypherProperty> onMatchOverride = null,
            List<CypherProperty> onCreateOverride = null,
            CypherExtensionContext context = null) where T : class
        {
            options = options ?? new MergeOptions();
            context = context ?? CypherExtension.DefaultExtensionContext;

            // Determine identifier for the entity
            var identifier = options.Identifier ?? entity.EntityParamKey();

            // If mergeOverride is provided, use it; otherwise, retrieve configured merge properties
            var mergeProperties = mergeOverride ?? CypherExtension.GetPropertiesForConfig(entity, context, typeof(CypherMergeAttribute));
            var onCreateProperties = onCreateOverride ?? CypherExtension.GetPropertiesForConfig(entity, context, typeof(CypherMergeOnCreateAttribute));
            var onMatchProperties = onMatchOverride ?? CypherExtension.GetPropertiesForConfig(entity, context, typeof(CypherMergeOnMatchAttribute));

            // Generate the Cypher pattern for the entity
            var pattern = entity.ToCypherString<T, CypherMergeAttribute>(context, identifier);

            // Construct the full Cypher query with pre- and post-Cypher strings
            var wrappedPattern = $"{options.PreCql}({pattern}){options.PostCql}";

            // Call CommonMerge with the constructed pattern and configuration overrides
            return query.CommonMerge(
                entity,
                identifier,
                wrappedPattern,
                context,
                preCql: options.PreCql,
                postCql: options.PostCql,
                mergeOverride: mergeProperties,
                onMatchOverride: onMatchProperties,
                onCreateOverride: onCreateProperties
            );
        }




        public static ICypherFluentQuery MergeRelationship<T>(
            this ICypherFluentQuery query,
            T entity,
            List<CypherProperty> mergeOverride = null,
            List<CypherProperty> onMatchOverride = null,
            List<CypherProperty> onCreateOverride = null) where T : BaseRelationship
        {
            var context = CypherExtensionContext.Create(query);

            // Construct the relationship-specific Cypher pattern
            var relationshipPattern = GetRelationshipCql(
                entity.FromKey,
                entity.ToCypherString<T, CypherMergeAttribute>(context, entity.Key, mergeOverride),
                entity.ToKey
            );

            // Use CommonMerge for handling MERGE, ON MATCH, and ON CREATE with relationship pattern
            return query.CommonMerge(
                entity,
                entity.Key,
                relationshipPattern,
                context,
                mergeOverride: mergeOverride,
                onMatchOverride: onMatchOverride,
                onCreateOverride: onCreateOverride
            );
        }


        public static ICypherFluentQuery MatchRelationship<T>(
            this ICypherFluentQuery query
            , T relationship
            , MatchRelationshipOptions options) where T : BaseRelationship
        {
            return MatchRelationshipWorker(query, relationship, options, (fluentQuery, s) => fluentQuery.Match(s));
        }

        public static ICypherFluentQuery OptionalMatchRelationship<T>(
          this ICypherFluentQuery query
          , T relationship
          , MatchRelationshipOptions options = null) where T : BaseRelationship
        {
            if (options == null)
            {
                options = new MatchRelationshipOptions();
            }
            return MatchRelationshipWorker(query, relationship, options, (fluentQuery, s) => fluentQuery.OptionalMatch(s));
        }

        private static ICypherFluentQuery MatchRelationshipWorker<T>(
            this ICypherFluentQuery query
            , T relationship
            , MatchRelationshipOptions options
            , Func<ICypherFluentQuery, string, ICypherFluentQuery> matchFunction) where T : BaseRelationship
        {
            var matchProperties = options.MatchOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMatchAttribute>(relationship);
            var cql = GetRelationshipCql(
                relationship.FromKey
                , relationship.ToCypherString<T, CypherMatchAttribute>(CypherExtensionContext.Create(query), relationship.Key, matchProperties)
                , relationship.ToKey);

            return matchFunction(query,cql);
        }

        public static ICypherFluentQuery MatchRelationship<T>(this ICypherFluentQuery query, T relationship, List<CypherProperty> matchOverride = null) where T : BaseRelationship
        {
            var options = new MatchRelationshipOptions();
            options.MatchOverride = matchOverride;
            return MatchRelationship(query, relationship, options);
        }

        private static ICypherFluentQuery CommonMerge<T>(
    this ICypherFluentQuery query,
    T entity,
    string key,
    string mergeCql,
    CypherExtensionContext context,
    string preCql = "",
    string postCql = "",
    List<CypherProperty> mergeOverride = null,
    List<CypherProperty> onMatchOverride = null,
    List<CypherProperty> onCreateOverride = null) where T : class
        {
            // Use provided overrides or fetch based on configuration attributes
            var mergeProperties = mergeOverride ?? CypherExtension.GetPropertiesForConfig(entity, context, typeof(CypherMergeAttribute));
            var onCreateProperties = onCreateOverride ?? CypherExtension.GetPropertiesForConfig(entity, context, typeof(CypherMergeOnCreateAttribute));
            var onMatchProperties = onMatchOverride ?? CypherExtension.GetPropertiesForConfig(entity, context, typeof(CypherMergeOnMatchAttribute));

            var mergeParamName = $"{key}_MergeParams";
            var createParamName = $"{key}_OnCreateParams";
            var matchParamPrefix = $"{key}_OnMatch";

            // Perform the MERGE operation with the primary properties
            if (mergeProperties.Any())
            {
                var mergeObjectParam = entity.CreateDynamic(mergeProperties);
                query = query.Merge(preCql + mergeCql + postCql).WithParam(mergeParamName, mergeObjectParam);
            }

            // Conditionally apply ON MATCH SET if onMatchProperties has values
            if (onMatchProperties.Any())
            {
                foreach (var prop in onMatchProperties)
                {
                    var matchParamName = $"{matchParamPrefix}_{prop.JsonName}";
                    var matchParamValue = GetValue(entity, prop);
                    query = query.OnMatch().Set($"{key}.{prop.JsonName} = ${matchParamName}").WithParam(matchParamName, matchParamValue);
                }
            }

            // Conditionally apply ON CREATE SET if onCreateProperties has values
            if (onCreateProperties.Any())
            {
                var createObjectParam = entity.CreateDynamic(onCreateProperties);
                query = query.OnCreate().Set($"{key} = ${createParamName}").WithParam(createParamName, createObjectParam);
            }

            return query;
        }




        public static string GetFormattedDebugText(this ICypherFluentQuery query)
        {
            return GetFormattedCypher(query.Query.DebugQueryText);
        }

        public static string GetFormattedCypher(string cypherText)
        {
            var regex = new Regex("\\\"([^(\\\")\"]+)\\\":", RegexOptions.Multiline);
            var s = regex.Replace(cypherText, "$1:");
            s = s.Replace("ON MATCH\r\nSET", "ON MATCH SET");   // this is more readable
            s = s.Replace("ON CREATE\r\nSET", "ON CREATE SET");
            return s;
        }
    }
}
