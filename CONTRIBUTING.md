# Contributing to AFIP .NET SDK

We welcome contributions to the AFIP .NET SDK! This document provides guidelines for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How to Contribute](#how-to-contribute)
- [Development Setup](#development-setup)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Documentation Guidelines](#documentation-guidelines)
- [Pull Request Process](#pull-request-process)
- [Release Process](#release-process)
- [Getting Help](#getting-help)

## Code of Conduct

By participating in this project, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md). Please read it before contributing.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally
3. **Create a branch** for your feature or bug fix
4. **Make your changes** following our coding standards
5. **Test your changes** thoroughly
6. **Submit a pull request**

## How to Contribute

### Reporting Bugs

Before creating bug reports, please check the existing issues to avoid duplicates. When creating a bug report, include:

- **Clear title and description**
- **Steps to reproduce** the issue
- **Expected vs actual behavior**
- **Environment details** (.NET version, OS, etc.)
- **Code samples** or test cases if applicable
- **Screenshots** if relevant

### Suggesting Enhancements

Enhancement suggestions are welcome! Please include:

- **Clear title and description**
- **Use case and rationale**
- **Proposed API changes** if applicable
- **Implementation suggestions** if you have them

### Contributing Code

We welcome:

- **Bug fixes**
- **Feature implementations**
- **Performance improvements**
- **Documentation improvements**
- **Test improvements**
- **Example applications**

## Development Setup

### Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- [Git](https://git-scm.com/)
- IDE: [Visual Studio](https://visualstudio.microsoft.com/), [Visual Studio Code](https://code.visualstudio.com/), or [JetBrains Rider](https://www.jetbrains.com/rider/)

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/afip-dotnet.git
   cd afip-dotnet
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

### Project Structure

```
afip-dotnet/
├── src/
│   ├── Afip.Dotnet.Abstractions/    # Interfaces and models
│   ├── Afip.Dotnet/                 # Core implementation
│   └── Afip.Dotnet.UnitTests/       # Unit tests
├── examples/
│   └── BasicUsage/                  # Usage examples
├── docs/                            # Documentation
├── .github/
│   └── workflows/                   # CI/CD workflows
├── GitVersion.yml                   # Version configuration
├── README.md
├── CONTRIBUTING.md
└── LICENSE
```

## Coding Standards

### General Guidelines

- **Follow C# conventions** as outlined in the [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- **Use meaningful names** for variables, methods, and classes
- **Write self-documenting code** with clear logic flow
- **Add XML documentation** for public APIs
- **Use async/await** for asynchronous operations
- **Handle exceptions** appropriately
- **Follow SOLID principles**

### Code Style

We use the default .NET code style with these additions:

```csharp
// Use explicit access modifiers
public class ExampleClass
{
    private readonly string _privateField;
    
    // Use XML documentation for public members
    /// <summary>
    /// Performs an example operation
    /// </summary>
    /// <param name="parameter">The input parameter</param>
    /// <returns>The operation result</returns>
    public async Task<string> ExampleMethodAsync(string parameter)
    {
        // Use meaningful variable names
        var processedResult = await ProcessAsync(parameter);
        return processedResult;
    }
}
```

### Naming Conventions

- **Classes**: PascalCase (`AfipClient`)
- **Methods**: PascalCase (`AuthorizeInvoiceAsync`)
- **Properties**: PascalCase (`InvoiceNumber`)
- **Fields**: camelCase with underscore prefix (`_logger`)
- **Parameters**: camelCase (`invoiceRequest`)
- **Constants**: PascalCase (`DefaultTimeout`)
- **Interfaces**: Start with 'I' (`IAfipClient`)

### File Organization

- **One class per file**
- **Match filename with class name**
- **Group related classes in appropriate namespaces**
- **Use folders to organize by feature/responsibility**

## Testing Guidelines

### Test Structure

- **Unit tests** for all public methods
- **Integration tests** for service interactions
- **Use meaningful test names** that describe the scenario
- **Follow AAA pattern**: Arrange, Act, Assert

### Test Naming Convention

```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var input = CreateTestInput();
    
    // Act
    var result = _service.MethodName(input);
    
    // Assert
    Assert.NotNull(result);
}
```

### Test Categories

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test component interactions
- **End-to-End Tests**: Test complete workflows

### Mocking Guidelines

- **Use Moq** for mocking dependencies
- **Mock external dependencies** (HTTP clients, databases)
- **Don't mock value objects** or simple data structures
- **Verify interactions** when behavior is important

### Code Coverage

- **Aim for 80%+ code coverage**
- **Focus on testing critical paths**
- **Don't write tests just for coverage**
- **Test edge cases and error conditions**

## Documentation Guidelines

### XML Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Authorizes an electronic invoice with AFIP
/// </summary>
/// <param name="request">The invoice authorization request</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>The authorization response with CAE details</returns>
/// <exception cref="AfipException">Thrown when authorization fails</exception>
public async Task<InvoiceResponse> AuthorizeInvoiceAsync(
    InvoiceRequest request, 
    CancellationToken cancellationToken = default)
```

### README Updates

- **Update README.md** for significant changes
- **Include code examples** for new features
- **Update installation instructions** if needed
- **Add breaking change notes** when applicable

### Code Comments

- **Explain why, not what**
- **Document complex algorithms**
- **Add TODO comments** for future improvements
- **Remove commented-out code**

## Pull Request Process

### Before Submitting

1. **Update your branch** with the latest changes from main
2. **Run all tests** and ensure they pass
3. **Run code analysis** and fix any issues
4. **Update documentation** as needed
5. **Add/update tests** for your changes

### Pull Request Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing performed

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review performed
- [ ] Comments added for complex code
- [ ] Documentation updated
- [ ] No new warnings introduced
```

### Review Process

1. **Automated checks** must pass
2. **Code review** by maintainers
3. **Testing** on target environments
4. **Approval** from project maintainers
5. **Merge** to main branch

### Commit Messages

Use conventional commit format:

```
type(scope): description

- feat: new feature
- fix: bug fix
- docs: documentation changes
- test: test changes
- refactor: code refactoring
- style: formatting changes
- chore: maintenance tasks
```

Examples:
```
feat(invoice): add support for foreign currency invoices
fix(auth): handle certificate expiration correctly
docs(readme): update installation instructions
test(client): add unit tests for configuration validation
```

## Release Process

### Versioning

We use [Semantic Versioning](https://semver.org/):

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Release Flow

1. **Develop branch**: Active development
2. **Main branch**: Stable releases
3. **Release tags**: Created for each version
4. **NuGet packages**: Auto-published on release

### Breaking Changes

- **Document** in CHANGELOG.md
- **Provide migration guide**
- **Update major version**
- **Announce** on relevant channels

## Getting Help

### Communication Channels

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: General questions and ideas
- **Pull Request Comments**: Code-specific discussions

### Development Questions

- **Check existing issues** and documentation first
- **Search closed issues** for similar problems
- **Provide context** when asking questions
- **Include code samples** when relevant

### Maintainer Response

- **Issues**: Response within 1-2 business days
- **Pull Requests**: Review within 3-5 business days
- **Security Issues**: Response within 24 hours

## Recognition

Contributors will be:

- **Listed** in the CONTRIBUTORS.md file
- **Mentioned** in release notes for significant contributions
- **Credited** in documentation where appropriate

## License

By contributing to this project, you agree that your contributions will be licensed under the same [MIT License](LICENSE) that covers the project.

Thank you for contributing to AFIP .NET SDK! 🚀