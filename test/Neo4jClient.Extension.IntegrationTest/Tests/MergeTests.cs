using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Test.Cypher;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Integration.Tests
{
    public class MergeTests : IntegrationTest
    {
        [Test]
        public void MergePerson()
        {
            var person = SampleDataFactory.GetWellKnownPerson(1);

            CypherQuery
                .MergeEntity(person)
                .ExecuteWithoutResults();
        }
    }
}
