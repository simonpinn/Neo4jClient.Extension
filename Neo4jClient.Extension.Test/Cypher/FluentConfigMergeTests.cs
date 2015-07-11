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
    [TestFixture]
    public class FluentConfigMergeTests
    {
        public IGraphClient GetMockCypherClient()
        {
            var moqGraphClient = new Mock<IGraphClient>();
            var mockRawClient = moqGraphClient.As<IRawGraphClient>();
            return mockRawClient.Object;
        }

        public ICypherFluentQuery GetFluentQuery()
        {
           return new CypherFluentQuery(GetMockCypherClient());
        }

        [SetUp]
        public void TestSetup()
        {
            // arrange
            var fluentConfig = FluentConfig.Config()
            .With<Person>("SecretAgent")
            .Match(x => x.Id)
            .Merge(x => x.Id)
            .MergeOnCreate(p => p.Title)
            .MergeOnCreate(p => p.Name)
            .MergeOnCreate(p => p.DateCreated)
            .MergeOnCreate(p => p.IsOperative)
            .MergeOnCreate(p => p.Sex)
            .MergeOnCreate(p => p.SerialNumber)
            .MergeOnCreate(p => p.SpendingAuthorisation)
            //.MergeOnCreate(p => p.HomeAddress)
            .MergeOnMatch(p => p.Title)
            .MergeOnMatch(p => p.Name)
            .MergeOnMatch(p => p.DateCreated)
            .MergeOnMatch(p => p.IsOperative)
            .MergeOnMatch(p => p.Sex)
            .MergeOnMatch(p => p.SerialNumber)
            .MergeOnMatch(p => p.SpendingAuthorisation)
            //.MergeOnMatch(p => p.HomeAddress)
            .Set();

            fluentConfig = FluentConfig.Config()
            .With<Address>()
            .Match(a => a.Id)
            .Merge(a => a.Id)
            .MergeOnCreate(a => a.Street)
            .MergeOnCreate(a => a.Suburb)
            .MergeOnMatch(a => a.Street)
            .MergeOnMatch(a => a.Suburb)
            .Set();

            Assert.AreEqual(4, fluentConfig.Count);
        }

        [Test]
        public void FormattedCypherOneDeep()
        {
            var address = SampleDataFactory.GetWellKnownAddress(7);
            var q = GetFluentQuery().MergeEntity(address);
            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            Assert.AreEqual(@"MERGE (address:Address {id:{
  suburb: ""Fakeville"",
  street: ""7 Isis Street"",
  id: 7
}.id})
ON MATCH
SET address.suburb={
  suburb: ""Fakeville"",
  street: ""7 Isis Street"",
  id: 7
}.suburb,address.street={
  suburb: ""Fakeville"",
  street: ""7 Isis Street"",
  id: 7
}.street
ON CREATE
SET address.suburb={
  suburb: ""Fakeville"",
  street: ""7 Isis Street"",
  id: 7
}.suburb,address.street={
  suburb: ""Fakeville"",
  street: ""7 Isis Street"",
  id: 7
}.street", text);
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
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.id})
ON MATCH
SET person.spendingAuthorisation={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.spendingAuthorisation,person.serialNumber={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.serialNumber,person.sex={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.sex,person.isOperative={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.isOperative,person.dateCreated={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.dateCreated,person.name={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.name,person.title={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.title
ON CREATE
SET person.spendingAuthorisation={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.spendingAuthorisation,person.serialNumber={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.serialNumber,person.sex={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.sex,person.isOperative={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.isOperative,person.dateCreated={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.dateCreated,person.name={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.name,person.title={
  spendingAuthorisation: 100.23,
  serialNumber: 123456,
  sex: ""Male"",
  isOperative: true,
  dateCreated: ""2015-07-11T08:00:00+10:00"",
  name: ""Sterling Archer"",
  title: null,
  id: 7
}.title
MERGE (address:Address {id:{
  suburb: ""Fakeville"",
  street: ""200 Isis Street"",
  id: 200
}.id})
ON MATCH
SET address.suburb={
  suburb: ""Fakeville"",
  street: ""200 Isis Street"",
  id: 200
}.suburb,address.street={
  suburb: ""Fakeville"",
  street: ""200 Isis Street"",
  id: 200
}.street
ON CREATE
SET address.suburb={
  suburb: ""Fakeville"",
  street: ""200 Isis Street"",
  id: 200
}.suburb,address.street={
  suburb: ""Fakeville"",
  street: ""200 Isis Street"",
  id: 200
}.street
MERGE (person)-[personaddress:HOME_ADDRESS {dateEffective:{
  dateEffective: ""2011-01-10T09:00:00+11:00""
}.dateEffective}]->(address)", text);
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
