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
        public void CreateWithUnusualType()
        {
            new FluentConfigCreateTests(RealQueryFactory)
                .CreateWithUnusualTypeAct()
                .ExecuteWithoutResults();
        }

        [Test]
        public void CreateComplex()
        {
            new FluentConfigCreateTests(RealQueryFactory)
                .CreateComplexAct()
                .ExecuteWithoutResults();
        }
    }
}
