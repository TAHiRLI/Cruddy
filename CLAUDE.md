# CLAUDE.md - AI Assistant Guide for Cruddy

## Project Overview

**Cruddy** is a CRUD boilerplate generator for .NET & React that allows solo developers to skip repetitive CRUD code and focus on unique business logic. The project consists of two main components:

1. **Cruddy.Core** - A NuGet library (.NET 9.0) providing a fluent configuration API
2. **Cruddy.Cli** - A global .NET tool for code generation and project management

**Target Framework**: .NET 9.0
**Primary Language**: C# with nullable reference types enabled
**License**: MIT

---

## Repository Structure

```
/home/user/Cruddy/
├── Cruddy.sln                          # Solution file
├── README.md                           # User-facing documentation
├── LICENSE                             # MIT License
├── .gitignore                          # Standard .NET gitignore
│
├── Cruddy.Core/                        # Core library (NuGet package v1.1.4)
│   ├── Cruddy.Core.csproj             # Library project file
│   ├── Attributes/                     # Entity attributes (future use)
│   ├── Configuration/
│   │   ├── CruddyEntityConfig.cs      # Base class for entity configs
│   │   └── FluentBuilders/            # Fluent API builder classes
│   │       ├── EntityBuilder.cs       # Entity-level configuration
│   │       ├── PropertyBuilder.cs     # Property-level configuration
│   │       ├── RelationshipBuilder.cs # One-to-one, one-to-many, many-to-one
│   │       └── ManyToManyRelationshipBuilder.cs
│   ├── Extensions/
│   │   └── DependencyInjectionExtensions.cs  # AddCruddy() extension
│   ├── Interfaces/
│   │   └── IEntityMetadataProvider.cs # Provider interface
│   ├── Models/
│   │   └── Metadata.cs                # EntityMetadata, PropertyMetadata, etc.
│   ├── Providers/
│   │   └── EntityMetadataProvider.cs  # In-memory metadata provider
│   └── Scanner/
│       └── ConfigurationScanner.cs    # Assembly scanning & conventions
│
├── Cruddy.Cli/                        # CLI tool (v0.1.2)
│   ├── Cruddy.Cli.csproj             # CLI project file (packaged as tool)
│   ├── Program.cs                     # Entry point & command dispatcher
│   ├── Commands/                      # CLI command implementations
│   │   ├── CheckCommand.cs           # Validates config
│   │   ├── InitCommand.cs            # Initializes cruddy.config.json
│   │   └── MigrationsCommand.cs      # Manages migrations
│   ├── Core/
│   │   └── ICommand.cs               # Command interface
│   ├── Helpers/
│   │   └── ConsoleHelper.cs          # Console output utilities
│   ├── Models/
│   │   ├── MigrationModel.cs         # Migration data structures
│   │   └── SnapshotModel.cs          # Snapshot data structures
│   └── Services/
│       ├── IFileSystemService.cs      # File operations abstraction
│       ├── FileSystemService.cs       # Implementation
│       ├── IMigrationService.cs       # Migration management
│       ├── MigrationService.cs        # Implementation
│       └── MigrationGenerator.cs      # Migration file creation
│
└── docs/                              # Documentation
    ├── cli-usage.md
    ├── changelog.md
    ├── customization.md
    ├── contribution.md
    ├── intro.md
    └── developer/                     # Architecture & conventions (mostly empty)
        ├── architecture.md
        ├── conventions.md
        └── reglaments.md
```

---

## Core Architecture & Design Patterns

### 1. Fluent Builder Pattern (Primary Design)

Inspired by Entity Framework Core and FluentValidation, the fluent API provides a type-safe, expression-based configuration system:

