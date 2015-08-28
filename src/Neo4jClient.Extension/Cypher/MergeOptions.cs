using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClient.Extension.Cypher
{
    public class MergeOptions
    {
        public string ParamKey { get; set; }

        public List<CypherProperty> MergeOverride { get; set; }

        public List<CypherProperty> OnMatchOverride { get; set; }

        public List<CypherProperty> OnCreateOverride { get; set; }

        public string PreCql { get; set; }

        public string PostCql { get; set; }

        public BaseRelationship MergeViaRelationship { get; set; }

        public bool UseToLabel { get; set; }

        public static MergeOptions Create(string paramKey)
        {
            return new MergeOptions { ParamKey = paramKey };

        }
        public MergeOptions()
        {
            UseToLabel = true;
            MergeOverride = null;
            OnMatchOverride = null;
            OnCreateOverride = null;
        }
    }

    public static class MergeOptionExtensions
    {
        public static MergeOptions WithProperties(this MergeOptions target, List<CypherProperty> mergeOverride)
        {
            target.MergeOverride = mergeOverride;
            return target;
        }

        public static MergeOptions WithNoProperties(this MergeOptions target)
        {
            return WithProperties(target, new List<CypherProperty>());
        }

        public static MergeOptions WithRelationship(this MergeOptions target, BaseRelationship relationship)
        {
            target.MergeViaRelationship = relationship;
            return target;
        }

        public static MergeOptions UseToLabel(this MergeOptions target, bool useToLabel)
        {
            target.UseToLabel = useToLabel;
            return target;
        }
    }

}
