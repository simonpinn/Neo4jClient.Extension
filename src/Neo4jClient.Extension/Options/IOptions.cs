using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClient.Extension.Cypher
{
    public interface IOptionsBase
    {
        string PreCql { get; set; }
        string PostCql { get; set; }
    }
}
