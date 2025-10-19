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

```csharp
FluentConfig.Config()
    .With<Person>("SecretAgent")
    .Match(x => x.Id)
    .Merge(x => x.Id)
    .MergeOnCreate(p => p.DateCreated)
    .MergeOnMatchOrCreate(p => p.Name)
    .MergeOnMatchOrCreate(p => p.Title)
    .Set();
```

Configure relationships with properties:

```csharp
FluentConfig.Config()
    .With<HomeAddressRelationship>()
    .MergeOnMatchOrCreate(hr => hr.DateEffective)
    .Set();
```

## Usage Examples

### Create a Node

```csharp
var person = new Person { Id = 1, Name = "John Doe" };
await client.Cypher
    .CreateEntity(person, "p")
    .ExecuteWithoutResultsAsync();
```

### Create Nodes with Relationships

```csharp
var person = new Person { Id = 1, Name = "John Doe" };
var address = new Address { Street = "123 Main St", City = "Austin" };

await client.Cypher
    .CreateEntity(person, "p")
    .CreateEntity(address, "a")
    .CreateRelationship(new HomeAddressRelationship("p", "a"))
    .ExecuteWithoutResultsAsync();
```

### Merge Nodes

```csharp
var person = new Person { Id = 1, Name = "John Doe", DateCreated = DateTime.UtcNow };

await client.Cypher
    .MergeEntity(person)  // Uses configured Merge properties
    .MergeEntity(person.HomeAddress)
    .MergeRelationship(new HomeAddressRelationship("person", "homeAddress"))
    .ExecuteWithoutResultsAsync();
```

## Alternative: Attribute Configuration

For those who prefer attributes, you can decorate your models directly:

```csharp
[CypherLabel(Name = "Person")]
public class Person
{
    [CypherMerge]
    public Guid Id { get; set; }

    [CypherMergeOnCreate]
    public string Name { get; set; }

    [CypherMergeOnMatchOrCreate]
    public bool IsActive { get; set; }
}
```

**Available Attributes:**
- `CypherLabel` - Custom node label (defaults to class name)
- `CypherMatch` - Used in MATCH clauses
- `CypherMerge` - Used in MERGE clauses
- `CypherMergeOnCreate` - Set only when creating (ON CREATE SET)
- `CypherMergeOnMatch` - Set only when matching (ON MATCH SET)

> **Note:** Fluent configuration is recommended to keep domain models infrastructure-free.

## Relationship Modeling

Define strongly-typed relationships by inheriting from `BaseRelationship`:

```csharp
[CypherLabel(Name = "HOME_ADDRESS")]
public class HomeAddressRelationship : BaseRelationship
{
    public HomeAddressRelationship(string fromKey, string toKey)
        : base(fromKey, toKey) { }

    public DateTime DateEffective { get; set; }
}
```

## Development

### Building

```bash
dotnet build Neo4jClient.Extension.sln
```

### Running Tests

**Unit Tests:**
```bash
dotnet test test/Neo4jClient.Extension.UnitTest/
```

**Integration Tests** (requires Neo4j):
```bash
# Automated setup (recommended)
./run-tests-with-neo4j.sh      # Linux/macOS
run-tests-with-neo4j.bat        # Windows

# Manual setup
docker compose up -d neo4j
dotnet test --filter Integration
docker compose down
```

### Packaging

```bash
powershell -f build.ps1 -packageVersion 1.0.0
```

Output: `./_output/` directory

## Documentation

- [CLAUDE.md](CLAUDE.md) - Comprehensive architecture documentation
- [DOCKER-TESTING.md](DOCKER-TESTING.md) - Docker setup for integration tests
- [Unit Tests](test/Neo4jClient.Extension.UnitTest/) - Usage examples

## Requirements

- .NET 9.0 or later
- Neo4jClient 4.0.0+
- Neo4j 5.x (for integration tests)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the terms specified in the [LICENSE](LICENSE) file.