```csharp
public class UserCruddyConfig : CruddyEntityConfig<User>
{
    public UserCruddyConfig()
    {
        ForEntity()
            .HasDisplayName("User")
            .HasPluralName("Users")
            .HasIcon("user")
            .HasDefaultSort(x => x.CreatedAt, descending: true);

        ForProperty(x => x.Name)
            .IsRequired()
            .HasDisplayName("Full Name")
            .HasMaxLength(100)
            .ShowInList()
            .ShowInForm();

        ForProperty(x => x.Email)
            .HasFieldType("email")
            .IsRequired()
            .ShowInList();

        Ignore(x => x.PasswordHash);
    }
}
```

**Key Implementation Details:**
- `CruddyEntityConfig<TEntity>` is the base class users inherit from (Cruddy.Core/Configuration/CruddyEntityConfig.cs:11)
- `ForEntity()` returns an `EntityBuilder<TEntity>` for entity-level config (CruddyEntityConfig.cs:23)
- `ForProperty<TProperty>(Expression<Func<TEntity, TProperty>>)` provides property-level config (CruddyEntityConfig.cs:31)
- Expression trees are used for type safety and compile-time checking (CruddyEntityConfig.cs:55)
- `GetMetadata()` internal method builds `EntityMetadata` from configuration (CruddyEntityConfig.cs:50)

### 2. Convention over Configuration

The `ConfigurationScanner` applies intelligent defaults when properties aren't explicitly configured:

**Conventions Applied:**
- `Email` properties → `FieldType = "email"` (ConfigurationScanner.cs:168)
- `DateTime` properties → `Format = "date"` (ConfigurationScanner.cs:178)
- `CreatedAt`/`UpdatedAt` properties → `IsReadOnly = true` (ConfigurationScanner.cs:183)
- `string` properties → `MaxLength = 255` (ConfigurationScanner.cs:191)
- Entity names → `PluralName = Name + "s"` (ConfigurationScanner.cs:87)
- All properties get default `FieldType = "text"` if not specified (ConfigurationScanner.cs:156)

**How It Works:**
1. Scanner discovers all `CruddyEntityConfig<T>` implementations via reflection (ConfigurationScanner.cs:20)
2. Creates instances using `Activator.CreateInstance()` (ConfigurationScanner.cs:29)
3. Invokes `GetMetadata()` to extract configured metadata (ConfigurationScanner.cs:38)
4. Applies conventions to unconfigured properties (ConfigurationScanner.cs:41)
5. Returns list of `EntityMetadata` objects (ConfigurationScanner.cs:52)

### 3. Metadata-Driven Code Generation

**Flow:** Configuration → Metadata Objects → JSON Serialization → Code Generation

**Key Metadata Classes** (Cruddy.Core/Models/Metadata.cs):
- `EntityMetadata` (line 6): Entity name, display name, icon, sort config, properties, relationships
- `PropertyMetadata` (line 23): Name, type, display name, validation rules, UI behavior, field type
- `RelationshipMetadata` (line 50): Name, target entity, foreign key, relationship type
- `SortConfiguration` (line 77): Field name, sort direction
- `RelationType` enum (line 67): OneToOne, OneToMany, ManyToOne, ManyToMany

### 4. Command Pattern for CLI

All CLI commands implement `ICommand` interface (Cruddy.Cli/Core/ICommand.cs):
- `Name`: Command name for routing
- `ExecuteAsync(string[] args)`: Async execution method

**Current Commands** (registered in Program.cs:17):
- `CheckCommand`: Validates cruddy.config.json
- `InitCommand`: Initializes project configuration
- `MigrationsCommand`: Manages migrations

**Command Dispatcher** (Program.cs:24):
- Reads first argument as command name
- Finds matching command via LINQ
- Passes remaining arguments to command
- Simple string-based routing

### 5. File Differentiation Strategy

Generated files use `.cruddy.tsx` extension to distinguish from user customizations:

```
components/
├── UserList.cruddy.tsx      # GENERATED - Regenerated on each run
├── UserList.tsx             # CUSTOMIZED - User's version (takes precedence)
├── UserForm.cruddy.tsx      # GENERATED - Not customized
└── ProductList.cruddy.tsx   # GENERATED
```

