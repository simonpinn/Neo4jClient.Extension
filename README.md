# Neo4jClient.Extension #

Extending the awesome of [Neo4jClient](https://github.com/Readify/Neo4jClient)

Merge and match nodes or relationships using objects instead of typing pseudo Cypher.

Reduces mistakes and simplifies composition of queries. As well as some more advanced features, two key extension methods are provided:

* MergeEntity<T\>
* MergeRelationship<T\>

Any object can be provided to these two methods. 

##Fluent Config Setup##

To allow unobtrusive usage the extension library with domain model projects which don't want a reference to Neo4j, a fluent config interface has been included, that way any class can be used with a repository which uses Neo4j.

Given a domain model like below:

![Person, Address domain entities](https://raw.githubusercontent.com/simonpinn/Neo4jClient.Extension/master/docs/images/TestDataDiagram.png)

The address entity would be configured once per application lifetime scope like this: 

        fluentConfig = FluentConfig.Config()
            .With<Address>()
            .Match(a => a.Id)
            .Merge(a => a.Id)
            .MergeOnCreate(a => a.Street)
            .MergeOnCreate(a => a.Suburb)
            .MergeOnMatch(a => a.Street)
            .MergeOnMatch(a => a.Suburb)
            .Set();

The person entity undergoes a similar setup - see unit test examples.

##Fluent Config Usage##
Saving an address is as simple as:

    var address = SampleDataFactory.GetWellKnownAddress(7);
	GetFluentQuery().MergeEntity(address);

Saving a person, their address and setting the relationship between the two:

    var testPerson = SampleDataFactory.GetWellKnownPerson(7);

    var homeAddressRelationship = new HomeRelationship("person", "address");

    homeAddressRelationship.DateEffective = DateTime.Parse("2011-01-10T08:00:00+10:00");

    GetFluentQuery()
        .MergeEntity(testPerson)
        .MergeEntity(testPerson.HomeAddress)
        .MergeRelationship(homeAddressRelationship, homeAddressRelationship.UseProperties(r => r.DateEffective))
        .ExecuteWithoutResults();



## Attribute Config Example ##
The following attributes can be placed onto a domain model to control the generated query

* `CypherLabel` For use on a class, it controls the node Label, if unspecified then the class name is used
* `CypherMatch` Specifies that a property will be used in a MATCH statement
* `CypherMerge` Specifies that a property will be used in a MERGE statement
* `CypherMergeOnCreate` Specifies that a property will be used in the On Create SET portion of a MERGE statement
* `CypherMergeOnMatch` Specifies that a property will be used in the On Match SET portion of a MERGE statement

Hence, given a model such as:

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
