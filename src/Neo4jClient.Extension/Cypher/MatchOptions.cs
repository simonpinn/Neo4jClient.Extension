using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClient.Extension.Cypher
{
    public class MatchOptions
    {
        public string Identifier { get; set; }

        public string PreCql { get; set; }

        public string PostCql { get; set; }

        public List<CypherProperty> MatchOverride { get; set; }

        public MatchOptions()
        {
            MatchOverride = null;
        }

        public static MatchOptions Create(string identifier)
        {
            return new MatchOptions {Identifier = identifier};
        }
    }

    public static class MatchOptionExtensions
    {
        public static MatchOptions WithProperties(this MatchOptions target, List<CypherProperty> propertyOverride)
        {
            target.MatchOverride = propertyOverride;
            return target;
        }

        public static MatchOptions WithNoProperties(this MatchOptions target)
        {
            return WithProperties(target, new List<CypherProperty>());
        }
    }
}
