using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Neo4jClient.Extension.Cypher;
using Neo4jClient.Extension.Test.Cypher;
using Neo4jClient.Extension.Test.TestData.Entities;
using Neo4jClient.Extension.Test.TestEntities.Relationships;
using NUnit.Framework;
using UnitsNet;
using UnitsNet.Units;

namespace Neo4jClient.Extension.Test.Integration.Tests
{
    public class MatchTests : IntegrationTest
    {
        [Test]
        public async Task MatchEntity_ReturnsCreatedPerson()
        {
            // Arrange: Create a person using CreateEntity
            var person = new Person
            {
                Id = 1,
                Name = "James Bond",
                Title = "Agent",
                Sex = Gender.Male,
                IsOperative = true,
                SerialNumber = 7,
                SpendingAuthorisation = 1000000,
                DateCreated = DateTimeOffset.UtcNow
            };

            await CypherQuery
                .CreateEntity(person, "p")
                .ExecuteWithoutResultsAsync();

            // Act: Match using MatchEntity
            var matchPerson = new Person { Id = 1 };
            var result = await CypherQuery
                .MatchEntity(matchPerson, "p")
                .Return(p => p.As<Person>())
                .ResultsAsync;

            // Assert: Verify all properties were saved and retrieved correctly
            var retrieved = result.Single();
            retrieved.Id.Should().Be(1);
            retrieved.Name.Should().Be("James Bond");
            retrieved.Title.Should().Be("Agent");
            retrieved.Sex.Should().Be(Gender.Male);
            retrieved.IsOperative.Should().BeTrue();
            retrieved.SerialNumber.Should().Be(7);
            retrieved.SpendingAuthorisation.Should().Be(1000000);
        }

        [Test]
        public async Task MatchEntity_WithRelationship_ReturnsPersonAndAddress()
        {
            // Arrange: Create person with home address
            var person = new Person
            {
                Id = 2,
                Name = "Q",
                Title = "Quartermaster",
                DateCreated = DateTimeOffset.UtcNow
            };

            var address = new Address
            {
                Street = "MI6 Headquarters",
                Suburb = "London"
            };

            var relationship = new HomeAddressRelationship("p", "a")
            {
                DateEffective = DateTimeOffset.UtcNow
            };

            await CypherQuery
                .CreateEntity(person, "p")
                .CreateEntity(address, "a")
                .CreateRelationship(relationship)
                .ExecuteWithoutResultsAsync();

            // Act: Match person and follow relationship to address using MatchRelationship
            var matchPerson = new Person { Id = 2 };
            var homeRelationship = new HomeAddressRelationship("p", "a");

            var result = await CypherQuery
                .MatchEntity(matchPerson, "p")
                .MatchRelationship(homeRelationship, MatchRelationshipOptions.Create().WithNoProperties())
                .Return((p, a) => new
                {
                    Person = p.As<Person>(),
                    Address = a.As<Address>()
                })
                .ResultsAsync;

            // Assert
            var retrieved = result.Single();
            retrieved.Person.Id.Should().Be(2);
            retrieved.Person.Name.Should().Be("Q");
            retrieved.Address.Street.Should().Be("MI6 Headquarters");
            retrieved.Address.Suburb.Should().Be("London");
        }

        [Test]
        public async Task MatchEntity_MultipleResults_ReturnsAll()
        {
            // Arrange: Create multiple people with same title
            var people = new[]
            {
                new Person { Id = 10, Name = "Agent 1", Title = "Field Agent", DateCreated = DateTimeOffset.UtcNow },
                new Person { Id = 11, Name = "Agent 2", Title = "Field Agent", DateCreated = DateTimeOffset.UtcNow },
                new Person { Id = 12, Name = "Agent 3", Title = "Field Agent", DateCreated = DateTimeOffset.UtcNow }
            };

            foreach (var p in people)
            {
                await CypherQuery
                    .CreateEntity(p, "p")
                    .ExecuteWithoutResultsAsync();
            }

            // Act: Match all people (using raw Match since we want all, not filtering by properties)
            var results = await CypherQuery
                .Match("(p:SecretAgent)")
                .Return(p => p.As<Person>())
                .ResultsAsync;

            // Assert
            results.Should().HaveCount(3);
            results.Select(r => r.Name).Should().BeEquivalentTo(new[] { "Agent 1", "Agent 2", "Agent 3" });
        }

