using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Cypher
{
    [TestFixture]
    public class FluentConfigTests
    {
        public IGraphClient GetMockCypherClient()
        {
            var moqGraphClient = new Mock<IGraphClient>();
            var mockRawClient = moqGraphClient.As<IRawGraphClient>();
            return mockRawClient.Object;
        }
        

        [SetUp]
        public void TestSetup()
        {
            // arrange
            var fluentConfig = FluentConfig.Config()
            .With<Person>("SecretAgent")
            .Match(x => x.Id)
            .Merge(x => x.Id)
            .MergeOnCreate(p => p.Name)
            .MergeOnMatch(p => p.Name)
            .Set();

            Assert.AreEqual(4, fluentConfig.Count);
        }

        [Test]
        public void FluentMergeFormattedCypher()
        {
            //setup
            var testPerson = SampleDataFactory.GetWellKnownPerson(7);

            var fq = new CypherFluentQuery(GetMockCypherClient());

            //act
            var q = fq.MergeEntity(testPerson);
            var text = q.GetFormattedDebugText();
            Console.WriteLine(text);

            // assert
            Assert.AreEqual(@"MERGE (person:SecretAgent {id:{
  name: ""Sterling Archer"",
  id: 7
}.id})
ON MATCH
SET person.name={
  name: ""Sterling Archer"",
  id: 7
}.name
ON CREATE
SET person.name={
  name: ""Sterling Archer"",
  id: 7
}.name", text);
        }

        [Test]
        public void FluentMergePrimaryKey()
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