**Rules:**
- `.cruddy.tsx` files are always regenerated
- `.tsx` files (without .cruddy) are never touched by generator
- Users copy `.cruddy.tsx` → `.tsx` to customize
- Import system prefers `.tsx` over `.cruddy.tsx`

### 6. Dependency Injection

**Core Library DI:**
```csharp
services.AddCruddy(options =>
{
    options.ScanAssembly(Assembly.GetExecutingAssembly());
    options.DefaultDateFormat = "MM/dd/yyyy";
    options.DefaultPageSize = 25;
});
```

**Registered Services:**
- `IEntityMetadataProvider` → `EntityMetadataProvider` (singleton)
- Holds scanned metadata and global options

**CLI DI:**
- Services use constructor injection with overloads
- Production constructor: parameterless (creates own dependencies)
- Test constructor: accepts interfaces for mocking
- Example: `MigrationsCommand()` vs `MigrationsCommand(IMigrationService service)`

---

## Development Workflows

### Adding a New Fluent API Method

**Example:** Add `.HasPlaceholder()` to `PropertyBuilder`

1. **Add to PropertyBuilder** (Cruddy.Core/Configuration/FluentBuilders/PropertyBuilder.cs):
   ```csharp
   public PropertyBuilder<TEntity, TProperty> HasPlaceholder(string placeholder)
   {
       _metadata.Placeholder = placeholder;
       return this;
   }
   ```

2. **Ensure PropertyMetadata has field** (Cruddy.Core/Models/Metadata.cs:28):
   ```csharp
   public string Placeholder { get; set; } = string.Empty;
   ```

3. **Apply conventions if needed** (Cruddy.Core/Scanner/ConfigurationScanner.cs):
   - Add logic to `ApplyConventionsToProperty()` or `ApplyConventionsToExistingProperty()`

4. **Update README examples** if this is a significant user-facing feature

### Adding a New CLI Command

**Example:** Add `cruddy generate` command

1. **Create command class** (Cruddy.Cli/Commands/GenerateCommand.cs):
   ```csharp
   public class GenerateCommand : ICommand
   {
       public string Name => "generate";

       public async Task ExecuteAsync(string[] args)
       {
           // Implementation
       }
   }
   ```

2. **Register in Program.cs** (line 17):
   ```csharp
   var commands = new List<ICommand>
   {
       new CheckCommand(),
       new InitCommand(),
       new MigrationsCommand(),
       new GenerateCommand(), // Add here
   };
   ```

3. **Add any required services** (Cruddy.Cli/Services/):
   - Create interface (e.g., `ICodeGenerationService.cs`)
   - Create implementation
   - Use constructor injection in command

### Adding a New Convention

**Example:** Make properties ending with "Phone" use `FieldType = "tel"`

Edit `ConfigurationScanner.cs` (Cruddy.Core/Scanner/ConfigurationScanner.cs):

```csharp
private void ApplyConventionsToProperty(PropertyMetadata propertyMetadata, PropertyInfo property)
{
    // Existing conventions...

    // New convention: Phone numbers
    if (property.Name.EndsWith("Phone", StringComparison.OrdinalIgnoreCase))
    {
        propertyMetadata.FieldType = "tel";
    }
    // ... rest of method
}
```

Also update `ApplyConventionsToExistingProperty()` for consistency.

### Working with Metadata

**Reading Metadata:**
```csharp
// Via DI
var provider = serviceProvider.GetRequiredService<IEntityMetadataProvider>();
var metadata = provider.GetMetadata();

// Via Scanner directly
var scanner = new ConfigurationScanner();
var metadata = scanner.ScanAssembly(Assembly.GetExecutingAssembly());
```

