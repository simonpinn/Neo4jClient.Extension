using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Cypher
{
    [TestFixture]
    public class FluentConfigTests
    {
        [Test]
        public void FluentMergeTest()
        {
            //setup

            //act
            var values = FluentConfig.Config()
                .With<SomeClass>("helloworld")
                .Match(x => x.Bar)
                .Merge(x => x.Bar)
                .Merge(x => x.Foo)
                .Merge(x => x.SomeString)
                .Set();

            //assert
            Assert.AreEqual(2, values.Count);

            Console.WriteLine(
                (new SomeClass() {SomeString = "hello"}).ToCypherString<SomeClass, CypherMatchAttribute>(
                    new CypherExtensionContext(),"pkey"));
        }
    }

    public class SomeClass
    {
        public string SomeString { get; set; }
        public int Foo { get; set; }
        public bool Bar { get; set; }
    }
}
