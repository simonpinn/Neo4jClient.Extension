using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Cypher
{
    [TestFixture]
    public class CypherTypeItemHelperTests
    {
        [Test]
        public void AddKeyAttributeTest()
        {
            //setup
            var helper = new CypherTypeItemHelper();
            
            //act
            var key = helper.AddKeyAttribute<CypherModel, CypherMatchAttribute>(CypherExtension.DefaultExtensionContext, new CypherModel());

            //assert
            Assert.That(key.AttributeType, Is.EqualTo(typeof(CypherMatchAttribute)));
            Assert.That(key.Type, Is.EqualTo(typeof(CypherModel)));
        }

        [Test]
        public void PropertyForUsageTest()
        {
            //setup
            var helper = new CypherTypeItemHelper();

            //act
            var result = helper.PropertiesForPurpose<CypherModel, CypherMatchAttribute>(new CypherModel());

            //assert
            Assert.That(result[0].TypeName, Is.EqualTo("id"));
            Assert.That(result[0].JsonName, Is.EqualTo("id"));
        }

    }
}
