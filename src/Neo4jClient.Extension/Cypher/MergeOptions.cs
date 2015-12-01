using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClient.Extension.Cypher
{
    public class MergeOptions
    {
        public string Identifier { get; set; }

        public List<CypherProperty> MergeOverride { get; set; }

        public List<CypherProperty> OnMatchOverride { get; set; }

        public List<CypherProperty> OnCreateOverride { get; set; }

        public string PreCql { get; set; }

        public string PostCql { get; set; }

        /// <summary>
        /// Merge the entity via a relationship
        /// </summary>
        public BaseRelationship MergeViaRelationship { get; set; }
        
        public MergeOptions()
        {
            MergeOverride = null;
            OnMatchOverride = null;
            OnCreateOverride = null;
        }

        /// <summary>
        /// For overriding the default identifier configured via FluentConfig
        /// </summary>
        public static MergeOptions WithIdentifier(string identifier)
        {
            return new MergeOptions { Identifier = identifier };
        }

        /// <summary>
        /// For when merging against a node that is matched via a relationsip
        /// </summary>
        public static MergeOptions ViaRelationship(BaseRelationship relationship)
        {
            var options = new MergeOptions();
            options.Identifier = relationship.ToKey;
            options.MergeViaRelationship = relationship;
            return options;
        }
    }

    public static class MergeOptionExtensions
    {
        public static MergeOptions WithMergeProperties(this MergeOptions target, List<CypherProperty> mergeOverride)
        {
            target.MergeOverride = mergeOverride;
            return target;
        }

        public static MergeOptions WithNoMergeProperties(this MergeOptions target)
        {
            return WithMergeProperties(target, new List<CypherProperty>());
        }
    }

}
