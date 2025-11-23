# Publishing Cruddy.Core

## Build and Pack

```bash
cd Cruddy.Core
dotnet pack --configuration Release
```

Output: `bin/Release/Cruddy.Core.0.1.0.nupkg`

## Use in CruddyTest

### Option 1: Local NuGet Source

```bash
cd CruddyTest.Core
dotnet add package Cruddy.Core --source /path/to/Cruddy/Cruddy.Core/bin/Release
```

### Option 2: Direct Reference

```bash
cd CruddyTest.Core
dotnet add reference /path/to/Cruddy/Cruddy.Core/Cruddy.Core.csproj
```

## Setup in CruddyTest.Api

### 1. Install Package
```bash
cd CruddyTest.Api
dotnet add package Cruddy.Core --source /path/to/Cruddy/Cruddy.Core/bin/Release
```

### 2. Create Configuration
```csharp
// CruddyTest.Core/Cruddy/EmployeeCruddyConfig.cs
using Cruddy.Core.Configuration;
using CruddyTest.Core.Entities;

namespace CruddyTest.Core.Cruddy;

public class EmployeeCruddyConfig : CruddyEntityConfig<Employee>
{
    public EmployeeCruddyConfig()
    {
        ForEntity()
            .HasDisplayName("Employee")
            .HasPluralName("Employees");

        ForProperty(x => x.Name)
            .IsRequired()
            .ShowInList()
            .ShowInForm();

        ForProperty(x => x.Email)
            .HasFieldType("email")
            .IsRequired();
    }
}
```

### 3. Register in Program.cs
```csharp
using Cruddy.Core.Extensions;
using CruddyTest.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCruddy(options =>
{
    options.ScanAssembly(typeof(Employee).Assembly);
    options.DefaultDateFormat = "MM/dd/yyyy";
    options.DefaultPageSize = 25;
});

// ... rest of your setup
```

### 4. Use Metadata (Optional)
```csharp
using Cruddy.Core.Interfaces;

app.MapGet("/api/metadata", (IEntityMetadataProvider provider) =>
{
    var metadata = provider.GetAllMetadata();
    return Results.Ok(metadata);
});
```

## Test It

```bash
cd CruddyTest.Api
dotnet run
```

The metadata provider will scan and build entity configurations at startup!