using System;

namespace Neo4jClient.Extension.Cypher
{
    public class CypherTypeItem : IEquatable<CypherTypeItem>
    {
        public Type Type { get; set; }
        public Type AttributeType { get; set; }

        public bool Equals(CypherTypeItem other)
        {
            return other.Type == Type && other.AttributeType == AttributeType;
        }

        bool IEquatable<CypherTypeItem>.Equals(CypherTypeItem other)
        {
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ AttributeType.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Type={0}, AttributeType={1}", Type.Name, AttributeType.Name);
        }
    }
}
