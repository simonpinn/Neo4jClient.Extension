namespace Neo4jClient.Extension.Cypher
{
    public class CreateDynamicOptions
    {
        public bool IgnoreNulls { get; set; }

        public override string ToString()
        {
            return string.Format("IgnoreNulls={0}", IgnoreNulls);
        }
    }
}