**Metadata Structure:**
```csharp
EntityMetadata
├── Name: "User"
├── DisplayName: "User"
├── PluralName: "Users"
├── ClrType: typeof(User)
├── Icon: "user"
├── DefaultSort: { Field: "CreatedAt", Descending: true }
├── Properties: List<PropertyMetadata>
│   └── PropertyMetadata
│       ├── Name: "Email"
│       ├── ClrType: typeof(string)
│       ├── DisplayName: "Email"
│       ├── FieldType: "email"
│       ├── IsRequired: true
│       ├── MaxLength: 255
│       ├── ShowInList: true
│       └── ShowInForm: true
├── Relationships: List<RelationshipMetadata>
└── IgnoredProperties: ["PasswordHash"]
```

---

## Key Files Reference

### Essential Files to Understand

**Core Configuration System:**
- `Cruddy.Core/Configuration/CruddyEntityConfig.cs` - Base class for all entity configs
- `Cruddy.Core/Configuration/FluentBuilders/EntityBuilder.cs` - Entity-level fluent API
- `Cruddy.Core/Configuration/FluentBuilders/PropertyBuilder.cs` - Property-level fluent API
- `Cruddy.Core/Models/Metadata.cs` - All metadata classes

**Convention System:**
- `Cruddy.Core/Scanner/ConfigurationScanner.cs` - Assembly scanning & convention application

**CLI System:**
- `Cruddy.Cli/Program.cs` - Entry point & command dispatcher
- `Cruddy.Cli/Core/ICommand.cs` - Command interface
- `Cruddy.Cli/Commands/*.cs` - Individual command implementations

**Service Layer:**
- `Cruddy.Cli/Services/IFileSystemService.cs` - File operations abstraction
- `Cruddy.Cli/Services/IMigrationService.cs` - Migration management
- `Cruddy.Cli/Services/MigrationGenerator.cs` - Migration file creation

### Configuration Files

**Project Configuration:**
- `Cruddy.Core/Cruddy.Core.csproj` - Core library config (NuGet package settings)
- `Cruddy.Cli/Cruddy.Cli.csproj` - CLI tool config (`<PackAsTool>true</PackAsTool>`)

**Runtime Configuration:**
- `cruddy.config.json` - Generated by `cruddy init`, user-facing config:
  ```json
  {
    "backend": { "path": "./MyApp.Api" },
    "frontend": {
      "path": "./client/src",
      "outputDir": "./client/src/components",
      "baseUrl": "/api"
    },
    "generate": {
      "extension": ".cruddy.tsx",
      "templatePath": "./.cruddy/templates/react-ts/"
    },
    "customized": []
  }
  ```

**Build Artifacts:**
- `.cruddy/snapshot.json` - Current state of entities
- `.cruddy/migrations/*.json` - Migration files with timestamps

---

## Coding Conventions & Best Practices

### C# Conventions

1. **Nullable Reference Types**: Enabled project-wide
   - Use `string?` for nullable strings
   - Use `= null!` for properties initialized in constructor
   - Use `= string.Empty` for non-nullable string defaults

2. **Expression Trees**: Use for type-safe property access
   ```csharp
   // ✅ Good
   ForProperty(x => x.Email)

   // ❌ Bad
   ForProperty("Email")  // String-based, no compile-time checking
   ```

3. **Fluent Methods**: Always return `this` or the builder
   ```csharp
   public PropertyBuilder<TEntity, TProperty> IsRequired()
   {
       _metadata.IsRequired = true;
       return this;  // Enable method chaining
   }
   ```

4. **Access Modifiers**:
   - Public: User-facing API surface
   - Protected: Accessible in derived configurations
   - Internal: Used within same assembly (e.g., `GetMetadata()`)
   - Private: Implementation details

5. **Async/Await**: Commands use async pattern
   ```csharp
   public async Task ExecuteAsync(string[] args)
   {
       // Even if not truly async, keeps interface consistent
       await Task.CompletedTask;
   }
   ```

### Naming Conventions

