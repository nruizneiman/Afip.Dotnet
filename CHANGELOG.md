# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial SDK implementation
- WSAA authentication service with certificate-based authentication
- WSFEv1 electronic invoicing service
- Support for all major invoice types (A, B, C, FCE MiPyMEs)
- Foreign currency support (RG 5616/2024 - FEv4)
- Parameter tables service for dynamic AFIP lookups
- Comprehensive unit testing suite
- GitHub Actions CI/CD pipeline
- NuGet package configuration
- Comprehensive documentation and examples

### Changed
- N/A (initial release)

### Deprecated
- N/A (initial release)

### Removed
- N/A (initial release)

### Fixed
- N/A (initial release)

### Security
- Implemented secure certificate handling for X.509 PKCS#12 certificates
- Added input validation for all public APIs
- Secure HTTP communication with AFIP services

## [1.0.0] - TBD

### Added
- Complete AFIP .NET SDK implementation
- Support for WSAA (Web Service Authentication and Authorization)
- Support for WSFEv1 (Electronic Invoicing - Domestic Market)
- Electronic invoice authorization and querying
- Parameter tables integration
- Multi-environment support (Testing/Production)
- Async/await programming model throughout
- Microsoft.Extensions.Logging integration
- Comprehensive error handling with custom exceptions
- Strong typing for all AFIP models and responses
- Factory methods for easy client creation
- Automatic ticket caching and renewal
- Support for batch invoice processing
- VAT and tax details management
- Credit and debit note associations
- Foreign currency transaction support
- Regulatory compliance for multiple AFIP resolutions

### Technical Features
- .NET Standard 2.0 compatibility
- Thread-safe implementation
- Proper resource disposal patterns
- Cancellation token support
- HTTP connection pooling
- Comprehensive unit test coverage
- GitHub Actions CI/CD
- Automated NuGet publishing
- Code coverage reporting
- Security vulnerability scanning

### Supported Regulations
- RG 2485, RG 2904, RG 3067 (Base electronic invoicing)
- RG 3668/2014 (Restaurants, hotels, bars)
- RG 3749/2015 (VAT conditions)
- RG 4367/2018 (FCE MiPyMEs)
- RG 5616/2024 (Foreign currency - FEv4)

### Documentation
- Comprehensive README with examples
- Contributing guidelines
- Code of conduct
- API documentation
- Usage examples
- Migration guides
- FAQ section

---

## Version History Format

### [Version] - Date

#### Added
- New features

#### Changed
- Changes in existing functionality

#### Deprecated
- Soon-to-be removed features

#### Removed
- Now removed features

#### Fixed
- Bug fixes

#### Security
- Security improvements