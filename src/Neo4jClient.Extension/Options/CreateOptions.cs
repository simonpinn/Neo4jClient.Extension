using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClient.Extension.Cypher
{
    public class CreateOptions : IOptionsBase
    {
        public string Identifier { get; set; }
        public string PreCql { get; set; }
        public string PostCql { get; set; }
        public List<CypherProperty> CreateOverride { get; set; }
    }
}
