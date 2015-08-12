using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Cypher
{
    public class FluentConfigUpdateTests : FluentConfigBaseTest
    {
        /// <summary>
        /// Demonstrates 
        /// 1) we dont have expression tree support for SET
        /// 2) Bug in neo4jclient(?) where Id property is not lower case in generated cypher so match fails. When bug fixed, this test will start failing
        /// </summary>
        [Test]
        public void IncrementAValue_ExpressionTreeNotAvailable()
        {
            var cypherText = GetFluentQuery()
                .Match("(p:SecretAgent)")
                .Where((Person p) => p.Id == 7)
                .Set("p.serialNumber = p.serialNumber + 1")
                .GetFormattedDebugText();

            Console.WriteLine(cypherText);

            Assert.AreEqual(@"MATCH (p:SecretAgent)
WHERE (p.Id = 7)
SET p.serialNumber = p.serialNumber + 1", cypherText);
        }
    }
}
