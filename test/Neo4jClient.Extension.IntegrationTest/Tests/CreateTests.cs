using System.Threading.Tasks;
using Neo4jClient.Extension.Test.Cypher;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Integration.Tests
{
    public class CreateTests : IntegrationTest
    {
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
