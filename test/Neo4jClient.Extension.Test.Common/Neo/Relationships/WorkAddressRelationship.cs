using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;

namespace Neo4jClient.Extension.Test.TestEntities.Relationships
{
    [CypherLabel(Name = LabelName)]
    public class WorkAddressRelationship : BaseRelationship
    {
        public const string LabelName = "WORK_ADDRESS";
        public WorkAddressRelationship(string from = null, string to = null)
            : base(from, to)
        {
        }
    }
}
