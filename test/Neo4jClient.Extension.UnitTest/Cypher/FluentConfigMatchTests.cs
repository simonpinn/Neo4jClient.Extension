using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;
using Neo4jClient.Extension.Test.TestData.Relationships;
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

            Assert.AreEqual(@"MATCH (person:SecretAgent {id:{
  id: 7
}.id})", text);
        }

        [Test]
        public void OptionalMatchEntity()
        {
            var person = SampleDataFactory.GetWellKnownPerson(7);
            var q = GetFluentQuery()
                .MatchEntity(person)
                .OptionalMatchEntity(person.HomeAddress, MatchOptions.Create("ha").WithNoProperties());
            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            Assert.AreEqual(@"MATCH (person:SecretAgent {id:{
  id: 7
}.id})
OPTIONAL MATCH (ha:Address)", text);
        }

        [Test]
        public void OptionalMatchRelationship()
        {
            var person = SampleDataFactory.GetWellKnownPerson(7);
            var homeAddressRelationship = new HomeAddressRelationship();
            var q = GetFluentQuery()
                .MatchEntity(person)
                .OptionalMatchRelationship(homeAddressRelationship, MatchRelationshipOptions.Create().WithNoProperties());
            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            Assert.AreEqual(@"MATCH (person:SecretAgent {id:{
  id: 7
}.id})
OPTIONAL MATCH (person)-[personaddress:HOME_ADDRESS]->(address)", text);
        }

        [Test] 
        public void MatchRelationshipSimple()
        {
            var addressRelationship = new CheckedOutRelationship();
            var q = GetFluentQuery()
                    .MatchRelationship(addressRelationship);
            var text = q.GetFormattedDebugText();

            Console.WriteLine(text);

            Assert.AreEqual(@"MATCH (agent)-[agentweapon:HAS_CHECKED_OUT]->(weapon)", text);
        }

        [Test]
        public void MatchRelationshipWithProperty()
        {
            var addressRelationship = new HomeAddressRelationship(DateTimeOffset.Parse("2015-08-05 12:00"), "agent", "homeAddress");
            var q = GetFluentQuery()
                    .MatchRelationship(addressRelationship);
            var text = q.GetFormattedDebugText();

            Console.WriteLine(text);

            Assert.AreEqual(@"MATCH (agent)-[agenthomeAddress:HOME_ADDRESS {dateEffective:{agenthomeAddressMatchKey}.dateEffective}]->(homeAddress)", text);
        }
    }
}
