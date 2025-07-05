# AFIP Certificate Generator

A PowerShell Core script to generate AFIP-compatible certificates for web services integration.

## Features

- **Cross-platform**: Works on Windows, Linux, and macOS
- **Fully Self-Contained**: Automatically installs all required dependencies
- **AFIP Compliance**: Generates certificates that meet AFIP's requirements
- **Secure**: Uses strong encryption (2048-bit RSA, SHA256)
- **User-friendly**: Interactive prompts and clear feedback

## Auto-Installation Capabilities

The script automatically checks and installs all required dependencies:

### PowerShell Core
- **Windows**: winget, Chocolatey, or direct download from Microsoft
- **Linux**: apt-get, yum, dnf with Microsoft repository
- **macOS**: Homebrew or MacPorts

### OpenSSL
- **Windows**: winget, Chocolatey, Scoop, or direct download
- **Linux**: apt-get, yum, dnf, zypper, pacman, apk
- **macOS**: Homebrew or MacPorts

### Package Managers (Auto-Installed if Needed)
- **Windows**: Chocolatey, Scoop
- **macOS**: Homebrew

## Prerequisites

**None!** The script is completely self-contained and will install everything it needs.

However, for optimal experience:
- **Windows**: Run as Administrator for system-wide installations
- **Linux/macOS**: Use `sudo` for system-wide installations
- **Internet connection**: Required for downloading dependencies

## Usage

### Basic Usage

```powershell
# Generate certificate with default settings
.\generate-afip-certificate.ps1 -Cuit "20123456789"
```

### Advanced Usage

```powershell
# Generate certificate with custom settings
.\generate-afip-certificate.ps1 `
    -Cuit "20123456789" `
    -CompanyName "Mi Empresa S.A." `
    -Country "AR" `
    -State "Buenos Aires" `
    -City "Ciudad Autónoma de Buenos Aires" `
    -Email "admin@miempresa.com" `
    -OutputPath ".\certificates" `
    -CertificateName "afip-cert" `
    -Password "MySecurePassword123!" `
    -Force
```

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Cuit` | string | Yes | - | 11-digit CUIT number |
| `CompanyName` | string | No | "Test Company" | Company name |
| `Country` | string | No | "AR" | Country code |
| `State` | string | No | "Buenos Aires" | State/Province |
| `City` | string | No | "Ciudad Autónoma de Buenos Aires" | City |
| `Email` | string | No | "test@example.com" | Email address |
| `OutputPath` | string | No | ".\certificates" | Output directory |
| `CertificateName` | string | No | "afip-certificate" | Certificate file name |
| `Password` | string | No | Auto-generated | PKCS#12 password |
| `Force` | switch | No | false | Overwrite existing files |

## What the Script Does

1. **Checks PowerShell Core version** (requires 6.0+)
2. **Tests web connectivity** for downloads
3. **Checks for OpenSSL** and installs if missing
4. **Verifies directory access** for output
5. **Checks admin privileges** (informational)
6. **Validates CUIT format** (11 digits, valid prefixes)
7. **Generates certificate** with AFIP-compliant settings
8. **Creates PKCS#12 file** with password protection
9. **Verifies the certificate** for integrity
10. **Cleans up temporary files**

## Output

The script generates a PKCS#12 certificate file (`.p12`) that can be used with AFIP web services.

### Generated Files
- `{CertificateName}.p12` - PKCS#12 certificate file (main output)

### Certificate Specifications
- **Key size**: 2048 bits (RSA)
- **Signature algorithm**: SHA256
- **Validity**: 2 years
- **Key usage**: Digital signature, key encipherment, data encipherment
- **Extended key usage**: Server authentication, client authentication
- **Subject alternative names**: localhost, *.afip.gov.ar, *.ws.afip.gov.ar

## Integration with AFIP.Dotnet SDK

After generating the certificate, you can use it with the AFIP.Dotnet SDK:

```csharp
var config = new AfipConfiguration
{
    CertificatePath = "path/to/afip-certificate.p12",
    CertificatePassword = "your-password",
    Environment = AfipEnvironment.Testing, // or Production
    Cuit = "20123456789"
};

var client = new AfipClient(config);
```

## Troubleshooting

### Dependency Installation Issues

The script will automatically try multiple installation methods. If all fail:

1. **Check internet connectivity**
2. **Run as Administrator** (Windows) or with `sudo` (Linux/macOS)
3. **Check firewall/antivirus** settings
4. **Install dependencies manually** using the instructions below

### Manual Installation (if auto-installation fails)

#### PowerShell Core
- **Windows**: Download from https://github.com/PowerShell/PowerShell/releases
- **Linux**: Follow Microsoft's installation guide
- **macOS**: `brew install powershell`

#### OpenSSL
- **Windows**: `choco install openssl` or `scoop install openssl`
- **Linux**: `sudo apt-get install openssl` (Ubuntu/Debian)
- **macOS**: `brew install openssl`

### Permission Issues

- **Windows**: Run PowerShell as Administrator
- **Linux/macOS**: Use `sudo` for system-wide installation

### PATH Issues

If dependencies are installed but not found:

1. Restart your terminal/command prompt
2. Check if tools are in your PATH: `openssl version`
3. Add tools to your PATH manually if needed

### Certificate Validation

The script automatically verifies the generated certificate. If verification fails:

1. Check that the password is correct
2. Ensure the certificate file is not corrupted
3. Try regenerating the certificate

## Security Notes

- Keep the generated password secure
- Store the certificate file in a secure location
- Use different certificates for testing and production
- Regularly rotate certificates before expiration

## License

This script is part of the AFIP.Dotnet project and follows the same license terms. 