using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Cypher;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Cypher
{
    [TestFixture]
    public abstract class FluentConfigBaseTest
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
            .MergeOnMatch(p => p.Title)
            .MergeOnMatch(p => p.Name)
            .MergeOnMatch(p => p.DateCreated)
            .MergeOnMatch(p => p.IsOperative)
            .MergeOnMatch(p => p.Sex)
            .MergeOnMatch(p => p.SerialNumber)
            .MergeOnMatch(p => p.SpendingAuthorisation)
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
    }
}