        [Test]
        public async Task MatchEntity_NoResults_ReturnsEmpty()
        {
            // Act: Try to match a person that doesn't exist using MatchEntity
            var matchPerson = new Person { Id = 999 };
            var results = await CypherQuery
                .MatchEntity(matchPerson, "p")
                .Return(p => p.As<Person>())
                .ResultsAsync;

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public async Task OptionalMatchEntity_NoResults_ReturnsNull()
        {
            // Arrange: Create one person
            var person = new Person
            {
                Id = 20,
                Name = "Solo Agent",
                DateCreated = DateTimeOffset.UtcNow
            };

            await CypherQuery
                .CreateEntity(person, "p")
                .ExecuteWithoutResultsAsync();

            // Act: Match person and optionally match address (which doesn't exist) using OptionalMatchEntity
            var matchPerson = new Person { Id = 20 };
            var result = await CypherQuery
                .MatchEntity(matchPerson, "p")
                .OptionalMatch("(p)-[:HOME_ADDRESS]->(a:Address)")
                .Return((p, a) => new
                {
                    Person = p.As<Person>(),
                    Address = a.As<Address>()
                })
                .ResultsAsync;

            // Assert
            var retrieved = result.Single();
            retrieved.Person.Id.Should().Be(20);
            retrieved.Person.Name.Should().Be("Solo Agent");
            retrieved.Address.Should().BeNull();
        }

        [Test]
        public async Task MatchEntity_WithWeapon_ReturnsWeapon()
        {
            // Arrange: Create weapon
            var weapon = new Weapon
            {
                Id = 1,
                Name = "Walther PPK",
                BlastRadius = new Area(12.4, AreaUnit.SquareKilometer)
            };

            await CypherQuery
                .CreateEntity(weapon, "w")
                .ExecuteWithoutResultsAsync();

            // Act: Match the weapon by Id using MatchEntity
            var matchWeapon = new Weapon { Id = 1 };
            var result = await CypherQuery
                .MatchEntity(matchWeapon, "w")
                .Return(w => w.As<Weapon>())
                .ResultsAsync;

            // Assert
            var retrieved = result.Single();
            retrieved.Id.Should().Be(1);
            retrieved.Name.Should().Be("Walther PPK");
            retrieved.BlastRadius.Should().NotBeNull();
            retrieved.BlastRadius.Value.SquareKilometers.Should().BeApproximately(12.4, 0.01);
        }
        
        public void ArrangeTestData()
        {
            var archer = SampleDataFactory.GetWellKnownPerson(1);
            var isis = new Organisation {Name="ISIS"};
            var kgb = new Organisation { Name = "KGB" };
            
            var archerVariable = "a";
            var kgbVariable = "k";
            var isisVariable = "i";

            var agentRelationship = new WorksForRelationship("special agent", archerVariable, isisVariable);
            var doubleAgentRelationship = new WorksForRelationship("double agent", archerVariable, kgbVariable);

            var q = RealQueryFactory();

            q
                .CreateEntity(archer, archerVariable)
                .CreateEntity(isis, isisVariable)
                .CreateEntity(kgb, kgbVariable)
                .CreateRelationship(agentRelationship)
                .CreateRelationship(doubleAgentRelationship)
                .ExecuteWithoutResults();
        }
        
        [Test]
        public void Match()
        {
            ArrangeTestData();
            
            // Act
            var q = RealQueryFactory()
                .MatchRelationship(new WorksForRelationship("special agent", "p", "o"))
                .Return(o => o.As<Organisation>());

            Console.WriteLine(q.GetFormattedDebugText());
            var r = q.Results.ToList();

            Assert.AreEqual(1, r.Count);
            
            //Not working??
            Console.WriteLine($" Org={r[0].Name}");
        }
    }
}
