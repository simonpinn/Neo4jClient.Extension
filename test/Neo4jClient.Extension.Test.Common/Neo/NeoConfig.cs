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

            FluentConfig.Config()
               .With<WorkAddressRelationship>()
               .Set();
        }
    }
}
