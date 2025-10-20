namespace Neo4jClient.Extension.Cypher
{
    public interface IOptionsBase
    {
        string PreCql { get; set; }
        string PostCql { get; set; }
    }
}
