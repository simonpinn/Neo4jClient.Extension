using System;

namespace Neo4jClient.Extension.Test.Cypher
{
    using Neo4jClient.Extension.Cypher;
    using Neo4jClient.Extension.Cypher.Attributes;
    using NUnit.Framework;

    [TestFixture]
    public class CypherLabelAttributeTests
    {
        [Test]
        public void UsesClassName_WhenMultipleLabelsAreSpecified()
        {
            var model = new MultiLabel { Id = 1 };
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var q = helper.Query.MergeEntity(model);

            Assert.AreEqual("MERGE (multilabel:Multi:Label {multilabelMatchKey})", q.Query.QueryText);
        }

        [Test]
        public void UsesSuppliedParamName_WhenMultipleLabelsAreSpecified()
        {
            var model = new MultiLabel { Id = 1 };
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var q = helper.Query.MergeEntity(model, "n");

            Assert.AreEqual("MERGE (n:Multi:Label {nMatchKey})", q.Query.QueryText);
        }

        [Test]
        public void HandlesLabelsWithSpaces()
        {
            var model = new MultiLabelWithSpace { Id = 1 };
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var q = helper.Query.MergeEntity(model);

            var text = q.Query.QueryText;
            Console.WriteLine(text);
            Assert.AreEqual("MERGE (multilabelwithspace:Multi:`Space Label` {multilabelwithspaceMatchKey})", text);
        }

        public abstract class MultiBase
        {
            [CypherMerge]
            public int Id { get; set; }
        }

        [CypherLabel(Name = "Multi:`Space Label`")]
        public class MultiLabelWithSpace : MultiBase {}

        [CypherLabel(Name = "Multi:Label")]
        public class MultiLabel : MultiBase {}
    }
}