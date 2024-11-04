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
        public async Task CreateWithUnusualType()
        {
            await new FluentConfigCreateTests(RealQueryFactory)
                .CreateWithUnusualTypeAct()
                .ExecuteWithoutResultsAsync();
        }

        [Test]
        public async Task CreateComplex()
        {
            try
            {
                await new FluentConfigCreateTests(RealQueryFactory)
                    .CreateComplexAct()
                    .ExecuteWithoutResultsAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            
        }
    }
}
