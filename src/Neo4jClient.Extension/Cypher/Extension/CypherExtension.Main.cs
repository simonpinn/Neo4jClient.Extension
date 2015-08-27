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


        public static ICypherFluentQuery CreateEntity<T>(this ICypherFluentQuery query, T entity, string paramKey = null, List<CypherProperty> onCreateOverride = null, string preCql = "", string postCql = "") where T : class
        {
            paramKey = entity.EntityParamKey(paramKey);
           
            var cypher2 = GetMatchWithParam(paramKey, entity.EntityLabel(), paramKey);
            var cql = string.Format("{0}({1}){2}", preCql, cypher2, postCql);

            var createProperties = GetCreateProperties(entity);

            var options = new CreateDynamicOptions {IgnoreNulls = true}; // working around some buug where null properties are blowing up. don't care on create.
            dynamic cutdownEntity = entity.CreateDynamic(createProperties, options);
            
            query = query.Create(cql);
            query = query.WithParam(paramKey, cutdownEntity);

            return query;
        }

        public static ICypherFluentQuery MergeEntity<T>(this ICypherFluentQuery query, T entity, string paramKey = null, List<CypherProperty> mergeOverride = null, List<CypherProperty> onMatchOverride = null, List<CypherProperty> onCreateOverride = null,string preCql = "", string postCql = "") where T : class
        {
            var options = new MergeOptions
            {
                ParamKey = entity.EntityParamKey(paramKey),
                PreCql = preCql,
                PostCql = postCql,
                MergeOverride = mergeOverride,
                OnCreateOverride = onCreateOverride,
                OnMatchOverride = onMatchOverride
            };
            return MergeEntity(query, entity, options);
        }

        public static ICypherFluentQuery MergeEntity<T>(this ICypherFluentQuery query, T entity, MergeOptions options) where T : class
        {
            var context = CypherExtensionContext.Create(query);
            string pattern = string.Empty;
            string cql = string.Empty;
            if (options.MergeViaRelationship != null)
            {
                var relationshipSegment = GetAliasLabelCql(string.Empty, options.MergeViaRelationship.EntityLabel()); 
                pattern = GetRelationshipCql(
                options.MergeViaRelationship.FromKey
                , relationshipSegment
                , options.MergeViaRelationshipLabel ? string.Concat(options.MergeViaRelationship.ToKey, ":", entity.EntityLabel()) : options.MergeViaRelationship.ToKey);
                cql = string.Format("{0}{1}{2}", options.PreCql, pattern, options.PostCql);
            }
            else
            {
                pattern = entity.ToCypherString<T, CypherMergeAttribute>(context, options.ParamKey, options.MergeOverride);
                cql = string.Format("{0}({1}){2}", options.PreCql, pattern, options.PostCql);
            }

            //var cql = string.Format("{0}({1}){2}", options.PreCql, pattern, options.PostCql);
            return query.CommonMerge(entity, options.ParamKey, cql, options.MergeOverride, options.OnMatchOverride, options.OnCreateOverride);
        }

        public static ICypherFluentQuery CreateRelationship<T>(this ICypherFluentQuery query, T entity) where T : BaseRelationship
        {
            //bug: isn't creating properties 
            var relationshipSegment = GetAliasLabelCql(entity.Key, entity.EntityLabel()); 
            var cql = GetRelationshipCql(entity.FromKey, relationshipSegment, entity.ToKey);
            return query.Create(cql);
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
            // write once, read never!
            var regex = new Regex("\\\"([^(\\\")\"]+)\\\":", RegexOptions.Multiline);
            return regex.Replace(query.Query.DebugQueryText, "$1:");
        }
    }
}