- **Classes**: PascalCase (e.g., `EntityBuilder`, `ConfigurationScanner`)
- **Methods**: PascalCase (e.g., `HasDisplayName`, `ApplyConventions`)
- **Properties**: PascalCase (e.g., `DisplayName`, `IsRequired`)
- **Private fields**: camelCase with underscore (e.g., `_metadata`)
- **Parameters**: camelCase (e.g., `propertyExpression`, `assemblyPath`)
- **Local variables**: camelCase (e.g., `configType`, `entityMetadata`)

### Project-Specific Conventions

1. **Fluent API Methods**:
   - Start with verbs: `Has`, `Is`, `Show`, `With`
   - Be descriptive: `HasDisplayName()` not `DisplayName()`
   - Group logically: display, validation, UI behavior, field types

2. **Builder Pattern**:
   - Store partial state in `_metadata` field
   - Build final object in `Build()` method
   - Return builder from each method for chaining

3. **Metadata Classes**:
   - Initialize collections: `= new()` or `= new List<>()`
   - Initialize strings: `= string.Empty`
   - Use nullable types sparingly: `int?` only when value can be unset

4. **Error Handling**:
   - Use exceptions for configuration errors
   - Use try-catch in scanner for resilience
   - Log errors to console in CLI commands
   - Provide helpful error messages

---

## Testing Strategy

### Current State
- **No test projects exist yet**
- Infrastructure is test-friendly (DI, interfaces, no static state)

### Recommended Testing Approach

**Unit Tests:**
```csharp
// Test fluent builders
[Fact]
public void PropertyBuilder_IsRequired_SetsIsRequiredTrue()
{
    var builder = new PropertyBuilder<User, string>("Email");
    builder.IsRequired();
    var metadata = builder.Build();
    Assert.True(metadata.IsRequired);
}

// Test conventions
[Fact]
public void ConfigurationScanner_AppliesEmailConvention()
{
    var scanner = new ConfigurationScanner();
    var metadata = scanner.ScanAssembly(typeof(TestEntity).Assembly);
    var emailProperty = metadata[0].Properties.First(p => p.Name == "Email");
    Assert.Equal("email", emailProperty.FieldType);
}

// Test CLI commands with mocks
[Fact]
public async Task MigrationsCommand_CreatesSnapshotFile()
{
    var mockFileSystem = new Mock<IFileSystemService>();
    var mockMigrationService = new Mock<IMigrationService>();
    var command = new MigrationsCommand(mockMigrationService.Object);

    await command.ExecuteAsync(new[] { "create", "test" });

    mockMigrationService.Verify(m => m.CreateMigration("test"), Times.Once);
}
```

**Integration Tests:**
- Test full scanning → metadata → JSON serialization pipeline
- Test CLI commands with temporary directories
- Test convention application with real entity classes

**Test Projects to Create:**
- `Cruddy.Core.Tests` - Unit tests for core library
- `Cruddy.Cli.Tests` - Unit tests for CLI commands and services
- `Cruddy.IntegrationTests` - End-to-end workflow tests

---

## Common Tasks for AI Assistants

### 1. Adding a New Property to Metadata

**Steps:**
1. Add property to appropriate metadata class (EntityMetadata or PropertyMetadata)
2. Update builder class to include fluent method
3. Update conventions if applicable
4. Test that metadata is properly serialized

**Example:**
```csharp
// 1. Add to PropertyMetadata
public string IconName { get; set; } = string.Empty;

// 2. Add to PropertyBuilder
public PropertyBuilder<TEntity, TProperty> HasIcon(string icon)
{
    _metadata.IconName = icon;
    return this;
}
```

### 2. Debugging Configuration Issues

**Check:**
1. Is the configuration class properly inheriting from `CruddyEntityConfig<T>`?
2. Is the assembly being scanned correctly?
3. Are conventions being applied? (Check ConfigurationScanner.cs)
4. Is the entity type generic parameter correct?

**Debugging Commands:**
```bash
# Check what entities are discovered
cruddy scan-entities

# Validate configuration
cruddy check
```

### 3. Understanding Data Flow

