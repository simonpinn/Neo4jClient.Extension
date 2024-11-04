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
        protected static ITransactionalGraphClient GraphClient { get; private set; }

        protected ICypherFluentQuery CypherQuery { get { return GraphClient.Cypher; } }

        [SetUp]
        public void Setup()
        {
            CypherQuery.Match("(n)")
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
            var connectionString = ConfigurationManager.AppSettings["Neo4jConnectionString"];
            
            //GraphClient  =new GraphClient(new Uri("http://localhost:7687"), "neo4j", "72%En4qX6lWK6u");

            GraphClient = new BoltGraphClient(new Uri("bolt://localhost:7687"), "neo4j", "72%En4qX6lWK6u");


            GraphClient.JsonConverters.Add(new AreaJsonConverter());

            GraphClient.ConnectAsync().GetAwaiter().GetResult();

            NeoConfig.ConfigureModel();
        }
    }
}
