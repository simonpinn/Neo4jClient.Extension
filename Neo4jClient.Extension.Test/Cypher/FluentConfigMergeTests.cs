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
        [Test]
        public void FormattedCypherOneDeep()
        {
            var person = SampleDataFactory.GetWellKnownPerson(7);
            var q = GetFluentQuery().MergeEntity(person);
            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            Assert.AreEqual(@"MERGE (person:SecretAgent {id:{
  id: 7
}.id})
SET person = {
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}", text);
        }

        [Test]
        public void FormattedCypherTwoDeep()
        {
            //setup
            var testPerson = SampleDataFactory.GetWellKnownPerson(7);

            var homeAddressRelationship = new HomeRelationship("person", "address");

            // perhaps this would be modelled on the address node but serves to show how to attach relationship property
            homeAddressRelationship.DateEffective = DateTime.Parse("2011-01-10T08:00:00+10:00");

            //act
            var q = GetFluentQuery()
                .MergeEntity(testPerson)
                .MergeEntity(testPerson.HomeAddress)
                .MergeRelationship(homeAddressRelationship, homeAddressRelationship.UseProperties(r => r.DateEffective));
           
            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            // assert
            Assert.AreEqual(@"MERGE (person:SecretAgent {id:{
  id: 7
}.id})
SET person = {
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}
MERGE (address:Address {suburb:{
  suburb: ""Fakeville"",
  street: ""200 Isis Street""
}.suburb,street:{
  suburb: ""Fakeville"",
  street: ""200 Isis Street""
}.street})
SET address = {
  suburb: ""Fakeville"",
  street: ""200 Isis Street""
}
MERGE (person)-[personaddress:HOME_ADDRESS {dateEffective:{
  dateEffective: ""2011-01-10T09:00:00+11:00""
}.dateEffective}]->(address)
SET personaddress = {
  dateEffective: ""2011-01-10T09:00:00+11:00""
}", text);
        }

        [Test]
        public void PrimaryKey()
        {
            var testPerson = SampleDataFactory.GetWellKnownPerson(7);

            // act
            var cypherKey = testPerson.ToCypherString<Person, CypherMatchAttribute>(new CypherExtensionContext(), "pkey");
            Console.WriteLine(cypherKey);

            // assert
            Assert.AreEqual(cypherKey, "pkey:SecretAgent {id:{pkey}.id}");
        }
    }
}
