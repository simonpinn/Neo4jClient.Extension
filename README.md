# Neo4jClient.Extension

[![NuGet Version](https://img.shields.io/nuget/v/Neo4jClient.Extension.svg)](https://www.nuget.org/packages/Neo4jClient.Extension/)
[![Build Status](https://img.shields.io/github/actions/workflow/status/simonpinn/Neo4jClient.Extension/ci.yml?branch=master)](https://github.com/simonpinn/Neo4jClient.Extension/actions)
[![License](https://img.shields.io/github/license/simonpinn/Neo4jClient.Extension.svg)](LICENSE)

A fluent API extension for [Neo4jClient](https://github.com/Readify/Neo4jClient) that simplifies building Cypher queries using strongly-typed C# objects.

## Features

- **Type-Safe Query Building** - Create, match, and merge nodes and relationships using objects instead of writing Cypher strings
- **Fluent Configuration** - Configure entity metadata without cluttering domain models with attributes
- **Relationship Modeling** - Strongly-typed relationships with properties
- **IntelliSense Support** - Full IDE autocomplete for improved productivity
- **Reduced Errors** - Compile-time checking prevents property name typos and refactoring issues

## Key Extension Methods

- `CreateEntity<T>` - Create nodes from objects
- `MergeEntity<T>` - Merge nodes with ON CREATE/ON MATCH support
- `MatchEntity<T>` - Match nodes by properties
- `CreateRelationship<T>` - Create typed relationships
- `MergeRelationship<T>` - Merge relationships
- `MatchRelationship<T>` - Match relationships

## Quick Start

### Installation

```bash
dotnet add package Neo4jClient.Extension
``` 

## Fluent Configuration

Configure entity metadata once at application startup without decorating your domain models: 

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

Note how we only set DateCreated when creating, not updating.

A relationship might be setup like this:

		FluentConfig.Config()
                .With<HomeAddressRelationship>()
                .MergeOnMatchOrCreate(hr => hr.DateEffective)
                .Set();

The address entity undergoes a similar setup - see the [unit tests](https://github.com/simonpinn/Neo4jClient.Extension/blob/master/test/Neo4jClient.Extension.Test.Common/Neo/NeoConfig.cs) for the complete setup.

##Fluent Config Usage##
Now that our model is configured, creating a weapon is as simple as:

		var weapon = SampleDataFactory.GetWellKnownWeapon(1);
    	var q = GetFluentQuery()
                .CreateEntity(weapon, "w");

Creating a person, their two addresses and setting the relationships between the three nodes:

		var agent = SampleDataFactory.GetWellKnownPerson(7);

        var q = GetFluentQuery()
                .CreateEntity(agent, "a")
                .CreateEntity(agent.HomeAddress, "ha")
                .CreateEntity(agent.WorkAddress, "wa")
                .CreateRelationship(new HomeAddressRelationship("a", "ha"))
                .CreateRelationship(new WorkAddressRelationship("a", "wa"));
        		.ExecuteWithoutResults();

Easy. Here is some merge syntax just to show off:

		var person = SampleDataFactory.GetWellKnownPerson(7);

        var homeAddressRelationship = new HomeAddressRelationship("person", "address");

        homeAddressRelationship.DateEffective = DateTime.Parse("2011-01-10T08:00:00+10:00");

        var q = GetFluentQuery()
            .MergeEntity(person)
            .MergeEntity(person.HomeAddress)
            .MergeRelationship(homeAddressRelationship);
			.ExecuteWithoutResults();

## Attribute Config ##
Before Fluent Config there was Attribute Config. If you insist on decorating your models with attributes, you may use the following attributes on a domain model to control the generated query

* `CypherLabel` Placed at class level, controls the node `label`, if unspecified then the class name is used
* `CypherMatch` Specifies that a property will be used in a `MATCH` statement
* `CypherMerge` Specifies that a property will be used in a `MERGE` statement
* `CypherMergeOnCreate` Specifies that a property will be used in the `ON CREATE SET` portion of a`MERGE` statement
* `CypherMergeOnMatch` Specifies that a property will be used in the `ON MATCH SET` portion of a `MERGE` statement

Below is an example model decorated with the above attributes

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

Yes, we think you should use the Fluent Config too.

A full list of examples can be found in the unit tests within the solution.


## Packaging ##
`build.ps1` is designed for [myget](http://www.myget.org/) compatibility. 

The script can be run locally via `powershell -f build.ps1`. By default, it expects an environment variable named `packageVersion`.

Some default parameters may be overridden, for example:
`powershell -f build.ps1 -configuration debug -sourceUrl https://github.com/your-username/Neo4jClient.Extension -packageVersion 5.0.0.1` 

Nuget packages are written to `./_output`