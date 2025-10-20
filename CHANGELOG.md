# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [4.0.0] - Upcoming

**BREAKING CHANGES:** This release aligns the major version with Neo4jClient 4.x to indicate compatibility.

### Added
- GitHub Actions CI/CD pipeline for automated builds and tests
- GitVersion for automatic semantic versioning
- Comprehensive CLAUDE.md architecture documentation
- Enhanced README with modern formatting and examples
- Docker-based integration testing with Neo4j 5.24
- CHANGELOG.md for tracking project changes
- CONTRIBUTING.md with development guidelines
- Pull request template

### Changed
- **BREAKING:** Updated to .NET 9.0 (from .NET Framework)
- **BREAKING:** Updated to Neo4jClient 4.0.0
- Updated unit tests to match Neo4jClient 4.0.0 query formatting changes
- Modernized README with better examples and structure
- Enhanced test scripts for Docker integration

### Fixed
- Fixed unit tests for Neo4jClient 4.0.0 parameter syntax (`$param` instead of `{param}`)
- Fixed unit tests for Neo4jClient 4.0.0 formatting (`ON MATCH\nSET` instead of `ON MATCH SET`)

## Versioning Strategy

Starting with v4.0.0, this library's major version will match the Neo4jClient major version it targets:
- Neo4jClient.Extension 4.x → Neo4jClient 4.x
- Neo4jClient.Extension 5.x → Neo4jClient 5.x (future)

This makes it clear which version of Neo4jClient is compatible.

## [1.0.2] - 2024-08-26

### Changed
- Made `UseProperties` method public instead of internal for better extensibility

## [1.0.1] - Prior Release

### Fixed
- Fixed `CreateRelationship` not honoring relationship identifier
- Fixed "An item with the same key has already been added" exception
- Fixed bad merge affecting `GetFormattedCypher`

### Changed
- Reference attributes by project instead of package

## [1.0.0] - Initial Release

### Added
- Fluent configuration API for entity metadata
- Extension methods for creating, matching, and merging entities
- Extension methods for relationship operations
- Attribute-based configuration support
- Strongly-typed relationship modeling
- Support for ON CREATE SET and ON MATCH SET in MERGE operations
- Automatic property name casing and JSON serialization
- Comprehensive unit and integration test suite

### Features
- `CreateEntity<T>` - Create nodes from objects
- `MergeEntity<T>` - Merge nodes with ON CREATE/ON MATCH
- `MatchEntity<T>` - Match nodes by properties
- `CreateRelationship<T>` - Create typed relationships
- `MergeRelationship<T>` - Merge relationships
- `MatchRelationship<T>` - Match relationships

---

## Version Guidelines

This project uses [Semantic Versioning](https://semver.org/):

- **MAJOR** version for incompatible API changes
- **MINOR** version for new functionality in a backwards compatible manner
- **PATCH** version for backwards compatible bug fixes

### Commit Message Conventions

To control version increments, use these commit message prefixes:

- `+semver: major` or `+semver: breaking` - Increment major version
- `+semver: minor` or `+semver: feature` - Increment minor version
- `+semver: patch` or `+semver: fix` - Increment patch version
- `+semver: none` or `+semver: skip` - No version increment

[Unreleased]: https://github.com/simonpinn/Neo4jClient.Extension/compare/v1.0.2...HEAD
[1.0.2]: https://github.com/simonpinn/Neo4jClient.Extension/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/simonpinn/Neo4jClient.Extension/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/simonpinn/Neo4jClient.Extension/releases/tag/v1.0.0
