#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Generates PKCS#12 certificates for AFIP web services according to AFIP requirements.

.DESCRIPTION
    This script generates a PKCS#12 certificate (.p12 file) that meets AFIP's requirements:
    - RSA key with 2048 bits
    - SHA256 signature algorithm
    - Valid for 2 years
    - Subject includes CUIT and other required fields
    - Includes private key and certificate chain

.PARAMETER OutputPath
    The path where the certificate files will be saved. Defaults to current directory.

.PARAMETER CertificateName
    The name for the certificate files (without extension). Defaults to "afip-certificate".

.PARAMETER Cuit
    The CUIT (Clave Unica de Identificacion Tributaria) number. Required.

.PARAMETER CompanyName
    The company name for the certificate. Defaults to "Test Company".

.PARAMETER Country
    The country code. Defaults to "AR" (Argentina).

.PARAMETER State
    The state/province. Defaults to "Buenos Aires".

.PARAMETER City
    The city. Defaults to "Buenos Aires".

.PARAMETER Email
    The email address. Defaults to "test@example.com".

.PARAMETER Password
    The password for the PKCS#12 file. If not provided, will generate a random one.

.PARAMETER Force
    Overwrite existing files without prompting.

.EXAMPLE
    .\generate-afip-certificate.ps1 -Cuit "20123456789" -CompanyName "My Company"

.EXAMPLE
    .\generate-afip-certificate.ps1 -Cuit "20123456789" -OutputPath "C:\Certificates" -Password "MyPassword123!"

.NOTES
    This script requires OpenSSL to be installed and available in the PATH.
    On Windows, you can install OpenSSL via Chocolatey: choco install openssl
    On Linux/macOS, you can install OpenSSL via package manager.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = ".",
    
    [Parameter(Mandatory = $false)]
    [string]$CertificateName = "afip-certificate",
    
    [Parameter(Mandatory = $true)]
    [string]$Cuit,
    
    [Parameter(Mandatory = $false)]
    [string]$CompanyName = "Test Company",
    
    [Parameter(Mandatory = $false)]
    [string]$Country = "AR",
    
    [Parameter(Mandatory = $false)]
    [string]$State = "Buenos Aires",
    
    [Parameter(Mandatory = $false)]
    [string]$City = "Buenos Aires",
    
    [Parameter(Mandatory = $false)]
    [string]$Email = "test@example.com",
    
    [Parameter(Mandatory = $false)]
    [string]$Password,
    
    [Parameter(Mandatory = $false)]
    [switch]$Force
)

# Function to check PowerShell Core version
function Test-PowerShellCore {
    $minVersion = [Version]"6.0.0"
    $currentVersion = $PSVersionTable.PSVersion
    
    if ($currentVersion -ge $minVersion) {
        Write-Host "OK PowerShell Core $currentVersion detected" -ForegroundColor Green
        return $true
    }
    else {
        Write-Host "WARNING: PowerShell Core $currentVersion detected, but $minVersion or higher is recommended" -ForegroundColor Yellow
        return $true  # Still allow execution
    }
}

# Function to check if running with admin privileges
function Test-AdminPrivileges {
    if ($IsWindows) {
        $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
        $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
        return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    }
    else {
        return $false  # On Unix-like systems, we'll use sudo
    }
}

# Function to check if OpenSSL is available
function Test-OpenSSL {
    try {
        $null = & openssl version 2>$null
        return $true
    }
    catch {
        return $false
    }
}

# Function to check if curl/Invoke-WebRequest is available
function Test-WebRequest {
    try {
        $null = Invoke-WebRequest -Uri "https://httpbin.org/get" -UseBasicParsing -TimeoutSec 5 2>$null
        return $true
    }
    catch {
        return $false
    }
}

# Function to check if required directories are writable
function Test-DirectoryAccess {
    param([string]$Path)
    
    try {
        $testFile = Join-Path $Path "test-write-access.tmp"
        "test" | Out-File -FilePath $testFile -Encoding UTF8 -ErrorAction Stop
        Remove-Item $testFile -Force -ErrorAction SilentlyContinue
        return $true
    }
    catch {
        return $false
    }
}

