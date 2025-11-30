using System.Text.Json;
using Cruddy.Cli.Helpers;
using Cruddy.Cli.Models;
using Cruddy.Cli.Services.Base;
using Cruddy.Core.Models;
using Cruddy.Core.Scanner;

namespace Cruddy.Cli.Services
{
    /// <summary>
    /// Main service for managing migrations
    /// </summary>
    public class MigrationService : IMigrationService
    {
        private readonly IFileSystemService _fileSystem;
        private readonly MigrationGenerator _generator;
        private readonly ConfigService _configService;
        private readonly AssemblyLoaderService _assemblyLoader;
        private readonly string _migrationsPath;
        private readonly string _snapshotPath;

        public MigrationService(
            IFileSystemService fileSystem,
            string? migrationsPath = null,
            string? snapshotPath = null)
        {
            _fileSystem = fileSystem;
            _generator = new MigrationGenerator(fileSystem);
            _configService = new ConfigService(fileSystem);
            _assemblyLoader = new AssemblyLoaderService(fileSystem);

            var currentDir = _fileSystem.GetCurrentDirectory();
            _migrationsPath = migrationsPath ?? _fileSystem.CombinePaths(currentDir, ".cruddy", "migrations");
            _snapshotPath = snapshotPath ?? _fileSystem.CombinePaths(currentDir, ".cruddy", "snapshot.json");
        }

        public bool IsMigrationDirectoryInitialized()
        {
            return _fileSystem.DirectoryExists(_migrationsPath);
        }

        public async Task<string> CreateMigrationAsync(string migrationName)
        {
            EnsureInitialized();

            // Load configuration
            var config = await _configService.LoadConfigAsync();

            // Load backend assembly and scan for entities
            var assemblies = await _assemblyLoader.LoadAllAssembliesAsync(config.Backend.Path);
            var scanner = new ConfigurationScanner();
            var currentEntities = new List<EntityMetadata>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var entities = scanner.ScanAssembly(assembly);
                    if (entities.Count > 0)
                    {
                        Console.WriteLine($"  Found {entities.Count} entity configuration(s) in {assembly.GetName().Name}");
                        currentEntities.AddRange(entities);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ⊗ Skipped {assembly.GetName().Name} ({ex.GetType().Name})");
                }

            }

            // Get current snapshot
            var snapshot = await GetSnapshotAsync();

            // Detect changes between current state and snapshot
            var changes = _generator.DetectChanges(currentEntities, snapshot.Entities);

            // Create the migration file
            var filePath = await _generator.CreateMigrationFileAsync(
                _migrationsPath,
                migrationName,
                changes);

            // Update snapshot with new migration and current entities
            var migrationId = _generator.GenerateMigrationId(migrationName);
            snapshot.LastMigration = migrationId;
            snapshot.AppliedMigrations.Add(migrationId);
            snapshot.Entities = currentEntities; // Update entities to current state
            await SaveSnapshotAsync(snapshot);

            return filePath;
        }

        public async Task<bool> RemoveLastMigrationAsync()
        {
            EnsureInitialized();

            var snapshot = await GetSnapshotAsync();

            if (snapshot.AppliedMigrations.Count == 0)
            {
                return false;
            }

            var lastMigrationId = snapshot.AppliedMigrations.Last();
            var migrationFile = _fileSystem.CombinePaths(_migrationsPath, $"{lastMigrationId}.json");

            if (_fileSystem.FileExists(migrationFile))
            {
                _fileSystem.DeleteFile(migrationFile);
            }

            snapshot.AppliedMigrations.RemoveAt(snapshot.AppliedMigrations.Count - 1);
            snapshot.LastMigration = snapshot.AppliedMigrations.Count > 0
                ? snapshot.AppliedMigrations.Last()
                : null;

            await SaveSnapshotAsync(snapshot);

            return true;
        }

        public async Task<List<Migration>> ListMigrationsAsync()
        {
            EnsureInitialized();

            var migrationFiles = _fileSystem.GetFiles(_migrationsPath, "*.json");
            var migrations = new List<Migration>();

            foreach (var file in migrationFiles)
            {
                var json = await _fileSystem.ReadFileAsync(file);
                var migration = JsonSerializer.Deserialize<Migration>(json, JsonHelper.GetOptions());
                if (migration != null)
                {
                    migrations.Add(migration);
                }
            }

            return migrations.OrderBy(m => m.MigrationId).ToList();
        }

        public async Task<Snapshot> GetSnapshotAsync()
        {
            if (!_fileSystem.FileExists(_snapshotPath))
            {
                return new Snapshot();
            }

            var json = await _fileSystem.ReadFileAsync(_snapshotPath);
            return JsonSerializer.Deserialize<Snapshot>(json, JsonHelper.GetOptions()) ?? new Snapshot();
        }

        private async Task SaveSnapshotAsync(Snapshot snapshot)
        {
            var json = JsonSerializer.Serialize(snapshot, JsonHelper.GetOptions());
            await _fileSystem.WriteFileAsync(_snapshotPath, json);
        }

        private void EnsureInitialized()
        {
            if (!IsMigrationDirectoryInitialized())
            {
                throw new InvalidOperationException(
                    "Migration directory not initialized. Run 'cruddy init' first.");
            }
        }
    }
}