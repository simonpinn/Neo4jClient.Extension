using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClient.Extension.Cypher
{
    public static partial class CypherExtension
    {
        internal static void AddConfigProperties(CypherTypeItem type, List<CypherProperty> properties)
        {
            CypherTypeItemHelper.AddPropertyUsage(type, properties);
        }
        internal static void SetConfigLabel(Type type, string label)
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
