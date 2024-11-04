using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher.Attributes;

namespace Neo4jClient.Extension.Cypher
{
    /// <summary>
    /// Entity extension methods
    /// </summary>
    public static partial class CypherExtension
    {
        private static readonly object _syncRoot = new object();

        internal static string EntityLabel<T>(this T entity)
        {
            var entityType = entity.GetType();

            // http://stackoverflow.com/questions/157933
            lock (_syncRoot)
            {
                if (!EntityLabelCache.ContainsKey(entityType))
                {
                    var label = entityType.GetCustomAttributes(typeof (CypherLabelAttribute), true).FirstOrDefault() as CypherLabelAttribute;

                    try
                    {
                        EntityLabelCache.Add(entityType, label == null ? entityType.Name : label.Name);
                    }
                    catch (ArgumentException e)
                    {
                        var moreInfoException = new ArgumentException($"Failed to cache label '{label}' for type='{typeof(T).Name}'", e);
                        throw moreInfoException;
                    }
                }
            }

            var output = EntityLabelCache[entityType];
            return output;
        }

        internal static string EntityParamKey<T>(this T entity, string paramKey = null)
        {
            return paramKey ?? entity.GetType().Name.ToLowerInvariant();
        }


        public static List<CypherProperty> UseProperties<T>(this T entity, params Expression<Func<T, object>>[] properties)
            where T : class
        {
            return entity.UseProperties(DefaultExtensionContext, properties);
        }

        internal static List<CypherProperty> UseProperties<T>(this T entity, CypherExtensionContext context, params Expression<Func<T, object>>[] properties)
            where T : class
        {
            //Cache the T entity properties into a dictionary of strings
            if (properties != null)
            {
                return properties.ToList().Where(x => x != null).Select(x =>
                {
                    var memberExpression = x.Body as MemberExpression ?? ((UnaryExpression)x.Body).Operand as MemberExpression;
                    return memberExpression == null ? null : memberExpression.Member.Name;
                }).Select(x => new CypherProperty { TypeName = x, JsonName = x.ApplyCasing(context) }).ToList();
            }
            return new List<CypherProperty>();
        }

        private static List<CypherProperty> GetCreateProperties<T>(T entity, List<CypherProperty> onCreateOverride = null) where T : class
        {
            return onCreateOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMergeOnCreateAttribute>(entity);
        }

        internal static List<CypherProperty> GetMatchProperties<T>(T entity) where T : class
        {
            return CypherTypeItemHelper.PropertiesForPurpose<T, CypherMatchAttribute>(entity);
        }

        internal static List<CypherProperty> GetMergeOnCreateProperties<T>(T entity) where T : class
        {
            return CypherTypeItemHelper.PropertiesForPurpose<T, CypherMergeOnCreateAttribute>(entity);
        }

        internal static List<CypherProperty> GetMergeOnMatchProperties<T>(T entity) where T : class
        {
            return CypherTypeItemHelper.PropertiesForPurpose<T, CypherMergeOnMatchAttribute>(entity);
        }


        public static List<CypherProperty> GetPropertiesForConfig<T>(T entity, CypherExtensionContext context, Type attributeType) where T : class
        {
            var properties = new List<CypherProperty>();

            // Get the CypherTypeItemHelper instance
            var helper = new CypherTypeItemHelper();

            // Use reflection to call the generic method PropertiesForPurpose<TEntity, TAttr>
            var method = typeof(CypherTypeItemHelper).GetMethod("PropertiesForPurpose", new[] { typeof(ICypherExtensionContext), typeof(T) });
            if (method != null)
            {
                // Make a generic method with the specified attributeType
                var genericMethod = method.MakeGenericMethod(typeof(T), attributeType);

                // Invoke the generic method with the context and entity as parameters
                var result = genericMethod.Invoke(helper, new object[] { context, entity }) as List<CypherProperty>;
                if (result != null)
                {
                    properties.AddRange(result.Select(prop => new CypherProperty
                    {
                        TypeName = prop.TypeName,
                        JsonName = prop.JsonName.ApplyCasing(context)
                    }));
                }
            }

            return properties;
        }


    }
}
