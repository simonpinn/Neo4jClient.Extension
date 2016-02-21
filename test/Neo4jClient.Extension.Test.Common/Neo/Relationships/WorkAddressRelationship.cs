using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;
using Neo4jClient.Extension.Test.Cypher;

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
