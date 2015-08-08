using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Test.CustomConverters;
using Neo4jClient.Extension.Test.Data;
using Neo4jClient.Extension.Test.TestData.Entities;
using Neo4jClient.Extension.Test.TestEntities.Relationships;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Cypher
{
    [TestFixture]
    public abstract class FluentConfigBaseTest
    {
        protected List<JsonConverter> JsonConverters { get; private set; }

        private Func<ICypherFluentQuery> _seedQueryFactory;

        protected FluentConfigBaseTest()
        {
            UseMockQueryFactory();
        }

        protected void UseQueryFactory(Func<ICypherFluentQuery> queryFactory)
        {
            _seedQueryFactory = queryFactory;
        }

        [SetUp]
        public void TestSetup()
        {
            JsonConverters = GraphClient.DefaultJsonConverters.ToList();
            JsonConverters.Add(new AreaJsonConverter());

            NeoConfig.ConfigureModel();
        }

        protected IGraphClient GetMockCypherClient()
        {
            var moqGraphClient = new Mock<IGraphClient>();
            var mockRawClient = moqGraphClient.As<IRawGraphClient>();
            
            moqGraphClient.Setup(c => c.JsonConverters).Returns(JsonConverters);
            moqGraphClient.Setup(c => c.JsonContractResolver).Returns(GraphClient.DefaultJsonContractResolver);
            
            return mockRawClient.Object;
        }

        protected ICypherFluentQuery GetFluentQuery()
        {
            return _seedQueryFactory();
        }

        private void UseMockQueryFactory()
        {
            _seedQueryFactory = () =>
            {
                var cypherClient = GetMockCypherClient();
                return new CypherFluentQuery(cypherClient);
            };
        }

    }
}
