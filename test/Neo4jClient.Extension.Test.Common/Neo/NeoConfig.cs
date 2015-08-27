using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Test.Cypher;
using Neo4jClient.Extension.Test.TestData.Entities;
using Neo4jClient.Extension.Test.TestEntities.Relationships;

namespace Neo4jClient.Extension.Test.Data
{
    public class NeoConfig
    {
        public static void ConfigureModel()
        {
            FluentConfig.Config()
               .With<Person>("SecretAgent")
               .Match(x => x.Id)
               .Merge(x => x.Id)
               .MergeOnCreate(p => p.Id)
               .MergeOnCreate(p => p.DateCreated)
               .MergeOnMatchOrCreate(p => p.Title)
               .MergeOnMatchOrCreate(p => p.Name)
               .MergeOnMatchOrCreate(p => p.IsOperative)
               .MergeOnMatchOrCreate(p => p.Sex)
               .MergeOnMatchOrCreate(p => p.SerialNumber)
               .MergeOnMatchOrCreate(p => p.SpendingAuthorisation)
               .Set();

            FluentConfig.Config()
                .With<Address>()
                //.Match(x => x.Street)
                //.Match(x => x.Suburb)
                //.Merge(x => x.Street)
                //.Merge(x => x.Suburb)
                .MergeOnMatchOrCreate(a => a.Street)
                .MergeOnMatchOrCreate(a => a.Suburb)
                .Set();

            FluentConfig.Config()
                .With<Weapon>()
                .Match(x => x.Id)
                .Merge(x => x.Id)
                .MergeOnMatchOrCreate(w => w.Name)
                .MergeOnMatchOrCreate(w => w.BlastRadius)
                .Set();

            FluentConfig.Config()
                .With<HomeAddressRelationship>()
                .Match(ha => ha.DateEffective)
                .MergeOnMatchOrCreate(hr => hr.DateEffective)
                .Set();
        }
    }
}
