using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;
using Neo4jClient.Extension.Test.TestEntities.Relationships;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Cypher
{
    public class FluentConfigMatchTests : FluentConfigBaseTest
    {

        [Test]
        public void MatchEntity()
        {
            var person = SampleDataFactory.GetWellKnownPerson(7);
            var q = GetFluentQuery()
                    .MatchEntity(person);
            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            Assert.AreEqual(@"MATCH (person:SecretAgent {
  id: 7
})", text);
        }
        
    }
}
