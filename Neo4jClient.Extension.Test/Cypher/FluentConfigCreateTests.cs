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
        public void CreateWithUnusualType()
        {

            var weapon = SampleDataFactory.GetWellKnownWeapon(1);

            var q = GetFluentQuery()
                .CreateEntity(weapon, "w");

            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);
        }

        [Test]
        public void CreateWithNullValues()
        {
            var agent = SampleDataFactory.GetWellKnownPerson(7);

            agent.HomeAddress.Suburb = null;

            var q = GetFluentQuery()
           .CreateEntity(agent, "a")
           .CreateEntity(agent.HomeAddress, "ha")
           .CreateEntity(agent.WorkAddress, "wa")
           .Create("(a)-[rha:HOME_ADDRESS]->(ha)")
           .Create("(a)-[wha:WORK_ADDRESS]->(wa)");


            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);
        }

        [Test]
        public void Create()
        {
            var agent = SampleDataFactory.GetWellKnownPerson(7);

            var q = GetFluentQuery()
                .CreateEntity(agent, "a")
                .CreateEntity(agent.HomeAddress, "ha")
                .CreateEntity(agent.WorkAddress, "wa")
                .Create("(a)-[rha:HOME_ADDRESS]->(ha)")
                .Create("(a)-[wha:WORK_ADDRESS]->(wa)");

            //var q = GetFluentQuery().Create(address);
            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            Assert.AreEqual(@"CREATE (a:SecretAgent {
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
})
CREATE (ha:Address {
  suburb: ""Fakeville"",
  street: ""200 Isis Street""
})
CREATE (wa:Address {
  suburb: ""Fakeville"",
  street: ""59 Isis Street""
})
CREATE (a)-[rha:HOME_ADDRESS]->(ha)
CREATE (a)-[wha:WORK_ADDRESS]->(wa)", text);
        }
    }
}
