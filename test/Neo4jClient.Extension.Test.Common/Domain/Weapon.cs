using UnitsNet;

namespace Neo4jClient.Extension.Test.TestData.Entities
{
    public class Weapon
    {
        public int Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Test unusal types
        /// </summary>
        public Area? BlastRadius { get; set; }
    }
}