**Configuration → Metadata → JSON → Code Generation:**
```
1. User writes UserCruddyConfig : CruddyEntityConfig<User>
   └── Calls ForProperty(x => x.Email).IsRequired()
       └── Stores in EntityBuilder._metadata

2. Scanner calls GetMetadata() via reflection
   └── Returns EntityMetadata object
       └── Applies conventions to fill gaps

3. CLI serializes metadata to JSON
   └── Stored in .cruddy/snapshot.json

4. Code generator (future) reads JSON
   └── Generates UserList.cruddy.tsx
   └── Generates UserForm.cruddy.tsx
```

### 4. Adding New Relationship Types

Currently supported: OneToOne, OneToMany, ManyToOne, ManyToMany

**To add a new relationship feature:**
1. Update `RelationshipMetadata` class with new properties
2. Update `RelationshipBuilder` with fluent methods
3. Update `ManyToManyRelationshipBuilder` if applicable
4. Test with entity configuration

---

## Dependencies & External Libraries

### Cruddy.Core Dependencies
- **Microsoft.Extensions.DependencyInjection 9.0.0** - Only external dependency
- Standard .NET 9.0 libraries (System.Reflection, System.Linq.Expressions)

### Cruddy.Cli Dependencies
- **Cruddy.Core** - Project reference
- Standard .NET 9.0 libraries only
- No external NuGet packages

**Design Philosophy:** Keep dependencies minimal
- Easier to maintain
- Faster build times
- Fewer breaking changes
- Smaller package size

---

## Build & Deployment

### Building Locally

```bash
# Build entire solution
dotnet build Cruddy.sln

# Build core library
dotnet build Cruddy.Core/Cruddy.Core.csproj

# Build CLI tool
dotnet build Cruddy.Cli/Cruddy.Cli.csproj

# Run tests (when created)
dotnet test
```

### Packaging

```bash
# Pack core library (generates NuGet package)
dotnet pack Cruddy.Core/Cruddy.Core.csproj

# Pack CLI tool (generates tool package)
dotnet pack Cruddy.Cli/Cruddy.Cli.csproj
```

**Output:**
- `Cruddy.Core/bin/Release/Cruddy.Core.1.1.4.nupkg`
- `Cruddy.Cli/bin/Release/Cruddy.Cli.0.1.2.nupkg`

### Installing CLI Locally

```bash
# Install from local package
dotnet tool install --global --add-source ./Cruddy.Cli/bin/Release Cruddy.Cli

# Uninstall
dotnet tool uninstall --global Cruddy.Cli

# Update
dotnet tool update --global Cruddy.Cli
```

### Version Management

**Current Versions:**
- Cruddy.Core: 1.1.4
- Cruddy.Cli: 0.1.2

**Updating Versions:**
1. Edit `Cruddy.Core/Cruddy.Core.csproj`: `<Version>1.1.5</Version>`
2. Edit `Cruddy.Cli/Cruddy.Cli.csproj`: `<Version>0.1.3</Version>`
3. Update changelog
4. Commit with message: `chore: bump version to X.Y.Z`

---

## Important Notes for AI Assistants

### What NOT to Do

1. **Don't pollute entity classes** - Keep them clean, use separate config classes
2. **Don't use strings for property names** - Always use expression trees
3. **Don't add dependencies lightly** - Discuss first, keep it minimal
4. **Don't break existing conventions** - Add new ones carefully
5. **Don't create files without reading** - Understand before modifying
6. **Don't skip tests** - Write tests for new features
7. **Don't use attributes** - This project uses fluent configuration exclusively

### What TO Do

1. **Read existing code first** - Understand patterns before adding
2. **Follow fluent API style** - Method chaining, return builders
3. **Apply conventions** - Think about smart defaults
4. **Use DI properly** - Constructor injection, interface-based
5. **Write descriptive names** - Clarity over brevity
6. **Add XML comments** - Document public APIs
7. **Test thoroughly** - Unit tests + integration tests

