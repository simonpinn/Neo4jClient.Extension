# Contributing to Neo4jClient.Extension

Thank you for considering contributing to Neo4jClient.Extension! This document provides guidelines and instructions for contributing.

## Code of Conduct

This project adheres to a code of conduct. By participating, you are expected to uphold this code. Please be respectful and constructive in all interactions.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues to avoid duplicates. When you create a bug report, include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples** (code snippets, test cases)
- **Describe the behavior you observed** and what you expected
- **Include version information** (.NET version, Neo4jClient.Extension version, Neo4j version)

### Suggesting Enhancements

Enhancement suggestions are welcome! Please provide:

- **A clear and descriptive title**
- **A detailed description of the proposed functionality**
- **Explain why this enhancement would be useful**
- **Provide examples** of how it would be used

### Pull Requests

1. **Fork the repository** and create your branch from `develop`
2. **Follow the branching strategy**:
   - `feature/your-feature-name` for new features
   - `hotfix/issue-description` for urgent fixes
   - `release/version-number` for release preparation

3. **Make your changes**:
   - Write clear, descriptive commit messages
   - Include semantic versioning hints in commits when appropriate
   - Follow the existing code style and conventions
   - Add or update tests as needed
   - Update documentation (README, CLAUDE.md, CHANGELOG.md)

4. **Test your changes**:
   ```bash
   # Run unit tests
   dotnet test test/Neo4jClient.Extension.UnitTest/

   # Run integration tests
   ./run-tests-with-neo4j.sh
   ```

5. **Update the CHANGELOG.md** under the `[Unreleased]` section

6. **Submit the pull request**:
   - Fill out the pull request template completely
   - Link any related issues
   - Indicate the semantic versioning impact

## Development Setup

### Prerequisites

- .NET 9.0 SDK or later
- Docker (for integration tests)
- Git
- Your favorite IDE (Visual Studio, Rider, VS Code)

### Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/simonpinn/Neo4jClient.Extension.git
   cd Neo4jClient.Extension
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run unit tests:
   ```bash
   dotnet test test/Neo4jClient.Extension.UnitTest/
   ```

5. Run integration tests (requires Docker):
   ```bash
   ./run-tests-with-neo4j.sh
   ```

## Coding Guidelines

### Style

- Follow standard C# conventions and best practices
- Use meaningful variable and method names
- Keep methods focused and concise
- Add XML documentation comments for public APIs
- Use nullable reference types where appropriate

### Architecture

- Maintain the existing architecture patterns (see CLAUDE.md)
- Static partial classes for extension methods
- Fluent configuration over attributes
- Options pattern for flexibility
- Keep domain models infrastructure-free

### Testing

- Write unit tests for all new functionality
- Add integration tests for complex scenarios
- Maintain or improve code coverage
- Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
- Follow the Arrange-Act-Assert pattern

### Documentation

- Update CLAUDE.md for architectural changes
- Update README.md for user-facing changes
- Add XML comments for public APIs
- Include code examples in documentation
- Update CHANGELOG.md

## Semantic Versioning

This project uses [GitVersion](https://gitversion.net/) for automatic versioning based on Git history.

### Commit Message Conventions

Use these prefixes to control version increments:

- `+semver: major` or `+semver: breaking` - Breaking changes (v1.0.0 → v2.0.0)
- `+semver: minor` or `+semver: feature` - New features (v1.0.0 → v1.1.0)
- `+semver: patch` or `+semver: fix` - Bug fixes (v1.0.0 → v1.0.1)
- `+semver: none` or `+semver: skip` - No version change

### Examples

```bash
git commit -m "Add support for nested relationships +semver: minor"
git commit -m "Fix null reference in CreateEntity +semver: patch"
git commit -m "Remove deprecated MatchEntity overload +semver: breaking"
git commit -m "Update documentation +semver: none"
```

## Branching Strategy

- **master** - Stable releases only
- **develop** - Main development branch
- **feature/*** - New features (branch from develop)
- **hotfix/*** - Urgent fixes (branch from master)
- **release/*** - Release preparation (branch from develop)

## Release Process

Releases are automated via GitHub Actions:

1. Merge changes to `master`
2. Create and push a version tag:
   ```bash
   git tag v1.2.3
   git push origin v1.2.3
   ```
3. GitHub Actions will:
   - Build and test the code
   - Create NuGet packages
   - Publish to NuGet.org
   - Create a GitHub release

## Questions?

Feel free to open an issue for questions or discussions about contributing.

## License

By contributing, you agree that your contributions will be licensed under the same license as the project (see LICENSE file).
