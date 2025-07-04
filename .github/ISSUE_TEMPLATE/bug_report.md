---
name: Bug report
about: Create a report to help us improve
title: '[BUG] '
labels: bug
assignees: ''

---

**Describe the bug**
A clear and concise description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior:
1. Configure the SDK with '...'
2. Call method '....'
3. Pass parameters '....'
4. See error

**Expected behavior**
A clear and concise description of what you expected to happen.

**Code Sample**
If applicable, add a minimal code sample to help explain your problem.

```csharp
// Your code here
var client = AfipClient.CreateForTesting(cuit, certPath, certPassword);
var result = await client.ElectronicInvoicing.AuthorizeInvoiceAsync(request);
```

**Error Details**
If applicable, add error messages, exception details, or screenshots.

```
Exception type: AfipException
Message: ...
Stack trace: ...
```

**Environment (please complete the following information):**
 - OS: [e.g. Windows 10, Ubuntu 20.04]
 - .NET Version: [e.g. .NET 6.0, .NET Framework 4.8]
 - SDK Version: [e.g. 1.0.0]
 - AFIP Environment: [Testing/Production]

**AFIP Configuration**
 - Invoice Type: [e.g. Invoice C (11)]
 - Point of Sale: [e.g. 1]
 - Currency: [e.g. PES, DOL]
 - Document Type: [e.g. DNI (96), CUIT (80)]

**Additional context**
Add any other context about the problem here. Include information about:
- Is this happening consistently or intermittently?
- Are you using any specific AFIP regulations (e.g., RG 5616/2024)?
- Any recent changes to your configuration?