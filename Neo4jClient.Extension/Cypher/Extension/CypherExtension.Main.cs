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
        
        public static ICypherFluentQuery MatchEntity<T>(this ICypherFluentQuery query, T entity, string paramKey = null, string preCql = "", string postCql = "", List<CypherProperty> propertyOverride = null) where T : class
        {
            paramKey = entity.EntityParamKey(paramKey);
            var matchCypher = entity.ToCypherString<T, CypherMatchAttribute>(CypherExtensionContext.Create(query), paramKey, propertyOverride);
            var cql = string.Format("{0}({1}){2}", preCql, matchCypher, postCql);
            //create a dynamic object for the type
            dynamic cutdown = entity.CreateDynamic(propertyOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMatchAttribute>(entity));

            var matchKey = GetMatchParamName(paramKey);

            return query
                .Match(cql)
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
            paramKey = entity.EntityParamKey(paramKey);
            var context = CypherExtensionContext.Create(query);
            var cypher1= entity.ToCypherString<T, CypherMergeAttribute>(context, paramKey, mergeOverride);
            var cql = string.Format("{0}({1}){2}", preCql, cypher1, postCql);
            return query.CommonMerge(entity, paramKey, cql, mergeOverride, onMatchOverride, onCreateOverride);
        }

        public static ICypherFluentQuery CreateRelationship<T>(this ICypherFluentQuery query, T entity) where T : BaseRelationship
        {
            var relationshipSegment = entity.Key + ":" + entity.EntityLabel();//need this if creating propertites:  entity.ToCypherString<T, CypherMergeAttribute>(CypherExtensionContext.Create(query), entity.Key);
            var cql = GetRelationshipCql(entity.FromKey, relationshipSegment, entity.ToKey);
            return query.Create(cql);
        }

        public static ICypherFluentQuery MergeRelationship<T>(this ICypherFluentQuery query, T entity, List<CypherProperty> mergeOverride = null, List<CypherProperty> onMatchOverride = null, List<CypherProperty> onCreateOverride = null) where T : BaseRelationship
        {
            //Eaxctly the same as a merge entity except the cql is different
            var cql = GetRelationshipCql(entity.FromKey
                , entity.ToCypherString<T, CypherMergeAttribute>(CypherExtensionContext.Create(query), entity.Key, mergeOverride)
                , entity.ToKey);

            return query.CommonMerge(entity, entity.Key, cql, mergeOverride, onMatchOverride, onCreateOverride);
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
            var regex = new Regex("\\\"([^(\\\")\"]+)\\\":", RegexOptions.Multiline);
            return regex.Replace(query.Query.DebugQueryText, "$1:");
        }
    }
}
