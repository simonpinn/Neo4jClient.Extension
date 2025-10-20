using System;
using System.Configuration;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Test.CustomConverters;
using Neo4jClient.Extension.Test.Data;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Integration
{

    public class IntegrationTest
    {
        protected static IGraphClient? GraphClient { get; private set; }

        protected ICypherFluentQuery CypherQuery { get { return GraphClient!.Cypher; } }

        [SetUp]
        public async Task Setup()
        {
            await CypherQuery.Match("(n)")
                .OptionalMatch("(n)-[r]-()")
                .Delete("n, r")
                .ExecuteWithoutResultsAsync();
        }

        protected Func<ICypherFluentQuery> RealQueryFactory
        {
            get { return () => CypherQuery; }
        }

        static IntegrationTest()
        {
            var connectionString = ConfigurationManager.AppSettings["Neo4jConnectionString"] ?? "bolt://localhost:7687";
            var username = ConfigurationManager.AppSettings["Neo4jUsername"] ?? "neo4j";
            var password = ConfigurationManager.AppSettings["Neo4jPassword"] ?? "testpassword";

            GraphClient = new BoltGraphClient(new Uri(connectionString), username, password);

            // Use CamelCasePropertyNamesContractResolver for consistent property naming
            GraphClient.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            GraphClient.JsonConverters.Add(new AreaJsonConverter());

            ((BoltGraphClient)GraphClient).ConnectAsync().Wait();

            NeoConfig.ConfigureModel();
        }
    }
}
