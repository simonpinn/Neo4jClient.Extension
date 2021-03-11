using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Test.Cypher;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Integration.Tests
{
    public class MergeTests : IntegrationTest
    {
        [Test]
        public void OneDeep()
        {
            // create
            new FluentConfigMergeTests(RealQueryFactory)
                .OneDeepAct()
                .ExecuteWithoutResultsAsync().Wait();

            // merge
            new FluentConfigMergeTests(RealQueryFactory)
                .OneDeepAct()
                .ExecuteWithoutResultsAsync().Wait();
        }

        [Test]
        public void TwoDeep()
        {
            // create
            new FluentConfigMergeTests(RealQueryFactory)
                .TwoDeepAct()
                .ExecuteWithoutResultsAsync().Wait();

            // merge
            new FluentConfigMergeTests(RealQueryFactory)
                .TwoDeepAct()
                .ExecuteWithoutResultsAsync().Wait();
        }

         [Test]
        public void OneDeepMergeByRelationship()
        {
            new FluentConfigMergeTests(RealQueryFactory)
                .OneDeepMergeByRelationshipAct()
                .ExecuteWithoutResultsAsync().Wait();
        }
        
    }
}