# Function to install PowerShell Core on Windows
function Install-PowerShellCoreWindows {
    Write-Host "Installing PowerShell Core..." -ForegroundColor Yellow
    
    # Try winget first
    try {
        Write-Host "Trying winget..." -ForegroundColor Cyan
        & winget install Microsoft.PowerShell --accept-source-agreements --accept-package-agreements 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "OK PowerShell Core installed via winget" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "winget not available, trying Chocolatey..." -ForegroundColor Cyan
    }
    
    # Try Chocolatey
    try {
        if (Get-Command choco -ErrorAction SilentlyContinue) {
            Write-Host "Installing PowerShell Core via Chocolatey..." -ForegroundColor Cyan
            & choco install powershell-core -y 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "OK PowerShell Core installed via Chocolatey" -ForegroundColor Green
                return $true
            }
        }
    }
    catch {
        Write-Host "Chocolatey not available, trying direct download..." -ForegroundColor Cyan
    }
    
    # Try direct download
    try {
        Write-Host "Downloading PowerShell Core from Microsoft..." -ForegroundColor Cyan
        $tempDir = Join-Path $env:TEMP "pwsh-install"
        New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
        
        # Get latest PowerShell Core version
        $releasesUrl = "https://api.github.com/repos/PowerShell/PowerShell/releases/latest"
        $release = Invoke-RestMethod -Uri $releasesUrl -UseBasicParsing
        
        # Find Windows x64 MSI
        $asset = $release.assets | Where-Object { $_.name -like "*win-x64.msi" } | Select-Object -First 1
        $installerPath = Join-Path $tempDir $asset.name
        
        Write-Host "Downloading PowerShell Core installer..." -ForegroundColor Cyan
        Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $installerPath -UseBasicParsing
        
        Write-Host "Installing PowerShell Core..." -ForegroundColor Cyan
        Start-Process -FilePath "msiexec.exe" -ArgumentList "/i `"$installerPath`" /quiet /norestart" -Wait
        
        # Clean up
        Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        
        Write-Host "OK PowerShell Core installed" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "Failed to install PowerShell Core automatically" -ForegroundColor Red
    }
    
    return $false
}

# Function to install PowerShell Core on Linux/macOS
function Install-PowerShellCoreUnix {
    Write-Host "Installing PowerShell Core..." -ForegroundColor Yellow
    
    if ($IsLinux) {
        # Try different package managers
        $packageManagers = @(
            @{ Name = "apt-get"; Command = "wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && sudo dpkg -i packages-microsoft-prod.deb && sudo apt-get update && sudo apt-get install -y powershell" },
            @{ Name = "yum"; Command = "sudo yum install -y https://packages.microsoft.com/rhel/7/prod/powershell-lts-7.3.9-1.rhel.7.x86_64.rpm" },
            @{ Name = "dnf"; Command = "sudo dnf install -y https://packages.microsoft.com/rhel/8/prod/powershell-lts-7.3.9-1.rhel.8.x86_64.rpm" }
        )
        
        foreach ($pm in $packageManagers) {
            try {
                Write-Host "Trying $($pm.Name)..." -ForegroundColor Cyan
                Invoke-Expression $pm.Command 2>$null
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "OK PowerShell Core installed via $($pm.Name)" -ForegroundColor Green
                    return $true
                }
            }
            catch {
                continue
            }
        }
    }
    elseif ($IsMacOS) {
        try {
            Write-Host "Installing PowerShell Core via Homebrew..." -ForegroundColor Cyan
            & brew install powershell 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "OK PowerShell Core installed via Homebrew" -ForegroundColor Green
                return $true
            }
        }
        catch {
            Write-Host "Homebrew not available" -ForegroundColor Yellow
        }
    }
    
    return $false
}

# Function to install Chocolatey on Windows
function Install-Chocolatey {
    Write-Host "Installing Chocolatey package manager..." -ForegroundColor Yellow
    
    try {
        Set-ExecutionPolicy Bypass -Scope Process -Force
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
        iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
        
        if (Get-Command choco -ErrorAction SilentlyContinue) {
            Write-Host "OK Chocolatey installed successfully" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "Failed to install Chocolatey: $_" -ForegroundColor Red
    }
    
    return $false
}

# Function to install Scoop on Windows
function Install-Scoop {
    Write-Host "Installing Scoop package manager..." -ForegroundColor Yellow
    
    try {
        Set-ExecutionPolicy RemoteSigned -Scope CurrentUser -Force
        Invoke-RestMethod -Uri https://get.scoop.sh | Invoke-Expression
        
        if (Get-Command scoop -ErrorAction SilentlyContinue) {
            Write-Host "OK Scoop installed successfully" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "Failed to install Scoop: $_" -ForegroundColor Red
    }
    
    return $false
}

# Function to install Homebrew on macOS
function Install-Homebrew {
    Write-Host "Installing Homebrew package manager..." -ForegroundColor Yellow
    
    try {
        /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
        
        if (Get-Command brew -ErrorAction SilentlyContinue) {
            Write-Host "OK Homebrew installed successfully" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "Failed to install Homebrew: $_" -ForegroundColor Red
    }
    
    return $false
}

# Function to install OpenSSL on Windows
function Install-OpenSSLWindows {
    Write-Host "Installing OpenSSL..." -ForegroundColor Yellow
    
    # Try winget first (Windows 10/11)
    try {
        Write-Host "Trying winget..." -ForegroundColor Cyan
        & winget install OpenSSL --accept-source-agreements --accept-package-agreements 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "OK OpenSSL installed via winget" -ForegroundColor Green
            # Refresh PATH for winget installations
            $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH","User")
            return $true
        }
    }
    catch {
        Write-Host "winget not available, trying Chocolatey..." -ForegroundColor Cyan
    }
    
    # Try Chocolatey
    try {
        if (Get-Command choco -ErrorAction SilentlyContinue) {
            Write-Host "Installing OpenSSL via Chocolatey..." -ForegroundColor Cyan
            & choco install openssl -y 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "OK OpenSSL installed via Chocolatey" -ForegroundColor Green
                # Refresh PATH for Chocolatey installations
                $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH","User")
                return $true
            }
        } else {
            Write-Host "Chocolatey not available, attempting to install it..." -ForegroundColor Cyan
            if (Install-Chocolatey) {
                Write-Host "Retrying OpenSSL installation via Chocolatey..." -ForegroundColor Cyan
                & choco install openssl -y 2>$null
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "OK OpenSSL installed via Chocolatey" -ForegroundColor Green
                    # Refresh PATH for Chocolatey installations
                    $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH","User")
                    return $true
                }
            }
        }
    }
    catch {
        Write-Host "Chocolatey installation failed, trying Scoop..." -ForegroundColor Cyan
    }
    
    # Try Scoop
    try {
        if (Get-Command scoop -ErrorAction SilentlyContinue) {
            Write-Host "Installing OpenSSL via Scoop..." -ForegroundColor Cyan
            & scoop install openssl 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "OK OpenSSL installed via Scoop" -ForegroundColor Green
                # Refresh PATH for Scoop installations
                $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH","User")
                return $true
            }
        } else {
            Write-Host "Scoop not available, attempting to install it..." -ForegroundColor Cyan
            if (Install-Scoop) {
                Write-Host "Retrying OpenSSL installation via Scoop..." -ForegroundColor Cyan
                & scoop install openssl 2>$null
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "OK OpenSSL installed via Scoop" -ForegroundColor Green
                    # Refresh PATH for Scoop installations
                    $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH","User")
                    return $true
                }
            }
        }
    }
    catch {
        Write-Host "Scoop installation failed, trying direct download..." -ForegroundColor Cyan
    }
    
    # Try downloading from official source
    try {
        Write-Host "Downloading OpenSSL from official source..." -ForegroundColor Cyan
        $tempDir = Join-Path $env:TEMP "openssl-install"
        New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
        
        # Download OpenSSL for Windows
        $opensslUrl = "https://slproweb.com/download/Win64OpenSSL-3_1_4.exe"
        $installerPath = Join-Path $tempDir "Win64OpenSSL.exe"
        
        Write-Host "Downloading OpenSSL installer..." -ForegroundColor Cyan
        Invoke-WebRequest -Uri $opensslUrl -OutFile $installerPath -UseBasicParsing
        
        Write-Host "Installing OpenSSL..." -ForegroundColor Cyan
        Start-Process -FilePath $installerPath -ArgumentList "/S" -Wait
        
        # Add to PATH
        $opensslPath = "C:\Program Files\OpenSSL-Win64\bin"
        if (Test-Path $opensslPath) {
            $currentPath = [Environment]::GetEnvironmentVariable("PATH", "Machine")
            if ($currentPath -notlike "*$opensslPath*") {
                [Environment]::SetEnvironmentVariable("PATH", "$currentPath;$opensslPath", "Machine")
                $env:PATH = "$env:PATH;$opensslPath"
            }
            Write-Host "OK OpenSSL installed and added to PATH" -ForegroundColor Green
            return $true
        }
        
        # Clean up
        Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    }
    catch {
        Write-Host "Failed to install OpenSSL automatically" -ForegroundColor Red
    }
    
    return $false
}

# Function to install OpenSSL on Linux/macOS
function Install-OpenSSLLinux {
    Write-Host "Installing OpenSSL..." -ForegroundColor Yellow
    
    # Detect OS and install accordingly
    if ($IsLinux) {
        # Try different package managers
        $packageManagers = @(
            @{ Name = "apt-get"; Command = "sudo apt-get update && sudo apt-get install -y openssl" },
            @{ Name = "yum"; Command = "sudo yum install -y openssl" },
            @{ Name = "dnf"; Command = "sudo dnf install -y openssl" },
            @{ Name = "zypper"; Command = "sudo zypper install -y openssl" },
            @{ Name = "pacman"; Command = "sudo pacman -S openssl --noconfirm" },
            @{ Name = "apk"; Command = "sudo apk add openssl" }
        )
        
        foreach ($pm in $packageManagers) {
            try {
                Write-Host "Trying $($pm.Name)..." -ForegroundColor Cyan
                Invoke-Expression $pm.Command 2>$null
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "OK OpenSSL installed via $($pm.Name)" -ForegroundColor Green
                    # Refresh PATH for Linux installations
                    $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ":" + [System.Environment]::GetEnvironmentVariable("PATH","User")
                    return $true
                }
            }
            catch {
                continue
            }
        }
    }
    elseif ($IsMacOS) {
        # Try Homebrew first
        try {
            Write-Host "Installing OpenSSL via Homebrew..." -ForegroundColor Cyan
            & brew install openssl 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "OK OpenSSL installed via Homebrew" -ForegroundColor Green
                # Refresh PATH for Homebrew installations
                $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ":" + [System.Environment]::GetEnvironmentVariable("PATH","User")
                return $true
            }
        }
        catch {
            Write-Host "Homebrew not available, attempting to install it..." -ForegroundColor Cyan
            if (Install-Homebrew) {
                Write-Host "Retrying OpenSSL installation via Homebrew..." -ForegroundColor Cyan
                & brew install openssl 2>$null
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "OK OpenSSL installed via Homebrew" -ForegroundColor Green
                    # Refresh PATH for Homebrew installations
                    $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ":" + [System.Environment]::GetEnvironmentVariable("PATH","User")
                    return $true
                }
            } else {
                Write-Host "Homebrew installation failed, trying MacPorts..." -ForegroundColor Cyan
            }
        }
        
        # Try MacPorts
        try {
            if (Get-Command port -ErrorAction SilentlyContinue) {
                Write-Host "Installing OpenSSL via MacPorts..." -ForegroundColor Cyan
                & sudo port install openssl3 2>$null
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "OK OpenSSL installed via MacPorts" -ForegroundColor Green
                    # Refresh PATH for MacPorts installations
                    $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ":" + [System.Environment]::GetEnvironmentVariable("PATH","User")
                    return $true
                }
            }
        }
        catch {
            Write-Host "MacPorts not available" -ForegroundColor Yellow
        }
    }
    
    return $false
}

# Function to install OpenSSL
function Install-OpenSSL {
    Write-Host "OpenSSL is not installed. Attempting to install automatically..." -ForegroundColor Yellow
    Write-Host ""
    
    # Check if we have admin privileges for system-wide installation
    if (-not (Test-AdminPrivileges)) {
        Write-Host "Note: Running without admin privileges. Some installation methods may not work." -ForegroundColor Yellow
        Write-Host "If installation fails, please run as administrator or install OpenSSL manually." -ForegroundColor Yellow
        Write-Host ""
    }
    
    if ($IsWindows) {
        return Install-OpenSSLWindows
    }
    else {
        return Install-OpenSSLLinux
    }
}

# Function to validate CUIT format
function Test-CuitFormat {
    param([string]$Cuit)
    
    # Remove any non-digit characters
    $cleanCuit = $Cuit -replace '[^0-9]', ''
    
    # Check if it's 11 digits
    if ($cleanCuit.Length -ne 11) {
        return $false
    }
    
    # Check if it starts with 20, 23, 24, 27, 30, 33, 34 (valid CUIT prefixes)
    $prefix = $cleanCuit.Substring(0, 2)
    $validPrefixes = @("20", "23", "24", "27", "30", "33", "34")
    
    if ($prefix -notin $validPrefixes) {
        return $false
    }
    
    return $true
}

# Function to generate random password if not provided
function New-RandomPassword {
    $chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*"
    $password = ""
    for ($i = 0; $i -lt 16; $i++) {
        $password += $chars[(Get-Random -Maximum $chars.Length)]
    }
    return $password
}

# Function to create OpenSSL configuration file
function New-OpenSSLConfig {
    param(
        [string]$ConfigPath,
        [string]$Cuit,
        [string]$CompanyName,
        [string]$Country,
        [string]$State,
        [string]$City,
        [string]$Email
    )
    
    $configContent = @"
[req]
default_bits = 2048
default_keyfile = private.key
distinguished_name = req_distinguished_name
req_extensions = v3_req
prompt = no
default_md = sha256

[req_distinguished_name]
C = $Country
ST = $State
L = $City
O = $CompanyName
OU = AFIP Web Services
CN = $Cuit
emailAddress = $Email

[v3_req]
basicConstraints = CA:FALSE
keyUsage = digitalSignature, keyEncipherment, dataEncipherment
extendedKeyUsage = serverAuth, clientAuth
subjectAltName = @alt_names

[alt_names]
DNS.1 = localhost
DNS.2 = *.afip.gov.ar
DNS.3 = *.ws.afip.gov.ar
"@
    
    $configContent | Out-File -FilePath $ConfigPath -Encoding UTF8 -NoNewline
}

# Main script execution
Write-Host "AFIP Certificate Generator" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green
Write-Host ""

# Check and install all required dependencies
Write-Host "Checking dependencies..." -ForegroundColor Cyan
Write-Host ""

# Check if PowerShell Core is available and meets minimum version
if (-not (Test-PowerShellCore)) {
    Write-Host "PowerShell Core is not installed or is too old. Attempting to install automatically..." -ForegroundColor Yellow
    Write-Host ""
    
    if ($IsWindows) {
        if (-not (Install-PowerShellCoreWindows)) {
            Write-Error "Failed to install PowerShell Core automatically. Please install it manually and ensure it's in your PATH."
            Write-Host ""
            Write-Host "Installation instructions:" -ForegroundColor Yellow
            Write-Host "  Windows: winget install Microsoft.PowerShell" -ForegroundColor Cyan
            Write-Host "  Windows: choco install powershell-core" -ForegroundColor Cyan
            Write-Host "  Windows: direct download from https://github.com/PowerShell/PowerShell/releases" -ForegroundColor Cyan
            exit 1
        }
    }
    else {
        if (-not (Install-PowerShellCoreUnix)) {
            Write-Error "Failed to install PowerShell Core automatically. Please install it manually and ensure it's in your PATH."
            Write-Host ""
            Write-Host "Installation instructions:" -ForegroundColor Yellow
            Write-Host "  Ubuntu/Debian: sudo apt-get install powershell" -ForegroundColor Cyan
            Write-Host "  CentOS/RHEL: sudo yum install powershell" -ForegroundColor Cyan
            Write-Host "  macOS: brew install powershell" -ForegroundColor Cyan
            exit 1
        }
    }
}

# Check if web connectivity is available (needed for downloads)
if (-not (Test-WebRequest)) {
    Write-Warning "Web connectivity test failed. Some features may not work properly."
    Write-Host "  This may affect automatic dependency installation." -ForegroundColor Yellow
    Write-Host ""
}

# Check if OpenSSL is available
if (-not (Test-OpenSSL)) {
    Write-Host "OpenSSL not found. Attempting to install automatically..." -ForegroundColor Yellow
    Write-Host ""
    
    if (-not (Install-OpenSSL)) {
        Write-Error "Failed to install OpenSSL automatically. Please install it manually and ensure it's in your PATH."
        Write-Host ""
        Write-Host "Installation instructions:" -ForegroundColor Yellow
        Write-Host "  Windows: choco install openssl" -ForegroundColor Cyan
        Write-Host "  Ubuntu/Debian: sudo apt-get install openssl" -ForegroundColor Cyan
        Write-Host "  CentOS/RHEL: sudo yum install openssl" -ForegroundColor Cyan
        Write-Host "  macOS: brew install openssl" -ForegroundColor Cyan
        exit 1
    }
    
    # Refresh PATH and test again
    Write-Host "Refreshing PATH..." -ForegroundColor Cyan
    $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH","User")
    
    # Wait a moment for installation to complete
    Start-Sleep -Seconds 2
    
    if (-not (Test-OpenSSL)) {
        Write-Error "OpenSSL installation completed but still not available in PATH."
        Write-Host "Please restart your terminal and try again, or add OpenSSL to your PATH manually." -ForegroundColor Yellow
        exit 1
    }
}

Write-Host "OK OpenSSL found: $(openssl version)" -ForegroundColor Green

# Check if output directory is accessible
if (-not (Test-DirectoryAccess -Path $OutputPath)) {
    Write-Error "Cannot write to output directory: $OutputPath"
    Write-Host "Please ensure you have write permissions to this directory." -ForegroundColor Yellow
    exit 1
}

Write-Host "OK Output directory is writable: $OutputPath" -ForegroundColor Green

# Check admin privileges (informational)
if (-not (Test-AdminPrivileges)) {
    Write-Host "INFO: Running without admin privileges. Some installation methods may not work." -ForegroundColor Yellow
    Write-Host "  If you encounter issues, try running as administrator." -ForegroundColor Yellow
    Write-Host ""
} else {
    Write-Host "OK Running with admin privileges" -ForegroundColor Green
}

Write-Host "All dependencies checked successfully!" -ForegroundColor Green
Write-Host ""

# Validate CUIT
if (-not (Test-CuitFormat -Cuit $Cuit)) {
    Write-Error "Invalid CUIT format. CUIT must be 11 digits and start with a valid prefix (20, 23, 24, 27, 30, 33, 34)."
    exit 1
}

Write-Host "OK CUIT format validated: $Cuit" -ForegroundColor Green

# Set file paths
$configFile = Join-Path $OutputPath "$CertificateName.conf"
$privateKeyFile = Join-Path $OutputPath "$CertificateName.key"
$certificateFile = Join-Path $OutputPath "$CertificateName.crt"
$p12File = Join-Path $OutputPath "$CertificateName.p12"

# Check if files already exist
$existingFiles = @($configFile, $privateKeyFile, $certificateFile, $p12File) | Where-Object { Test-Path $_ }
if ($existingFiles -and -not $Force) {
    Write-Warning "The following files already exist:"
    $existingFiles | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
    $response = Read-Host "Do you want to overwrite them? (y/N)"
    if ($response -ne "y" -and $response -ne "Y") {
        Write-Host "Operation cancelled." -ForegroundColor Yellow
        exit 0
    }
}

# Generate password if not provided
if (-not $Password) {
    $Password = New-RandomPassword
    Write-Host "OK Generated random password: $Password" -ForegroundColor Green
    Write-Host "  Please save this password securely!" -ForegroundColor Yellow
} else {
    Write-Host "OK Using provided password" -ForegroundColor Green
}

Write-Host ""

# Create OpenSSL configuration
Write-Host "Creating OpenSSL configuration..." -ForegroundColor Cyan
New-OpenSSLConfig -ConfigPath $configFile -Cuit $Cuit -CompanyName $CompanyName -Country $Country -State $State -City $City -Email $Email

# Generate private key and certificate
Write-Host "Generating private key and certificate..." -ForegroundColor Cyan
try {
    # Generate private key and certificate in one step
    & openssl req -new -x509 -keyout $privateKeyFile -out $certificateFile -days 730 -config $configFile -nodes 2>$null
    
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to generate certificate"
    }
    
    Write-Host "OK Private key and certificate generated" -ForegroundColor Green
} catch {
    Write-Error "Failed to generate certificate: $_"
    exit 1
}

# Create PKCS#12 file
Write-Host "Creating PKCS#12 file..." -ForegroundColor Cyan
try {
    & openssl pkcs12 -export -out $p12File -inkey $privateKeyFile -in $certificateFile -passout "pass:$Password" 2>$null
    
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create PKCS#12 file"
    }
    
    Write-Host "OK PKCS#12 file created" -ForegroundColor Green
} catch {
    Write-Error "Failed to create PKCS#12 file: $_"
    exit 1
}

# Verify the PKCS#12 file
Write-Host "Verifying PKCS#12 file..." -ForegroundColor Cyan
try {
    $verifyOutput = & openssl pkcs12 -info -in $p12File -noout -passin "pass:$Password" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK PKCS#12 file verified successfully" -ForegroundColor Green
    } else {
        Write-Warning "PKCS#12 file verification failed, but file was created"
    }
} catch {
    Write-Warning "Could not verify PKCS#12 file: $_"
}

# Clean up temporary files
Write-Host "Cleaning up temporary files..." -ForegroundColor Cyan
Remove-Item $configFile -Force -ErrorAction SilentlyContinue
Remove-Item $privateKeyFile -Force -ErrorAction SilentlyContinue
Remove-Item $certificateFile -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Certificate generation completed successfully!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Generated files:" -ForegroundColor Cyan
Write-Host "  PKCS#12 Certificate: $p12File" -ForegroundColor White
Write-Host ""
Write-Host "Certificate details:" -ForegroundColor Cyan
Write-Host "  CUIT: $Cuit" -ForegroundColor White
Write-Host "  Company: $CompanyName" -ForegroundColor White
Write-Host "  Country: $Country" -ForegroundColor White
Write-Host "  State: $State" -ForegroundColor White
Write-Host "  City: $City" -ForegroundColor White
Write-Host "  Email: $Email" -ForegroundColor White
Write-Host "  Password: $Password" -ForegroundColor White
Write-Host "  Validity: 2 years" -ForegroundColor White
Write-Host "  Key size: 2048 bits" -ForegroundColor White
Write-Host "  Signature algorithm: SHA256" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Upload the PKCS#12 file to AFIP's web services portal" -ForegroundColor White
Write-Host "2. Use the certificate in your AFIP integration" -ForegroundColor White
Write-Host "3. Keep the password secure - you'll need it for authentication" -ForegroundColor White
Write-Host ""
Write-Host "For testing with this SDK, update your configuration:" -ForegroundColor Yellow
Write-Host "  CertificatePath: '$p12File'" -ForegroundColor White
Write-Host "  CertificatePassword: '$Password'" -ForegroundColor White 