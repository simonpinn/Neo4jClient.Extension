using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Test.Cypher;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Integration.Tests
{
    public class CreateTests : IntegrationTest
    {
        [Test]
        public async Task MatchTest()
        {
            var config = new FluentConfigCreateTests(RealQueryFactory);
            var person = SampleDataFactory.GetWellKnownPerson(7);
            var query = config.GetFluentQuery().MatchEntity(person, "person").Return<Person>("person");
            var text = query.GetFormattedDebugText();
            
            var my = await query.ResultsAsync;
        }
        
        [Test]
        public async Task CreateWithUnusualType()
        {
            await new FluentConfigCreateTests(RealQueryFactory)
                .CreateWithUnusualTypeAct()
                .ExecuteWithoutResultsAsync();
        }

        [Test]
        public async Task CreateComplex()
        {
            await new FluentConfigCreateTests(RealQueryFactory)
                .CreateComplexAct()
                .ExecuteWithoutResultsAsync();
        }
    }
}
