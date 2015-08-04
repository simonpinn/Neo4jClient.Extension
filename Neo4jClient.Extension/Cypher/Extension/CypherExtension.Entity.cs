using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher.Attributes;

namespace Neo4jClient.Extension.Cypher
{
    public static partial class CypherExtension
    {
        public static string EntityLabel<T>(this T entity)
        {
            var entityType = entity.GetType();
            if (!EntityLabelCache.ContainsKey(entityType))
            {
                var label = entityType.GetCustomAttributes(typeof(CypherLabelAttribute), true).FirstOrDefault() as CypherLabelAttribute;
                EntityLabelCache.Add(entityType, label == null ? entityType.Name : label.Name);
            }
            return EntityLabelCache[entityType];
        }

        public static string EntityParamKey<T>(this T entity, string paramKey = null)
        {
            return paramKey ?? entity.GetType().Name.ToLowerInvariant();
        }
    }
}
