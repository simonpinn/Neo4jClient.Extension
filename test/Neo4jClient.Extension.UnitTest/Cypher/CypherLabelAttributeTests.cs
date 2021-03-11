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

            Console.WriteLine(q.Query.QueryText);
            Assert.AreEqual("MERGE (multilabel:Multi:Label {Id:$multilabelMatchKey.Id})", q.Query.QueryText);
        }

        [Test]
        public void UsesSuppliedParamName_WhenMultipleLabelsAreSpecified()
        {
            var model = new MultiLabel { Id = 1 };
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var q = helper.Query.MergeEntity(model, "n");

            Console.WriteLine(q.Query.QueryText);
            Assert.AreEqual("MERGE (n:Multi:Label {Id:$nMatchKey.Id})", q.Query.QueryText);
        }

        [Test]
        public void HandlesLabelsWithSpaces()
        {
            var model = new MultiLabelWithSpace { Id = 1 };
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var q = helper.Query.MergeEntity(model);

            var text = q.Query.QueryText;
            Console.WriteLine(text);
            Assert.AreEqual("MERGE (multilabelwithspace:Multi:`Space Label` {Id:$multilabelwithspaceMatchKey.Id})", text);
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