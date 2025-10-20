using System.Collections.Generic;

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
