using System.Collections.Generic;

namespace Neo4jClient.Extension.Cypher
{
    public class MatchRelationshipOptions
    {
        public List<CypherProperty> MatchOverride { get; set; }

        public static MatchRelationshipOptions Create()
        {
            return new MatchRelationshipOptions();
        }
    }

    public static class MatchRelationshipOptionsExtensions
    {
        public static MatchRelationshipOptions WithProperties(this MatchRelationshipOptions target, List<CypherProperty> propertyOverride)
        {
            target.MatchOverride = propertyOverride;
            return target;
        }

        public static MatchRelationshipOptions WithNoProperties(this MatchRelationshipOptions target)
        {
            return WithProperties(target, new List<CypherProperty>());
        }
    }
}