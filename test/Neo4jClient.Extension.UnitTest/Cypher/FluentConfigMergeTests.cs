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
    public class FluentConfigMergeTests : FluentConfigBaseTest
    {
        public FluentConfigMergeTests()
        {
        }

        /// <summary>
        /// Ctor for Integration tests to use
        /// </summary>
        public FluentConfigMergeTests(Func<ICypherFluentQuery> seedQueryFactory)
        {
            UseQueryFactory(seedQueryFactory);
        }

        [Test]
        public void Fiddle()
        {
            var person = SampleDataFactory.GetWellKnownPerson(7);

            var text = GetFluentQuery()
                .Match("(p:Person)")
                .Where((Person p) => p.Id == 7)
                .Set("p.Age = {age}").WithParam("age", 25)
                .Set("p.Name = {name}").WithParam("name", "foo")
                .GetFormattedDebugText();

            Console.WriteLine(text);
        }


        [Test]
        public void OneDeep()
        {
            var q = OneDeepAct();
            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            Assert.AreEqual(@"MERGE (person:SecretAgent {id:{
  id: 7
}.id})
ON MATCH
SET person.spendingAuthorisation = 100.23
ON MATCH
SET person.serialNumber = 123456
ON MATCH
SET person.sex = ""Male""
ON MATCH
SET person.isOperative = true
ON MATCH
SET person.name = ""Sterling Archer""
ON MATCH
SET person.title = null
ON CREATE
SET person = {
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  name: ""Sterling Archer"",
  title: null,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  id: 7
}", text);
        }
        
        public ICypherFluentQuery OneDeepAct()
        {
            var person = SampleDataFactory.GetWellKnownPerson(7);
            var q = GetFluentQuery()
                        .MergeEntity(person);
            return q;
        }

        [Test]
        public void TwoDeep()
        {
            var q = TwoDeepAct();
            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            // assert
            Assert.AreEqual(@"MERGE (person:SecretAgent {id:{
  id: 7
}.id})
ON MATCH
SET person.spendingAuthorisation = 100.23
ON MATCH
SET person.serialNumber = 123456
ON MATCH
SET person.sex = ""Male""
ON MATCH
SET person.isOperative = true
ON MATCH
SET person.name = ""Sterling Archer""
ON MATCH
SET person.title = null
ON CREATE
SET person = {
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  name: ""Sterling Archer"",
  title: null,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  id: 7
}
MERGE (person)-[:HOME_ADDRESS]->(address)
ON MATCH
SET address.suburb = ""Fakeville""
ON MATCH
SET address.street = ""200 Isis Street""
ON CREATE
SET address = {
  suburb: ""Fakeville"",
  street: ""200 Isis Street""
}
MERGE (person)-[personaddress:HOME_ADDRESS]->(address)
ON MATCH
SET personaddress.dateEffective = ""2011-01-10T09:00:00+11:00""
ON CREATE
SET personaddress = {
  dateEffective: ""2011-01-10T09:00:00+11:00""
}", text);
        }

        
        public ICypherFluentQuery TwoDeepAct()
        {
            //setup
            var testPerson = SampleDataFactory.GetWellKnownPerson(7);

            var homeAddressRelationship = new HomeAddressRelationship("person", "address");

            // perhaps this would be modelled on the address node but serves to show how to attach relationship property
            homeAddressRelationship.DateEffective = DateTime.Parse("2011-01-10T08:00:00+10:00");

            //act
            var q = GetFluentQuery()
                .MergeEntity(testPerson)
                .MergeEntity(testPerson.HomeAddress, MergeOptions.Create("address").WithRelationship(homeAddressRelationship))
                .MergeRelationship(homeAddressRelationship);

            return q;
        }

        [Test]
        public void OneDeepMergeByRelationship()
        {
            var q = OneDeepMergeByRelationshipAct();
            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            Assert.AreEqual(@"MERGE (person:SecretAgent {id:{
  id: 7
}.id})
ON MATCH
SET person.spendingAuthorisation = 100.23
ON MATCH
SET person.serialNumber = 123456
ON MATCH
SET person.sex = ""Male""
ON MATCH
SET person.isOperative = true
ON MATCH
SET person.name = ""Sterling Archer""
ON MATCH
SET person.title = null
ON CREATE
SET person = {
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  name: ""Sterling Archer"",
  title: null,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  id: 7
}
MERGE (person)-[:HOME_ADDRESS]->(address)
ON MATCH
SET address.suburb = ""Fakeville""
ON MATCH
SET address.street = ""200 Isis Street""
ON CREATE
SET address = {
  suburb: ""Fakeville"",
  street: ""200 Isis Street""
}", text);

        }

        public ICypherFluentQuery OneDeepMergeByRelationshipAct()
        {
            //setup
            var testPerson = SampleDataFactory.GetWellKnownPerson(7);

            var homeAddressRelationship = new HomeAddressRelationship("person", "address");

            // perhaps this would be modelled on the address node but serves to show how to attach relationship property
            homeAddressRelationship.DateEffective = DateTime.Parse("2011-01-10T08:00:00+10:00");

            //act
            var q = GetFluentQuery()
                .MergeEntity(testPerson)
                .MergeEntity(testPerson.HomeAddress,
                    new MergeOptions { MergeViaRelationship = homeAddressRelationship, ParamKey = "address"});

            return q;
        }

        [Test]
        public void MatchCypher()
        {
            var testPerson = SampleDataFactory.GetWellKnownPerson(7);

            // act
            var cypherKey = testPerson.ToCypherString<Person, CypherMatchAttribute>(new CypherExtensionContext(), "pkey");
            Console.WriteLine(cypherKey);

            // assert
            Assert.AreEqual("pkey:SecretAgent {id:{pkeyMatchKey}.id}", cypherKey);
        }
    }
}
