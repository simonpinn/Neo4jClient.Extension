namespace Neo4jClient.Extension.Test.Cypher
{
    public class Address
    {
        public string Street { get; set; }

        public string Suburb { get; set; }

        public override string ToString()
        {
            return string.Format("Street='{0}', Suburb='{1}'", Street, Suburb);
        }
    }
}