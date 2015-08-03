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
    public class CreateDynamicOptions
    {
        public bool IgnoreNulls { get; set; }

        public override string ToString()
        {
            return string.Format("IgnoreNulls={0}", IgnoreNulls);
        }
    }

    public static class CypherExtension
    {
        private static readonly CypherTypeItemHelper CypherTypeItemHelper = new CypherTypeItemHelper();
        public static CypherExtensionContext DefaultExtensionContext = new CypherExtensionContext();
        private static readonly Dictionary<Type, string> EntityLabelCache = new Dictionary<Type, string>();

        public static string EntityLabel<T>(this T entity)
        {
            var entityType = entity.GetType();
            if (!EntityLabelCache.ContainsKey(entityType))
            {
                var label = entityType.GetCustomAttributes(typeof (CypherLabelAttribute), true).FirstOrDefault() as CypherLabelAttribute;
                EntityLabelCache.Add(entityType, label == null ? entityType.Name : label.Name);
            }
            return EntityLabelCache[entityType];
        }

        public static string EntityParamKey<T>(this T entity, string paramKey = null)
        {
            return paramKey ?? entity.GetType().Name.ToLowerInvariant();
        }

        internal static string GetMatchCypher<TEntity>(this TEntity entity
            , ICypherExtensionContext context
            , List<CypherProperty> useProperties
            , string paramKey)
            where TEntity : class
        {
            var label = entity.EntityLabel();
            paramKey = entity.EntityParamKey(paramKey);
            
            var matchProperties = useProperties.Select(x => string.Format("{0}:{{{1}}}.{0}", x.JsonName, GetMergeParamName(paramKey)));

            var jsonProperties = string.Join(",", matchProperties);
            
            var braceWrappedProperties = AsWrappedVariable(jsonProperties);

            var cypher = GetMatchCypher(paramKey, label, braceWrappedProperties);
            return cypher;
        }

        private static string GetMatchWithParam(string key, string label, string paramName)
        {
            return GetMatchCypher( key, label, AsWrappedVariable(paramName));
        }

        private static string GetMatchCypher(string key, string label, string variable)
        {
            var cypher = string.Format("{0}:{1} {2}", key, label, variable);
            return cypher;
        }

        private static string AsWrappedVariable(string input)
        {
            var output = string.Format("{{{0}}}", input);
            return output;
        }

        public static string ToCypherString<TEntity, TAttr>(this TEntity entity, ICypherExtensionContext context, string paramKey = null, List<CypherProperty> useProperties = null)
            where TAttr : CypherExtensionAttribute
            where TEntity : class
        {
            var properties = useProperties ?? CypherTypeItemHelper.PropertiesForPurpose<TEntity, TAttr>(entity);
            
            return entity.GetMatchCypher(context, properties, paramKey);
        }
        
        public static Dictionary<string, object> CreateDynamic<TEntity>(
            this TEntity entity
            , List<CypherProperty> properties
            , CreateDynamicOptions options = null) where TEntity : class
        {
            if (options == null)
            {
                options = new CreateDynamicOptions();
            }

            var type = entity.GetType();
            var propertiesForDict = properties.Select(
                prop => new
                {
                    Key = prop.JsonName
                    ,Value = GetValue(entity, prop, type)}
                ).ToList();

            if (options.IgnoreNulls)
            {
                propertiesForDict.RemoveAll(p => p.Value == null);
            }

            return propertiesForDict.ToDictionary(x => x.Key, x => x.Value);
        }

        private static object GetValue<TEntity>(TEntity entity, CypherProperty property, Type entityTypeCache= null)
        {
            var entityType = entityTypeCache ?? entity.GetType();
            var value = entityType.GetProperty(property.TypeName).GetValue(entity, null);
            return value;
        }

        public static ICypherFluentQuery MatchEntity<T>(this ICypherFluentQuery query, T entity, string paramKey = null, string preCql = "", string postCql = "", List<CypherProperty> propertyOverride = null) where T : class
        {
            paramKey = entity.EntityParamKey(paramKey);
            var matchCypher = entity.ToCypherString<T, CypherMatchAttribute>(CypherExtensionContext.Create(query), paramKey, propertyOverride);
            var cql = string.Format("{0}({1}){2}", preCql, matchCypher, postCql);
            //create a dynamic object for the type
            dynamic cutdown = entity.CreateDynamic(propertyOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMatchAttribute>(entity));

            var matchKey = GetMergeParamName(paramKey);

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

        private static string GetRelationshipCql(string aliasFrom, string aliasRelationship, string aliasTo)
        {
            var cql = string.Format("({0})-[{1}]->({2})"
                , aliasFrom
                , aliasRelationship
                , aliasTo);

            return cql;
        }

        private static List<CypherProperty> GetCreateProperties<T>(T entity, List<CypherProperty> onCreateOverride = null) where T : class
        {
            var properties = onCreateOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMergeOnCreateAttribute>(entity);
            return properties;
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
            var mergeParamName = GetMergeParamName(key);

            query = query.Merge(mergeCql);
            query = query.WithParam(mergeParamName, mergeObjectParam);

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

        private static string GetSetWithParamCql(string alias, string paramName )
        {
            var cql = string.Format("{0} = {{{1}}}", alias, paramName);
            return cql;
        }

        private static string GetSetWithParamCql(string alias, string property, string paramName)
        {
            var cql = GetSetWithParamCql(alias + "." + property, paramName);
            return cql;
        }

        private static string GetMergeParamName(string key)
        {
            return key + "MergeKey";
        }

        public static List<CypherProperty> UseProperties<T>(this T entity, params Expression<Func<T, object>>[] properties)
            where T : class
        {
            return entity.UseProperties(DefaultExtensionContext, properties);
        }

        public static List<CypherProperty> UseProperties<T>(this T entity, CypherExtensionContext context, params Expression<Func<T, object>>[] properties)
            where T : class
        {
            //Cache the T entity properties into a dictionary of strings
            if (properties != null)
            {
                return properties.ToList().Where(x => x != null).Select(x =>
                {
                    var memberExpression = x.Body as MemberExpression ?? ((UnaryExpression) x.Body).Operand as MemberExpression;
                    return memberExpression == null ? null : memberExpression.Member.Name;
                }).Select(x => new CypherProperty {TypeName = x, JsonName = x.ApplyCasing(context)}).ToList();
            }
            return new List<CypherProperty>();
        }

        public static string GetFormattedDebugText(this ICypherFluentQuery query)
        {
            var regex = new Regex("\\\"([^(\\\")\"]+)\\\":", RegexOptions.Multiline);
            return regex.Replace(query.Query.DebugQueryText, "$1:");
        }

        public static string ApplyCasing(this string value, ICypherExtensionContext context)
        {
            var camelCase = (context.JsonContractResolver is CamelCasePropertyNamesContractResolver);
            return camelCase ? string.Format("{0}{1}", value.Substring(0, 1).ToLowerInvariant(), value.Length > 1 ? value.Substring(1, value.Length - 1) : string.Empty)
                                : value;
        }

        public static void ConfigProperties(CypherTypeItem type, List<CypherProperty> properties)
        {
            CypherTypeItemHelper.AddPropertyUsage(type, properties);
        }
        public static void ConfigLabel(Type type, string label)
        {
            if (EntityLabelCache.ContainsKey(type))
            {
                EntityLabelCache[type] = label;
            }
            else
            {
                EntityLabelCache.Add(type, label);
            }
        }
    }
}
