﻿using FluentAssertions;
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
            key.Should().BeEquivalentTo(new CypherTypeItem()
                { Type = typeof(CypherModel), AttributeType = typeof(CypherMatchAttribute) });
        }

        [Test]
        public void PropertyForUsageTest()
        {
            //setup
            var helper = new CypherTypeItemHelper();

            //act
            var result = helper.PropertiesForPurpose<CypherModel, CypherMatchAttribute>(new CypherModel());

            //assert
            result[0].TypeName.Should().Be("id");
            result[0].JsonName.Should().Be("id");
        }

    }
}
