using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;

namespace Neo4jClient.Extension.Test.TestData.Relationships
{
    [CypherLabel(Name = "HAS_CHECKED_OUT")]
    public class CheckedOutRelationship : BaseRelationship
    {
        public CheckedOutRelationship() : base ("agent", "weapon")
        {
            
        }
    }
}
