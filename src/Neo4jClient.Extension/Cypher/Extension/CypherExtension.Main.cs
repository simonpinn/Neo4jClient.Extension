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
            var options = new CreateOptions();

            options.PostCql = postCql;
            options.PreCql = preCql;
            options.Identifier = identifier;
            options.CreateOverride = onCreateOverride;

            return CreateEntity(query, entity, options);
        }

        public static ICypherFluentQuery CreateEntity<T>(this ICypherFluentQuery query, T entity, CreateOptions options) where T : class
        {
            Func<string, string> getFinalCql = intermediateCql => WithPrePostWrap(intermediateCql, options);

            query = CommonCreate(query, entity, options, getFinalCql);

            return query;
        }

        public static ICypherFluentQuery CreateRelationship<T>(this ICypherFluentQuery query, T entity, CreateOptions options = null) where T : BaseRelationship
        {
            Func<string, string> getFinalCql = intermediateCql => GetRelationshipCql(entity.FromKey, intermediateCql, entity.ToKey);

            if (options == null)
            {
                options = new CreateOptions();
                options.Identifier = entity.Key;
            }

            query = CommonCreate(query, entity, options, getFinalCql);

            return query;
        }
        
        private static ICypherFluentQuery CommonCreate<T>(
            this ICypherFluentQuery query
            , T entity
            , CreateOptions options
            , Func<string, string> getFinalCql) where T : class
        {
            if (options == null)
            {
                options = new CreateOptions();
            }

            var createProperties = GetCreateProperties(entity);
            var identifier = entity.EntityParamKey(options.Identifier);
            var intermediateCreateCql = GetMatchWithParam(identifier, entity.EntityLabel(), createProperties.Count > 0 ? identifier : "");

            var createCql = getFinalCql(intermediateCreateCql);

            var dynamicOptions = new CreateDynamicOptions { IgnoreNulls = true }; // working around some buug where null properties are blowing up. don't care on create.
            var cutdownEntity = entity.CreateDynamic(createProperties, dynamicOptions);

            query = query.Create(createCql);

            if (createProperties.Count > 0)
            {
                query = query.WithParam(identifier, cutdownEntity);
            }

            return query;
        }

        public static ICypherFluentQuery MergeEntity<T>(this ICypherFluentQuery query, T entity, string paramKey = null, List<CypherProperty> mergeOverride = null, List<CypherProperty> onMatchOverride = null, List<CypherProperty> onCreateOverride = null, string preCql = "", string postCql = "") where T : class
        {
            paramKey = entity.EntityParamKey(paramKey);
            var context = CypherExtensionContext.Create(query);
            var cypher1 = entity.ToCypherString<T, CypherMergeAttribute>(context, paramKey, mergeOverride);
            var cql = string.Format("{0}({1}){2}", preCql, cypher1, postCql);
            return query.CommonMerge(entity, paramKey, cql, mergeOverride, onMatchOverride, onCreateOverride);
        }

        public static ICypherFluentQuery MergeEntity<T>(this ICypherFluentQuery query, T entity, MergeOptions options) where T : class
        {
            var context = CypherExtensionContext.Create(query);
            string pattern;

            if (options.MergeViaRelationship != null)
            {
                var relationshipSegment = GetAliasLabelCql(string.Empty, options.MergeViaRelationship.EntityLabel());

                pattern = GetRelationshipCql(
                    options.MergeViaRelationship.FromKey
                    , relationshipSegment
                    , GetAliasLabelCql(options.MergeViaRelationship.ToKey, entity.EntityLabel()));
            }
            else
            {
                pattern = entity.ToCypherString<T, CypherMergeAttribute>(context, options.Identifier, options.MergeOverride);
            }
            var wrappedPattern = string.Format("{0}({1}){2}", options.PreCql, pattern, options.PostCql);
            return query.CommonMerge(entity, options.Identifier, wrappedPattern, options.MergeOverride, options.OnMatchOverride, options.OnCreateOverride);
        }

        public static ICypherFluentQuery MergeRelationship<T>(this ICypherFluentQuery query, T entity, List<CypherProperty> mergeOverride = null, List<CypherProperty> onMatchOverride = null, List<CypherProperty> onCreateOverride = null) where T : BaseRelationship
        {
            //Eaxctly the same as a merge entity except the cql is different
            var cql = GetRelationshipCql(
                entity.FromKey
                , entity.ToCypherString<T, CypherMergeAttribute>(CypherExtensionContext.Create(query), entity.Key, mergeOverride)
                , entity.ToKey);

            return query.CommonMerge(entity, entity.Key, cql, mergeOverride, onMatchOverride, onCreateOverride);
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
            this ICypherFluentQuery query
            , T entity
            , string key
            , string mergeCql
            , List<CypherProperty> mergeOverride = null
            , List<CypherProperty> onMatchOverride = null
            , List<CypherProperty> onCreateOverride = null) where T : class
        {
            //A merge requires the properties of both merge, create and match in the cutdown object
            var mergeProperties = mergeOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMergeAttribute>(entity);
            var createProperties = GetCreateProperties(entity, onCreateOverride);
            var matchProperties = onMatchOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMergeOnMatchAttribute>(entity);

            dynamic mergeObjectParam = entity.CreateDynamic(mergeProperties);
            var matchParamName = GetMatchParamName(key);

            query = query.Merge(mergeCql);
            query = query.WithParam(matchParamName, mergeObjectParam);

            if (matchProperties.Count > 0)
            {
                var entityType = entity.GetType();
                foreach (var matchProperty in matchProperties)
                {
                    var propertyParam = key + matchProperty.JsonName;
                    var propertyValue = GetValue(entity, matchProperty, entityType);
                    query = query.OnMatch().Set(GetSetWithParamCql(key, matchProperty.JsonName, propertyParam));
                    query = query.WithParam(propertyParam, propertyValue);
                }
            }

            if (createProperties.Count > 0)
            {
                var createParamName = key + "OnCreate";
                dynamic createObjectParam = entity.CreateDynamic(createProperties);
                query = query.OnCreate().Set(GetSetWithParamCql(key, createParamName));
                query = query.WithParam(createParamName, createObjectParam);
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
