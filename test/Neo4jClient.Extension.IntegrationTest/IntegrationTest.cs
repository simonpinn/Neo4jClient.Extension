using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Test.CustomConverters;
using Neo4jClient.Extension.Test.Data;
using Neo4jClient.Transactions;
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
            GraphClient = new BoltGraphClient(new Uri(connectionString), "neo4j", "password");

            GraphClient.JsonConverters.Add(new AreaJsonConverter());

            ((BoltGraphClient)GraphClient).ConnectAsync().Wait();

            NeoConfig.ConfigureModel();
        }
    }
}
