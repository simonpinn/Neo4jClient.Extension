using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Test.Cypher;
using Neo4jClient.Extension.Test.Data.Neo.Relationships;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Integration.Tests
{
    class MatchTests : IntegrationTest
    {
        public void ArrangeTestData()
        {
            var archer = SampleDataFactory.GetWellKnownPerson(1);
            var isis = new Organisation {Name="ISIS"};
            var kgb = new Organisation { Name = "KGB" };
            
            var archerVariable = "a";
            var kgbVariable = "k";
            var isisVariable = "i";

            var agentRelationship = new WorksForRelationship("special agent", archerVariable, isisVariable);
            var doubleAgentRelationship = new WorksForRelationship("double agent", archerVariable, kgbVariable);

            var q = RealQueryFactory();

            q
                .CreateEntity(archer, archerVariable)
                .CreateEntity(isis, isisVariable)
                .CreateEntity(kgb, kgbVariable)
                .CreateRelationship(agentRelationship)
                .CreateRelationship(doubleAgentRelationship)
                .ExecuteWithoutResults();
        }

        [Test]
        public void Match()
        {
            ArrangeTestData();
            
            // Act
            var q = RealQueryFactory()
                        .MatchRelationship(new WorksForRelationship("special agent", "p", "o"))
                        .Return(o => o.As<Organisation>());

             Console.WriteLine(q.GetFormattedDebugText());
             var r = q.Results.ToList();

            Assert.AreEqual(1, r.Count);
            
            //Not working??
            Console.WriteLine($" Org={r[0].Name}");
        }
    }
}
