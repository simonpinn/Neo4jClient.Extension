using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClient.Extension.Cypher
{
    public static partial class CypherExtension
    {
        private static Dictionary<string, object> CreateDynamic<TEntity>(
            this TEntity entity,
            List<CypherProperty> properties,
            CreateDynamicOptions options = null) where TEntity : class
        {
            if (properties == null || !properties.Any())
            {
                // Return an empty dictionary if no properties are provided
                return new Dictionary<string, object>();
            }

            options = options ?? new CreateDynamicOptions();
            var type = entity.GetType();

            var propertiesForDict = properties
                .Where(prop => prop != null)
                .Select(prop => new
                {
                    Key = prop.JsonName,
                    Value = GetValue(entity, prop, type)
                })
                .ToList();

            if (options.IgnoreNulls)
            {
                propertiesForDict.RemoveAll(p => p.Value == null);
            }

            return propertiesForDict.ToDictionary(x => x.Key, x => x.Value);
        }


        private static object GetValue<TEntity>(TEntity entity, CypherProperty property, Type entityTypeCache = null)
        {
            var entityType = entityTypeCache ?? entity.GetType();
            var value = entityType.GetProperty(property.TypeName).GetValue(entity, null);
            return value;
        }
    }
}
