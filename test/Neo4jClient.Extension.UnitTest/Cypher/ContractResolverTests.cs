using FluentAssertions;
using Moq;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Cypher;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Neo4jClient.Extension.Test.Cypher
{
    /// <summary>
    /// Tests to ensure the library respects the GraphClient's configured ContractResolver
    /// </summary>
    public class ContractResolverTests
    {
        [Test]
        public void ApplyCasing_WithCamelCaseResolver_ReturnsCamelCase()
        {
            // Arrange
            var context = new CypherExtensionContext
            {
                JsonContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            // Act
            var result = "FirstName".ApplyCasing(context);

            // Assert
            result.Should().Be("firstName");
        }

        [Test]
        public void ApplyCasing_WithDefaultResolver_ReturnsPascalCase()
        {
            // Arrange
            var context = new CypherExtensionContext
            {
                JsonContractResolver = new DefaultContractResolver()
            };

            // Act
            var result = "FirstName".ApplyCasing(context);

            // Assert
            result.Should().Be("FirstName");
        }

        [Test]
        public void ApplyCasing_WithSnakeCaseNamingStrategy_ReturnsSnakeCase()
        {
            // Arrange
            var context = new CypherExtensionContext
            {
                JsonContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            // Act
            var result = "FirstName".ApplyCasing(context);

            // Assert
            result.Should().Be("first_name");
        }

        [Test]
        public void ApplyCasing_WithCamelCaseNamingStrategy_ReturnsCamelCase()
        {
            // Arrange
            var context = new CypherExtensionContext
            {
                JsonContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            // Act
            var result = "FirstName".ApplyCasing(context);

            // Assert
            result.Should().Be("firstName");
        }

        [Test]
        public void ApplyCasing_WithKebabCaseNamingStrategy_ReturnsKebabCase()
        {
            // Arrange
            var context = new CypherExtensionContext
            {
                JsonContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new KebabCaseNamingStrategy()
                }
            };

            // Act
            var result = "FirstName".ApplyCasing(context);

            // Assert
            result.Should().Be("first-name");
        }

        [Test]
        public void ApplyCasing_WithNullResolver_ReturnsPascalCase()
        {
            // Arrange
            var context = new CypherExtensionContext
            {
                JsonContractResolver = null
            };

            // Act
            var result = "FirstName".ApplyCasing(context);

            // Assert
            result.Should().Be("FirstName");
        }

        // Note: Full CreateEntity integration test with different resolvers is covered
        // in integration tests. The ApplyCasing tests above prove the core functionality.

        [Test]
        public void UseProperties_WithCamelCaseResolver_GeneratesCamelCaseProperties()
        {
            // Arrange
            var context = new CypherExtensionContext
            {
                JsonContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var person = new Person { Id = 1, Name = "Test" };

            // Act
            var properties = person.UseProperties(context, p => p.Id, p => p.Name);

            // Assert
            properties.Should().HaveCount(2);
            properties[0].TypeName.Should().Be("Id");
            properties[0].JsonName.Should().Be("id");
            properties[1].TypeName.Should().Be("Name");
            properties[1].JsonName.Should().Be("name");
        }

        [Test]
        public void UseProperties_WithSnakeCaseNamingStrategy_GeneratesSnakeCaseProperties()
        {
            // Arrange
            var context = new CypherExtensionContext
            {
                JsonContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            var person = new Person { Id = 1, Name = "Test" };

            // Act
            var properties = person.UseProperties(context, p => p.Id, p => p.Name);

            // Assert
            properties.Should().HaveCount(2);
            properties[0].TypeName.Should().Be("Id");
            properties[0].JsonName.Should().Be("id");
            properties[1].TypeName.Should().Be("Name");
            properties[1].JsonName.Should().Be("name");
        }
    }
}
