using System.Collections.Generic;

namespace Neo4jClient.Extension.Cypher
{
    public class CypherProperty
    {
        public string TypeName { get; set; }
        public string JsonName { get; set; }

        public override string ToString()
        {
            return string.Format("TypeName={0}, JsonName={1}", TypeName, JsonName);
        }
    }

    public class CypherPropertyComparer : IEqualityComparer<CypherProperty>
    {
        public bool Equals(CypherProperty x, CypherProperty y)
        {
            return x.JsonName == y.JsonName;
        }

        public int GetHashCode(CypherProperty obj)
        {
            return obj.JsonName.GetHashCode();
        }
    }
}
