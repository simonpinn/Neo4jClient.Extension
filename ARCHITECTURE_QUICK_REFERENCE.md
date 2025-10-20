# Neo4jClient.Extension - Architecture Quick Reference

## One-Minute Overview

Neo4jClient.Extension wraps Neo4jClient to build Cypher queries using strongly-typed C# objects. 

**Core Idea:** Instead of writing `MATCH (p:Person {id:$id})`, write:
```csharp
graphClient.Cypher.MatchEntity(person, "p")
```

The library handles label resolution, property extraction, and Cypher generation automatically.

---

## File Responsibility Map

| File | Lines | Responsibility |
|------|-------|-----------------|
| CypherExtension.Main.cs | 267 | Public API (CREATE/MERGE/MATCH entry points) + worker methods |
| CypherExtension.CqlBuilders.cs | 110 | Cypher string generation (node, relationship, set clauses) |
| FluentConfig.cs | 106 | Fluent configuration builder for metadata setup |
| CypherExtension.Entity.cs | 78 | Entity metadata extraction and caching |
| CypherTypeItemHelper.cs | 49 | Metadata cache management (concurrent dictionary) |
| CypherExtension.Dynamics.cs | 46 | Convert entities to parameter dictionaries |
| CypherExtension.Fluent.cs | 27 | Bridge between FluentConfig and CypherExtension cache |
| BaseRelationship.cs | 18 | Base class for typed relationships |
| CypherProperty.cs | 28 | Property metadata (TypeName, JsonName) |
| CypherTypeItem.cs | 30 | Cache key: (Type, AttributeType) tuple |

---

## Architectural Layers

```
┌─────────────────────────────────────┐
│   Public Extension Methods          │
│ (CreateEntity, MergeEntity, etc.)   │
├─────────────────────────────────────┤
│   Worker Methods / Options           │
│ (CommonCreate, MatchWorker, etc.)   │
├─────────────────────────────────────┤
│   Cypher String Builders            │
│ (GetRelationshipCql, GetSetCql)     │
├─────────────────────────────────────┤
│   Metadata Cache                    │
│ (CypherTypeItemHelper)              │
├─────────────────────────────────────┤
│   Configuration System              │
│ (FluentConfig, Attributes)          │
├─────────────────────────────────────┤
│   Neo4jClient Library               │
│ (ICypherFluentQuery, etc.)          │
└─────────────────────────────────────┘
```

---

## Configuration Two Ways

### Way 1: Fluent Configuration (Preferred)

```csharp
// Setup (once at application startup)
FluentConfig.Config()
    .With<Person>("SecretAgent")
    .Merge(x => x.Id)
    .MergeOnCreate(x => x.DateCreated)
    .MergeOnMatchOrCreate(x => x.Name)
    .Set();

// Usage (anywhere in app)
var query = graphClient.Cypher
    .MergeEntity(person);
```

**Benefit:** Domain models remain free of infrastructure concerns

### Way 2: Attribute Configuration (Alternative)

```csharp
public class Person
{
    [CypherMerge]
    public int Id { get; set; }
    
    [CypherMergeOnCreate]
    [CypherMergeOnMatch]
    public string Name { get; set; }
}
```

**Benefit:** Configuration co-located with entity definition

---

## Key Caching Strategy

**Why Cache?**
- Reflection is expensive
- Same configurations used repeatedly
- Want to discover metadata only once

**What's Cached?**

| Cache | Key | Value | Thread Safe |
|-------|-----|-------|-------------|
| Entity Labels | Type | String (label) | Lock protected |
| Property Mappings | (Type, AttributeType) | List<CypherProperty> | ConcurrentDictionary |

**Example:**
```
Person + CypherMergeAttribute → [Id, DateCreated, Name, ...]
Person + CypherMatchAttribute → [Id, ...]
```

---

## Metadata Flow

```
Domain Entity
    ↓
EntityLabel() extracts label from:
  1. CypherLabelAttribute (if present)
  2. Class name (default)
    ↓
CypherTypeItemHelper.PropertiesForPurpose<T, TAttr>()
    ↓
Lookup (Type, AttributeType) in ConcurrentDictionary
    ↓
If miss: Reflect on properties, find those decorated with TAttr
    ↓
Cache result
    ↓
Return List<CypherProperty>
```

---

## Parameter Generation

**Flow:**
1. Extract properties from entity via reflection
2. Convert to dictionary with JSON naming convention
3. Namespace parameters to avoid collisions

**Namespacing:**
```csharp
person.Id          → $personMatchKey.id      (base)
person.LastModified → $personLastModified    (individual match property)
person.DateCreated → $personOnCreate        (base create)
```

---

## Relationship Model

```csharp
public class HomeAddressRelationship : BaseRelationship
{
    public HomeAddressRelationship(string from, string to) 
        : base(from, to)
    {
        // FromKey = from    ("person")
        // ToKey = to        ("address")  
        // Key = from+to     ("personaddress")
    }
    
    public DateTimeOffset DateEffective { get; set; }
}

// Generates: (person)-[personaddress:HOME_ADDRESS]->(address)
```

