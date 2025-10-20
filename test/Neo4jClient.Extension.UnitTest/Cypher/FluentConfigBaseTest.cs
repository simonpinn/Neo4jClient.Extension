using System;
using System.Collections.Generic;
using Moq;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Test.CustomConverters;
using Neo4jClient.Extension.Test.Data;
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
            JsonConverters = new List<JsonConverter>();
            JsonConverters.Add(new AreaJsonConverter());

            NeoConfig.ConfigureModel();
        }

        protected IGraphClient GetMockCypherClient()
        {
            var moqGraphClient = new Mock<IGraphClient>();
            var mockRawClient = moqGraphClient.As<IRawGraphClient>();
            
            moqGraphClient.Setup(c => c.JsonConverters).Returns(JsonConverters);
            moqGraphClient.Setup(c => c.JsonContractResolver).Returns(new Newtonsoft.Json.Serialization.DefaultContractResolver());
            
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
