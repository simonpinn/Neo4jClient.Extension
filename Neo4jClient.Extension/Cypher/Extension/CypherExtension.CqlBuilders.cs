using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher.Attributes;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Extension.Cypher
{
    public static partial class  CypherExtension
    {
        private static string GetMatchWithParam(string key, string label, string paramName)
        {
            return GetMatchCypher(key, label, AsWrappedVariable(paramName));
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
        
        private static string GetSetWithParamCql(string alias, string paramName)
        {
            var cql = string.Format("{0} = {{{1}}}", alias, paramName);
            return cql;
        }

        private static string GetSetWithParamCql(string alias, string property, string paramName)
        {
            var cql = GetSetWithParamCql(alias + "." + property, paramName);
            return cql;
        }

        private static string GetMatchParamName(string key)
        {
            return key + "MatchKey";
        }
        
        private static string GetRelationshipCql(string aliasFrom, string aliasRelationship, string aliasTo)
        {
            var cql = string.Format("({0})-[{1}]->({2})"
                , aliasFrom
                , aliasRelationship
                , aliasTo);

            return cql;
        }
        
        internal static string GetMatchCypher<TEntity>(this TEntity entity
            , ICypherExtensionContext context
            , List<CypherProperty> useProperties
            , string paramKey)
            where TEntity : class
        {
            var label = entity.EntityLabel();
            paramKey = entity.EntityParamKey(paramKey);

            var matchProperties = useProperties.Select(x => string.Format("{0}:{{{1}}}.{0}", x.JsonName, GetMatchParamName(paramKey)));

            var jsonProperties = string.Join(",", matchProperties);

            var braceWrappedProperties = AsWrappedVariable(jsonProperties);

            var cypher = GetMatchCypher(paramKey, label, braceWrappedProperties);
            return cypher;
        }
        
        public static string ToCypherString<TEntity, TAttr>(this TEntity entity, ICypherExtensionContext context, string paramKey = null, List<CypherProperty> useProperties = null)
            where TAttr : CypherExtensionAttribute
            where TEntity : class
        {
            var properties = useProperties ?? CypherTypeItemHelper.PropertiesForPurpose<TEntity, TAttr>(entity);

            return entity.GetMatchCypher(context, properties, paramKey);
        }
        public static string ApplyCasing(this string value, ICypherExtensionContext context)
        {
            var useCamelCase = (context.JsonContractResolver is CamelCasePropertyNamesContractResolver);
            if (useCamelCase)
            {
                return string.Format(
                    "{0}{1}"
                    , value.Substring(0, 1).ToLowerInvariant()
                    , value.Length > 1 ? value.Substring(1, value.Length - 1) : string.Empty);
            }
            return value;
        }
    }
}
