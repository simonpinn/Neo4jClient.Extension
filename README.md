Neo4jClient.Extension
=====================

Extending the awesome of [Neo4jClient](https://github.com/Readify/Neo4jClient)

Merge and match nodes or relationships using objects instead of typing pseudo Cypher.

Reduces mistakes and simplifies composition of queries. As well as some more advanced features, two key extension methods are provided:

* MergeEntity
* MergeRelationship

Any object can be provided to these two methods, the properties which are utilised in both Entity and Relationship can be controlled in two ways, attributes and explicit property usage, the following attributes are provided:

* CypherLabel For use on a class, it controls the node Label, if unspecified then the class name is used
* CypherMatch Specifies that a property will be used in a MATCH statement
* CypherMerge Specifies that a property will be used in a MERGE statement
* CypherMergeOnCreate Specifies that a property will be used in the On Create SET portion of a MERGE statement
* CypherMergeOnMatch Specifies that a property will be used in the On Match SET portion of a MERGE statement

FluentConfig
=====================

To allow unobtrusive usage the extension library with domain model projects which don't want a reference to Neo4j, a fluent config interface has been included, that way any class can be used with a repository which uses Neo4j.

Given a class:

    public class SomeClass
    {
        public string SomeString { get; set; }
        public int Foo { get; set; }
        public bool Bar { get; set; }
    }
    
It can be configured like so, this would be done once per application lifetime scope. 

    FluentConfig.Config()
                .With<SomeClass>("myCypherLabel")
                .Match(x => x.Bar)
                .Merge(x => x.Bar)
                .Merge(x => x.Foo)
                .Merge(x => x.SomeString)
                .Set()

Example
=====================

Given a model such as:

    public class CypherModel
    {
        public CypherModel()
        {
            id = Guid.NewGuid();
        }

        [CypherMerge]
        public Guid id { get; set; }

        [CypherMergeOnCreate]
        [CypherMatch]
        public string firstName { get; set; }
        
        [CypherMergeOnCreate]
        public DateTimeOffset dateOfBirth { get; set; }
        
        [CypherMergeOnCreate]
        [CypherMergeOnMatch]
        public bool isLegend { get; set; }
        
        [CypherMergeOnCreate]
        public int answerToTheMeaningOfLifeAndEverything { get; set; }
    }

Instead of manually writing error prone Neo4jClient strings such as

    graphClient.Cypher
        .Match("(cyphermodel:CypherModel {firstName:{cyphermodel}.firstName,isLegend:{cyphermodel}.isLegend})")
        .WithParam("cyphermodel", cyphermodel)
    
We can just write

    graphClient.Cypher
        .MatchEntity(cyphermodel)
    
This becomes more useful as we compose more complicated structures that take advantage of Merge OnCreate and Merge OnMatch such as, the following Cypher:

    MERGE (cyphermodel:CypherModel {id:{cyphermodel}.id})
    ON MATCH
    SET cyphermodel.isLegend={cyphermodel}.isLegend,cyphermodel.answerToTheMeaningOfLifeAndEverything={cyphermodel}.answerToTheMeaningOfLifeAndEverything
    ON CREATE
    SET cyphermodel.firstName={cyphermodel}.firstName,cyphermodel.dateOfBirth={cyphermodel}.dateOfBirth,cyphermodel.isLegend={cyphermodel}.isLegend,cyphermodel.answerToTheMeaningOfLifeAndEverything={cyphermodel}.answerToTheMeaningOfLifeAndEverything

Is generated from:

    graphClient.Cypher
        .MergeEntity(cyphermodel)

A full list of examples can be found in the unit tests within the solution.
