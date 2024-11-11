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
        public async Task OneDeep()
        {
            // create
            await new FluentConfigMergeTests(RealQueryFactory)
                .OneDeepAct()
                .ExecuteWithoutResultsAsync();

            // merge
            await new FluentConfigMergeTests(RealQueryFactory)
                .OneDeepAct()
                .ExecuteWithoutResultsAsync();
        }

        [Test]
        public async Task TwoDeep()
        {
            // create
            await new FluentConfigMergeTests(RealQueryFactory)
                .TwoDeepAct()
                .ExecuteWithoutResultsAsync();

            // merge
            await new FluentConfigMergeTests(RealQueryFactory)
                .TwoDeepAct()
                .ExecuteWithoutResultsAsync();
        }

         [Test]
        public async Task OneDeepMergeByRelationship()
        {
            await new FluentConfigMergeTests(RealQueryFactory)
                .OneDeepMergeByRelationshipAct()
                .ExecuteWithoutResultsAsync();
        }
        
    }
}
