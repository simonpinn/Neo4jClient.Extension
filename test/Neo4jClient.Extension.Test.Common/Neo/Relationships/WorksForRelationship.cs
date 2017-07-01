using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;

namespace Neo4jClient.Extension.Test.Data.Neo.Relationships
{
    [CypherLabel(Name = LabelName)]
    public class WorksForRelationship : BaseRelationship
    {
        public const string LabelName = "WORKS_FOR";

        public WorksForRelationship(string role, string from = "person", string to = "organisation")
            : base(from, to)
        {
            Role = role;
        }

        public WorksForRelationship(string from = "person", string to = "address")
            : base(from, to)
        {
        }

        public string Role { get; set; }
    }
}
