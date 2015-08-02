using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;
using Neo4jClient.Extension.Test.TestEntities.Relationships;
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
            // GetFormattedDebugText isn't honouring JsonConverter
        }

        /// <summary>
        /// work around exception somewhere in neo4jclent when creating null values even though cypher syntax is valid
        /// </summary>
        [Test]
        public void CreateWithNullValuesSkipsTheNulls()
        {
            var agent = SampleDataFactory.GetWellKnownPerson(7);

            agent.HomeAddress.Suburb = null;
            
            var q = GetFluentQuery()
                .CreateEntity(agent.HomeAddress);
            
            var text = q.GetFormattedDebugText();
            Assert.AreEqual(@"CREATE (address:Address {
  street: ""200 Isis Street""
})", text);
        }

        [Test]
        public void Create()
        {
            var agent = SampleDataFactory.GetWellKnownPerson(7);

            var q = GetFluentQuery()
                .CreateEntity(agent, "a")
                .CreateEntity(agent.HomeAddress, "ha")
                .CreateEntity(agent.WorkAddress, "wa")
                .CreateRelationship(new HomeAddressRelationship("a", "ha"))
                .CreateRelationship(new WorkAddressRelationship("a", "wa"));

            //var q = GetFluentQuery().Create(address);
            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            Assert.AreEqual(@"CREATE (a:SecretAgent {
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  name: ""Sterling Archer"",
  dateCreated: ""2015-07-11T08:00:00+10:00"",
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
CREATE (a)-[aha:HOME_ADDRESS]->(ha)
CREATE (a)-[awa:WORK_ADDRESS]->(wa)", text);
        }
    }
}
