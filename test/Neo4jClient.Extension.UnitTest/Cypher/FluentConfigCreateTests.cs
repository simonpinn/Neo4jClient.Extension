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
        public FluentConfigCreateTests()
        {
            
        }

        /// <summary>
        /// Ctor for Integration tests to use
        /// </summary>
        public FluentConfigCreateTests(Func<ICypherFluentQuery> seedQueryFactory)
        {
            UseQueryFactory(seedQueryFactory);
        }

       
        public ICypherFluentQuery CreateWithUnusualTypeAct()
        {
            var weapon = SampleDataFactory.GetWellKnownWeapon(1);

            var q = GetFluentQuery()
                .CreateEntity(weapon, "w");
            
            return q;
        }

        [Test]
        public void CreateWithUnusualType()
        {
            var q = CreateWithUnusualTypeAct();
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
            Assert.AreEqual(@"CREATE (address:Address)
SET address = $address_CreateParams", text);
        }

        [Test]
        public void CreateRelationshipWithNoIdentifier()
        {
            var homeRelationship = new HomeAddressRelationship(string.Empty, "a", "ha");

            var q = GetFluentQuery()
                .CreateRelationship(homeRelationship);
            
            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            Assert.AreEqual("CREATE (a)-[:HOMEADDRESSRELATIONSHIP]->(ha) SET  = $_RelationshipParams\r\nSET  = $_CreateParams", text);
        }


        [Test]
        public void CreateComplex()
        {
            var q = CreateComplexAct();

            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            Assert.AreEqual(@"CREATE (a:SecretAgent)
SET a = $a_CreateParams
CREATE (ha:Address)
SET ha = $ha_CreateParams
CREATE (wa:Address)
SET wa = $wa_CreateParams
CREATE (a)-[myHomeRelationshipIdentifier:HOMEADDRESSRELATIONSHIP]->(ha) SET myHomeRelationshipIdentifier = $myHomeRelationshipIdentifier_RelationshipParams
SET myHomeRelationshipIdentifier = $myHomeRelationshipIdentifier_CreateParams
CREATE (a)-[awa:WORKADDRESSRELATIONSHIP]->(wa) SET awa = $awa_RelationshipParams", text);
        }
        
        public ICypherFluentQuery CreateComplexAct()
        {
            var agent = SampleDataFactory.GetWellKnownPerson(7);
            var homeRelationship = new HomeAddressRelationship("myHomeRelationshipIdentifier", "a", "ha");
            homeRelationship.DateEffective = DateTimeOffset.Parse("2015-08-05 12:00+00:00");

            var q = GetFluentQuery()
                .CreateEntity(agent, "a")
                .CreateEntity(agent.HomeAddress, "ha")
                .CreateEntity(agent.WorkAddress, "wa")
                .CreateRelationship(homeRelationship)
                .CreateRelationship(new WorkAddressRelationship("a", "wa"));
            
            return q;
        }
    }
}
