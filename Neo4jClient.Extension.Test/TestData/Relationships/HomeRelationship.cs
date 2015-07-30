using System;
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
    public class HomeRelationship : BaseRelationship
    {
        public const string LabelName = "HOME_ADDRESS";
        public HomeRelationship(string from = null, string to = null)
            : base(from, to)
        {
        }

        public DateTimeOffset DateEffective { get; set; }
    }
}
