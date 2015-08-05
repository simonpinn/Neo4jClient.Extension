using System;
using System.Collections.Generic;
using Moq;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Cypher
{
    public class CypherExtensionTestHelper
    {

        public Mock<IRawGraphClient> GraphClient { get; private set; }
        public CypherExtensionContext CypherExtensionContext { get; private set; }
        public CypherFluentQuery Query { get; private set; }

        public CypherExtensionTestHelper()
        {
            CypherExtensionContext = new CypherExtensionContext();
        }

        public CypherExtensionTestHelper SetupGraphClient()
        {
            GraphClient = new Mock<IRawGraphClient>();
            GraphClient.Setup(x => x.JsonContractResolver).Returns(new DefaultContractResolver());
            Query = new CypherFluentQuery(GraphClient.Object);
            return this;
        }
    }

    [TestFixture]
    public class CypherExtensionTests
    {
        [Test]
        public void ToCypherStringMergeTest()
        {
            //setup
            var model = CreateModel();
            var helper = new CypherExtensionTestHelper();

            //act
            var result = model.ToCypherString<CypherModel, CypherMergeAttribute>(helper.CypherExtensionContext);
            var result2 = model.ToCypherString<CypherModel, CypherMergeAttribute>(helper.CypherExtensionContext);

            //assert
            Assert.AreEqual("cyphermodel:CypherModel {cyphermodelMatchKey}", result);
            Assert.AreEqual(result,result2);
        }

        [Test]
        public void MatchEntityTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = CreateModel();
            model.id = Guid.Parse("9aa1343f-18a4-41a6-a414-34b7df62c919");
            //act
            var q = helper.Query.MatchEntity(model).Return(cyphermodel => cyphermodel.As<CypherModel>());
            
            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MATCH (cyphermodel:CypherModel {cyphermodelMatchKey})\r\nRETURN cyphermodel", q.Query.QueryText);
        }

        [Test]
        public void MatchEntityOverrideTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = CreateModel();
            
            //act
            var q = helper.Query
                            .MatchEntity(model, propertyOverride: model.UseProperties(x => x.firstName, x => x.isLegend))
                            .Return(cyphermodel => cyphermodel.As<CypherModel>());

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual(@"MATCH (cyphermodel:CypherModel {
  firstName: ""Foo"",
  isLegend: false
})
RETURN cyphermodel", q.GetFormattedDebugText());
        }

        [Test]
        public void MatchEntityKeyTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = CreateModel();
            
            //act
            var q = helper.Query.MatchEntity(model,"key").Return(cyphermodel => cyphermodel.As<CypherModel>());

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MATCH (key:CypherModel {keyMatchKey})\r\nRETURN cyphermodel", q.Query.QueryText);
        }

        [Test]
        public void MatchEntityPreTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = CreateModel();
            

            //act
            var q = helper.Query.MatchEntity(model, preCql: "(a:Node)-->").Return(cyphermodel => cyphermodel.As<CypherModel>());

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual(@"MATCH (a:Node)-->(cyphermodel:CypherModel {
  id: ""b00b7355-ce53-49f2-a421-deadb655673d""
})
RETURN cyphermodel", q.GetFormattedDebugText());
        }

        [Test]
        public void MatchEntityPostTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = CreateModel();

            //act
            var q = helper.Query.MatchEntity(model, postCql: "<--(a:Node)").Return(cyphermodel => cyphermodel.As<CypherModel>());

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MATCH (cyphermodel:CypherModel {cyphermodelMatchKey})<--(a:Node)\r\nRETURN cyphermodel", q.Query.QueryText);
        }

        [Test]
        public void MatchEntityPrePostKeyOverrideTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = CreateModel();

            //act
            var q = helper.Query
                        .MatchEntity(model, "key",  "(a:Node)-->", "<--(b:Node)", new List<CypherProperty>())
                        .Return(cyphermodel => cyphermodel.As<CypherModel>());

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual(@"MATCH (a:Node)-->(key:CypherModel {})<--(b:Node)
RETURN cyphermodel", q.GetFormattedDebugText());
        }

        [Test]
        public void MatchAllTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            //act
            var result = helper.Query.MatchEntity(new CypherModel(), propertyOverride: new List<CypherProperty>());

            //assert
            Assert.AreEqual("MATCH (cyphermodel:CypherModel {})", result.GetFormattedDebugText());
        }

        [Test]
        public void MergeEntityTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();
            var model = CreateModel();

            //act
            var q = helper.Query.MergeEntity(model);

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (cyphermodel:CypherModel {cyphermodelMatchKey})\r\nON MATCH\r\nSET cyphermodel.isLegend = {cyphermodelisLegend}\r\nON MATCH\r\nSET cyphermodel.answerToTheMeaningOfLifeAndEverything = {cyphermodelanswerToTheMeaningOfLifeAndEverything}\r\nON CREATE\r\nSET cyphermodel = {cyphermodelOnCreate}", q.Query.QueryText);
        }

        [Test]
        public void MergeEntityKeyTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();
            var model = CreateModel();

            //act
            var q = helper.Query.MergeEntity(model,"key");

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual(@"MERGE (key:CypherModel {keyMatchKey})
ON MATCH
SET key.isLegend = {keyisLegend}
ON MATCH
SET key.answerToTheMeaningOfLifeAndEverything = {keyanswerToTheMeaningOfLifeAndEverything}
ON CREATE
SET key = {keyOnCreate}", q.Query.QueryText);
        }

        [Test]
        public void MergeEntityOverrideMergeTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();
            var model = CreateModel();

            //act
            var q = helper.Query.MergeEntity(model, mergeOverride:model.UseProperties(x => x.firstName));

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual(@"MERGE (cyphermodel:CypherModel {
  firstName: ""Foo""
})
ON MATCH
SET cyphermodel.isLegend = false
ON MATCH
SET cyphermodel.answerToTheMeaningOfLifeAndEverything = 42
ON CREATE
SET cyphermodel = {
  firstName: ""Foo"",
  dateOfBirth: ""1981-04-01T00:00:00+10:00"",
  isLegend: false,
  answerToTheMeaningOfLifeAndEverything: 42
}", q.GetFormattedDebugText());
        }

        [Test]
        public void MergeEntityOverrideOnMatchTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();
            var model = CreateModel();

            //act
            var q = helper.Query.MergeEntity(model, onMatchOverride: model.UseProperties(x => x.firstName));

            Console.WriteLine(q.Query.QueryText);

            //assert
            Assert.AreEqual(@"MERGE (cyphermodel:CypherModel {cyphermodelMatchKey})
ON MATCH
SET cyphermodel.firstName = {cyphermodelfirstName}
ON CREATE
SET cyphermodel = {cyphermodelOnCreate}", q.Query.QueryText);
        }

        [Test]
        public void MergeEntityOverrideOnCreateTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();
            var model = CreateModel();

            //act
            var q = helper.Query.MergeEntity(model, onCreateOverride: model.UseProperties(x => x.firstName));

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (cyphermodel:CypherModel {cyphermodelMatchKey})\r\nON MATCH\r\nSET cyphermodel.isLegend = {cyphermodelisLegend}\r\nON MATCH\r\nSET cyphermodel.answerToTheMeaningOfLifeAndEverything = {cyphermodelanswerToTheMeaningOfLifeAndEverything}\r\nON CREATE\r\nSET cyphermodel = {cyphermodelOnCreate}", q.Query.QueryText);
        }

        [Test]
        public void MergeEntityAllArgsTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();
            var model = CreateModel();

            //act
            var q = helper.Query.MergeEntity(model,"key", new List<CypherProperty>(),new List<CypherProperty>(), new List<CypherProperty>(), "(a:Node)-->","<--(b:Node)");

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (a:Node)-->(key:CypherModel {})<--(b:Node)", q.GetFormattedDebugText());
        }


        [Test]
        public void MergeRelationshipTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = new ComponentOf("from", "to");
            
            //act
            var q = helper.Query.MergeRelationship(model);

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual(@"MERGE (from)-[fromto:COMPONENT_OF {
  quantity: 0.0,
  unitOfMeasure: ""Gram"",
  factor: 0,
  instructionText: """"
}]->(to)
ON MATCH
SET fromto.quantity = 0.0
ON MATCH
SET fromto.unitOfMeasure = ""Gram""
ON MATCH
SET fromto.factor = 0
ON MATCH
SET fromto.instructionText = """"", q.GetFormattedDebugText());
        }

        [Test]
        public void MergeRelationshipDownCastTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = (BaseRelationship) new ComponentOf("from", "to");

            //act
            var q = helper.Query.MergeRelationship(model);

            Console.WriteLine(q.GetFormattedDebugText());
            
            //assert
            Assert.AreEqual(@"MERGE (from)-[fromto:COMPONENT_OF {
  quantity: 0.0,
  unitOfMeasure: ""Gram"",
  factor: 0,
  instructionText: """"
}]->(to)
ON MATCH
SET fromto.quantity = 0.0
ON MATCH
SET fromto.unitOfMeasure = ""Gram""
ON MATCH
SET fromto.factor = 0
ON MATCH
SET fromto.instructionText = """"", q.GetFormattedDebugText());
        }

        [Test]
        public void MergeRelationshipMergeOverrideTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = new ComponentOf("from", "to");

            //act
            var q = helper.Query.MergeRelationship(model, model.UseProperties(x => x.quantity));

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual(@"MERGE (from)-[fromto:COMPONENT_OF {
  quantity: 0.0
}]->(to)
ON MATCH
SET fromto.quantity = 0.0
ON MATCH
SET fromto.unitOfMeasure = ""Gram""
ON MATCH
SET fromto.factor = 0
ON MATCH
SET fromto.instructionText = """"", q.GetFormattedDebugText());
        }

        [Test]
        public void MergeRelationshipOnMatchOverrideTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = new ComponentOf("from", "to");

            //act
            var q = helper.Query.MergeRelationship(model,onMatchOverride:model.UseProperties(x => x.quantity));

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual(@"MERGE (from)-[fromto:COMPONENT_OF {
  quantity: 0.0,
  unitOfMeasure: ""Gram"",
  factor: 0,
  instructionText: """"
}]->(to)
ON MATCH
SET fromto.quantity = 0.0", q.GetFormattedDebugText());
        }

        [Test]
        public void MergeRelationshipOnCreateOverrideTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = new ComponentOf("from", "to");

            //act
            var q = helper.Query.MergeRelationship(model, onCreateOverride: model.UseProperties(x => x.quantity));

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual(@"MERGE (from)-[fromto:COMPONENT_OF {
  quantity: 0.0,
  unitOfMeasure: ""Gram"",
  factor: 0,
  instructionText: """"
}]->(to)
ON MATCH
SET fromto.quantity = 0.0
ON MATCH
SET fromto.unitOfMeasure = ""Gram""
ON MATCH
SET fromto.factor = 0
ON MATCH
SET fromto.instructionText = """"
ON CREATE
SET fromto = {
  quantity: 0.0
}", q.GetFormattedDebugText());
        }

        [Test]
        public void MergeRelationshipAllArgsTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = new ComponentOf("from", "to");

            //act
            var q = helper.Query.MergeRelationship(model, new List<CypherProperty>(), new List<CypherProperty>(), new List<CypherProperty>());

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (from)-[fromto:COMPONENT_OF {}]->(to)", q.GetFormattedDebugText());
        }

        [Test]
        public void EntityLabelWithoutAttrTest()
        {
            //setup
            var model = CreateModel();

            //act
            var result = model.EntityLabel();

            //assert
            Assert.AreEqual("CypherModel", result);
        }

        [Test]
        public void EntityLabelWithTest()
        {
            //setup
            var model = new LabelledModel();

            //act
            var result = model.EntityLabel();

            //assert
            Assert.AreEqual("MyName", result);
        }

        private CypherModel CreateModel()
        {
            var model = new CypherModel
            {
                dateOfBirth = new DateTime(1981, 4, 1),
                answerToTheMeaningOfLifeAndEverything = 42,
                firstName = "Foo",
                isLegend = false
            };

            model.id = Guid.Parse("b00b7355-ce53-49f2-a421-deadb655673d");

            return model;
        }

        public enum UnitsOfMeasure
        {
            Gram,
            Millimeter,
            Cup,
            TableSpoon,
            TeaSpoon,
            Unit
        }

        [CypherLabel(Name = "COMPONENT_OF")]
        public class ComponentOf : BaseRelationship
        {
            public ComponentOf(string from = null, string to = null): base(from, to)
            {
                instructionText = string.Empty;
            }
            [CypherMerge]
            [CypherMergeOnMatch]
            public double quantity { get; set; }
            [CypherMerge]
            [CypherMergeOnMatch]
            public UnitsOfMeasure unitOfMeasure { get; set; }
            [CypherMerge]
            [CypherMergeOnMatch]
            public int factor { get; set; }
            [CypherMerge]
            [CypherMergeOnMatch]
            public string instructionText { get; set; }
        }
    }
    [CypherLabel(Name = "MyName")]
    public class LabelledModel { }

    public class CypherModel
    {
        public CypherModel()
        {
            id = Guid.NewGuid();
        }

        [CypherMatch]
        [CypherMerge]
        public Guid id { get; set; }

        [CypherMergeOnCreate]
        public string firstName { get; set; }
        
        [CypherMergeOnCreate]
        public DateTimeOffset dateOfBirth { get; set; }
        
        [CypherMergeOnCreate]
        [CypherMergeOnMatch]
        public bool isLegend { get; set; }
        
        [CypherMergeOnCreate]
        [CypherMergeOnMatch]
        public int answerToTheMeaningOfLifeAndEverything { get; set; }
    }
}