---

## Common Extension Method Overloads

Most operations follow same pattern:

```csharp
// Simplest: just entity
CreateEntity<T>(query, entity)

// With identifier override
CreateEntity<T>(query, entity, identifier: "p")

// With all parameters
CreateEntity<T>(query, entity, identifier, onCreateOverride, preCql, postCql)

// With options object (most flexible)
CreateEntity<T>(query, entity, CreateOptions options)
```

---

## Options Classes (Advanced Control)

```csharp
// Match specific properties only
var matchOpts = new MatchOptions { MatchOverride = entity.UseProperties(x => x.Id) };
query.MatchEntity(entity, matchOpts)

// Create via relationship path
var mergeOpts = MergeOptions.ViaRelationship(relationship);
query.MergeEntity(address, mergeOpts)

// Custom pre/post Cypher
new CreateOptions 
{ 
    PreCql = "WITH [...] ",
    PostCql = " RETURN ..."
}
```

---

## Testing Patterns

### Unit Test (Mocked)
```csharp
public class MyTests : FluentConfigBaseTest
{
    [SetUp]
    public void Setup()
    {
        NeoConfig.ConfigureModel();  // Setup fluent config
    }
    
    [Test]
    public void MyTest()
    {
        var query = GetFluentQuery();  // Mock
        query.CreateEntity(entity);
        var cypher = query.GetFormattedDebugText();
        Assert.That(cypher, Does.Contain("CREATE"));
    }
}
```

### Integration Test (Real DB)
```csharp
public class MyIntegrationTests : IntegrationTest
{
    [Test]
    public async Task MyTest()
    {
        var result = await CypherQuery
            .CreateEntity(entity, "e")
            .ExecuteWithoutResultsAsync();
        // Verify in real Neo4j
    }
}
```

---

## Decision Tree: When to Use What

```
Want to create a node?
  └─ .CreateEntity<T>()

Want to find existing node?
  └─ .MatchEntity<T>()
       ├─ Optional match? 
       │  └─ .OptionalMatchEntity<T>()
       └─ Regular match?
          └─ .MatchEntity<T>()

Want to create or update node?
  └─ .MergeEntity<T>()

Want to setup entity once?
  └─ FluentConfig.Config().With<T>().Merge(...)...Set()

Need custom properties?
  └─ entity.UseProperties(x => x.Prop1, x => x.Prop2)

Working with relationships?
  └─ .CreateRelationship<T>()
  └─ .MergeRelationship<T>()
  └─ Inherit from BaseRelationship
```

---

## Performance Tips

1. **Configure Once:** FluentConfig.Config() at app startup, not per-request
2. **Cache Hits:** First use of entity type will reflect/cache, subsequent calls are fast
3. **Null Handling:** Null values skipped on CREATE (use IgnoreNulls option)
4. **Thread Safety:** Safe for concurrent use (locks/concurrent collections)

---

## Extension Points

### Custom JSON Converter
```csharp
public class CustomConverter : JsonConverter { }
graphClient.JsonConverters.Add(new CustomConverter());
```

### Custom Naming Convention
```csharp
var context = new CypherExtensionContext 
{ 
    JsonContractResolver = new PascalCaseResolver() 
};
```

### Property Overrides
```csharp
var props = entity.UseProperties(x => x.Id, x => x.Name);
query.MatchEntity(entity, propertyOverride: props)
```

### Pre/Post CQL Injection
```csharp
new CreateOptions { PreCql = "WITH [...] ", PostCql = " RETURN ..." }
```

---

## Useful Static Methods

```csharp
// Format Cypher for debugging
query.GetFormattedDebugText()

// Extract specific properties for override
entity.UseProperties(x => x.Prop1, x => x.Prop2)

// Add/override entity label
FluentConfig.Config().With<T>("CustomLabel")...Set()
```

---

## Code Organization

| Package | Purpose |
|---------|---------|
| Neo4jClient.Extension | Main library with extension methods |
| Neo4jClient.Extension.Attributes | Marker attributes (separate to keep clean) |
| Neo4jClient.Extension.UnitTest | Mocked tests with high speed |
| Neo4jClient.Extension.IntegrationTest | Real Neo4j database tests |
| Neo4jClient.Extension.Test.Common | Shared test infrastructure |

---

## Thread Safety Summary

- Entity label cache: Dictionary + Lock (safe)
- Property cache: ConcurrentDictionary (safe)
- FluentConfig: ConcurrentBag (safe)
- Static context: Read-only after init (safe)

**Verdict:** Safe for multi-threaded applications

---

## Next Steps to Understand Fully

1. Read `CLAUDE.md` for comprehensive architecture
2. Examine `CypherExtension.Main.cs` for public API
3. Look at `NeoConfig.cs` test to see fluent setup
4. Trace a single call through the layer stack
5. Review unit tests for usage patterns