### Code Generation Status

**Current State:**
- ✅ Fluent API implemented
- ✅ Configuration scanning works
- ✅ Metadata models complete
- ✅ CLI structure in place
- ⏳ Code generation not implemented yet
- ⏳ Template system not designed yet
- ⏳ React component generation pending

**When implementing code generation:**
- Read configuration from snapshot.json
- Use template engine (e.g., Scriban, Handlebars, or T4)
- Generate files with `.cruddy.tsx` extension
- Never overwrite `.tsx` files without `.cruddy` in name
- Track generated files for diff capabilities

---

## Git Workflow

### Current Branch
- **Main branch**: `master` or `main` (check with `git branch`)
- **Development branch**: Feature branches with `claude/` prefix for AI-assisted work

### Commit Message Conventions

Follow conventional commits:
```
feat: fluent builders configured
fix: scan entity config discovery
docs: update README with new conventions
chore: bump version to 1.2.0
refactor: simplify metadata scanning logic
test: add tests for PropertyBuilder
```

### Making Changes

1. **Before making changes:**
   - Read relevant files to understand current implementation
   - Check for existing patterns to follow
   - Verify you're on the correct branch

2. **After making changes:**
   - Test locally if possible
   - Commit with clear, descriptive messages
   - Don't commit multiple unrelated changes together

3. **Push changes:**
   ```bash
   git add .
   git commit -m "feat: add HasIcon method to PropertyBuilder"
   git push -u origin <branch-name>
   ```

---

## Quick Reference

### File Locations

| What | Where |
|------|-------|
| Base config class | `Cruddy.Core/Configuration/CruddyEntityConfig.cs` |
| Metadata models | `Cruddy.Core/Models/Metadata.cs` |
| Convention logic | `Cruddy.Core/Scanner/ConfigurationScanner.cs` |
| CLI entry point | `Cruddy.Cli/Program.cs` |
| Commands | `Cruddy.Cli/Commands/*.cs` |
| Services | `Cruddy.Cli/Services/*.cs` |
| Builders | `Cruddy.Core/Configuration/FluentBuilders/*.cs` |

### Key Commands

```bash
# Build
dotnet build

# Pack NuGet packages
dotnet pack

# Install CLI globally
dotnet tool install -g cruddy

# Run CLI
cruddy init
cruddy check
cruddy migrations create <name>
```

### Useful Git Commands

```bash
# Check status
git status

# View recent commits
git log --oneline -10

# View changes
git diff

# Create new branch
git checkout -b feature/new-feature
```

---

## Resources

- **README.md**: User-facing documentation and examples
- **docs/**: Additional documentation (mostly empty, needs expansion)
- **.NET 9.0 Docs**: https://learn.microsoft.com/en-us/dotnet/
- **Expression Trees**: https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/expression-trees/

---

## Recent Changes

**Git Log:**
```
2486116 feat: fluent builders configured
733eefd feat: discusson on migration generation strategy
e525c4d fix: scan entity config discovery
f3213b0 first commit
```

**Current State:**
- Fluent builder API is configured and working
- Migration strategy has been discussed and designed
- Entity config discovery has been fixed
- Ready for code generation implementation

---

## Questions to Ask When Unsure

1. **Does this follow the fluent API pattern?** (Method chaining, return builder)
2. **Does this break existing conventions?** (Check ConfigurationScanner.cs)
3. **Is this type-safe?** (Use expressions, not strings)
4. **Is this testable?** (Use DI, interfaces, avoid static state)
5. **Does this add unnecessary dependencies?** (Keep it minimal)
6. **Does this change public API?** (Consider backward compatibility)
7. **Does this need documentation?** (XML comments, README update)

---

*This document was created to help AI assistants understand the Cruddy codebase structure, conventions, and development workflows. Keep it updated as the project evolves.*

**Last Updated:** 2025-11-29
**Version:** 1.0.0
