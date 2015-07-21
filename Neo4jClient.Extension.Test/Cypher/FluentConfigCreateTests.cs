using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Cypher
{
    public class FluentConfigCreateTests : FluentConfigBaseTest
    {
        [Test]
        public void Create()
        {
            var agent = SampleDataFactory.GetWellKnownPerson(7);
            
            var q =  GetFluentQuery()
                    .CreateEntity(agent, "a");

            //var q = GetFluentQuery().Create(address);
            var text = q.GetFormattedDebugText();
            //Console.WriteLine(text);

            Assert.AreEqual(@"CREATE (a:SecretAgent {
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null
})", text);
        }
    }
}
