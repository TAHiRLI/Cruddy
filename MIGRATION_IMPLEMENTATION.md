# Migration Generation Implementation

## Architecture Overview

The migration system is designed with **separation of concerns** and **testability** in mind. Each component has a single responsibility and dependencies are injected through interfaces.

## Component Structure

```
Commands/
├── MigrationsCommand.cs          # CLI command handler (thin layer)

Services/
├── IMigrationService.cs          # Service interface
├── MigrationService.cs           # Core migration logic
├── MigrationGenerator.cs         # Migration file generation
├── IFileSystemService.cs         # File system abstraction
└── FileSystemService.cs          # File system implementation

Models/
├── Migration.cs                  # Migration data structures
├── Snapshot.cs                   # Snapshot model
└── CruddyConfig.cs              # Configuration model

Helpers/
└── ConsoleHelper.cs             # Console output utilities
```

## Design Principles

### 1. Single Responsibility Principle
Each class has one clear purpose:
- `MigrationsCommand`: Handles CLI argument parsing and user interaction
- `MigrationService`: Orchestrates migration operations
- `MigrationGenerator`: Creates migration files
- `FileSystemService`: Abstracts file I/O operations
- `ConsoleHelper`: Manages console output formatting

### 2. Dependency Injection
Services are injected through constructor parameters, enabling:
- Easy testing with mock implementations
- Flexibility to swap implementations
- Clear dependency relationships

### 3. Interface Segregation
Interfaces define clear contracts:
- `IMigrationService`: Migration operations
- `IFileSystemService`: File system operations
- `ICommand`: Command execution

### 4. Modular Design
Components are loosely coupled and can be:
- Tested independently
- Modified without affecting others
- Reused in different contexts

## Usage Examples

### Creating a Migration
```bash
cruddy migrations add InitialCreate
```

**Flow:**
1. `MigrationsCommand.AddMigration()` validates input
2. Calls `IMigrationService.CreateMigrationAsync()`
3. `MigrationService` uses `MigrationGenerator` to create file
4. Snapshot is updated with new migration
5. Success message displayed via `ConsoleHelper`

### Removing a Migration
```bash
cruddy migrations remove
```

**Flow:**
1. `MigrationsCommand.RemoveMigration()` initiates removal
2. `MigrationService` identifies last migration
3. File is deleted via `IFileSystemService`
4. Snapshot is updated

### Listing Migrations
```bash
cruddy migrations list
```

**Flow:**
1. `MigrationsCommand.ListMigrations()` requests list
2. `MigrationService` reads all migration files
3. Parses JSON into `Migration` objects
4. Displays formatted list via `ConsoleHelper`

## Key Features

### ✅ Clean Separation
- Business logic separated from CLI concerns
- File operations abstracted behind interface
- Output formatting centralized

### ✅ Testable
- All dependencies can be mocked
- Pure business logic in services
- No direct Console or File access in business logic

### ✅ Extensible
- Easy to add new migration types
- Simple to implement change detection
- Can add validation rules without modifying core logic

### ✅ Error Handling
- Validation at command level
- Exception handling with clear messages
- Graceful failure scenarios

## Migration File Structure

```json
{
  "version": "1.0.0",
  "timestamp": "2025-11-27T10:30:45Z",
  "name": "AddUserEmail",
  "migrationId": "20251127103045_AddUserEmail",
  "changes": [
    {
      "type": "field_added",
      "entityName": "User",
      "field": {
        "name": "email",
        "type": "string",
        "isRequired": true
      }
    }
  ]
}
```

## Snapshot Structure

```json
{
  "version": "1.0.0",
  "lastMigration": "20251127103045_AddUserEmail",
  "appliedMigrations": [
    "20251127103000_InitialCreate",
    "20251127103045_AddUserEmail"
  ],
  "entities": [...]
}
```

## Future Enhancements

### Entity Scanning (TODO)
The system is prepared to scan entities from your codebase:
```csharp
// TODO: Implement in MigrationGenerator
public List<EntityDefinition> ScanEntities(string assemblyPath)
{
    // Use reflection to discover entities
    // Parse attributes for metadata
    // Return entity definitions
}
```

### Change Detection (TODO)
Compare current entities with snapshot:
```csharp
// TODO: Implement in MigrationGenerator
public List<MigrationChange> DetectChanges(
    List<EntityDefinition> current, 
    List<EntityDefinition> snapshot)
{
    // Detect added entities
    // Detect removed entities
    // Detect modified fields
    // Return list of changes
}
```

### Migration Application
Apply migrations to generate components:
```csharp
// TODO: Implement new service
public interface IMigrationApplicator
{
    Task ApplyMigrationsAsync();
}
```

## Testing Strategy

### Unit Tests
Each service can be tested independently:
```csharp
[Test]
public async Task CreateMigration_ValidName_CreatesFile()
{
    // Arrange
    var mockFileSystem = new Mock<IFileSystemService>();
    var service = new MigrationService(mockFileSystem.Object);
    
    // Act
    await service.CreateMigrationAsync("TestMigration");
    
    // Assert
    mockFileSystem.Verify(x => x.WriteFileAsync(
        It.IsAny<string>(), 
        It.IsAny<string>()), 
        Times.Once);
}
```

### Integration Tests
Test command execution end-to-end:
```csharp
[Test]
public async Task MigrationsCommand_Add_CreatesFileAndUpdatesSnapshot()
{
    // Test full workflow
}
```

## Benefits of This Architecture

1. **Maintainability**: Clear structure makes code easy to understand
2. **Testability**: Interfaces allow comprehensive testing
3. **Flexibility**: Easy to modify without breaking changes
4. **Scalability**: New features integrate smoothly
5. **Reliability**: Error handling at appropriate levels

## Next Steps

1. Implement entity scanning from assemblies
2. Implement change detection algorithm
3. Add migration validation
4. Create migration application logic
5. Build code generation pipeline
6. Add configuration file management