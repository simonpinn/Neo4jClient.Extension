using System;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;

namespace Neo4jClient.Extension.Test.TestEntities.Relationships
{
    [CypherLabel(Name = LabelName)]
    public class HomeAddressRelationship : BaseRelationship
    {
        public const string LabelName = "HOME_ADDRESS";
        public HomeAddressRelationship(DateTimeOffset effective, string from = "agent", string to = "address")
            : base(from, to)
        {
            DateEffective = effective;
        }

        public HomeAddressRelationship(string from = "person", string to = "address")
            : base(from, to)
        {
        }

        public HomeAddressRelationship(string relationshipIdentifier, string from, string to)
          : base(relationshipIdentifier, from, to)
        {
        }

        public DateTimeOffset DateEffective { get; set; }
    }
}