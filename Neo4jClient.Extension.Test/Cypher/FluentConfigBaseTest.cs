using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Test.TestData.Entities;
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
            FluentConfig.Config()
            .With<Person>("SecretAgent")
            .Match(x => x.Id)
            .Merge(x => x.Id)
            .MergeOnCreate(p => p.Id)
            .MergeOnMatchOrCreate(p => p.Title)
            .MergeOnMatchOrCreate(p => p.Name)
            .MergeOnMatchOrCreate(p => p.DateCreated)
            .MergeOnMatchOrCreate(p => p.IsOperative)
            .MergeOnMatchOrCreate(p => p.Sex)
            .MergeOnMatchOrCreate(p => p.SerialNumber)
            .MergeOnMatchOrCreate(p => p.SpendingAuthorisation)
            .Set();

            FluentConfig.Config()
            .With<Address>()
            .MergeOnMatchOrCreate(a => a.Street)
            .MergeOnMatchOrCreate(a => a.Suburb)
            .Set();

            FluentConfig.Config()
                .With<Weapon>()
                .Match(x => x.Id)
                .Merge(x => x.Id)
                .MergeOnMatchOrCreate(w => w.Name)
                .MergeOnMatchOrCreate(w => w.BlastRadius)
                .Set();

        }
    }
}
