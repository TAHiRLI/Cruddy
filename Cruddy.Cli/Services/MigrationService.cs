using System.Text.Json;
using Cruddy.Cli.Models;
using Cruddy.Cli.Services.Base;

namespace Cruddy.Cli.Services
{
    /// <summary>
    /// Main service for managing migrations
    /// </summary>
    public class MigrationService : IMigrationService
    {
        private readonly IFileSystemService _fileSystem;
        private readonly MigrationGenerator _generator;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _migrationsPath;
        private readonly string _snapshotPath;

        public MigrationService(
            IFileSystemService fileSystem,
            string? migrationsPath = null,
            string? snapshotPath = null)
        {
            _fileSystem = fileSystem;
            _generator = new MigrationGenerator(fileSystem);
            
            var currentDir = _fileSystem.GetCurrentDirectory();
            _migrationsPath = migrationsPath ?? _fileSystem.CombinePaths(currentDir, ".cruddy", "migrations");
            _snapshotPath = snapshotPath ?? _fileSystem.CombinePaths(currentDir, ".cruddy", "snapshot.json");
            
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public bool IsMigrationDirectoryInitialized()
        {
            return _fileSystem.DirectoryExists(_migrationsPath);
        }

        public async Task<string> CreateMigrationAsync(string migrationName)
        {
            EnsureInitialized();

            // Get current snapshot
            var snapshot = await GetSnapshotAsync();

            // For now, create an empty migration
            // TODO: Implement entity scanning and change detection
            var changes = new List<MigrationChange>();

            // Create the migration file
            var filePath = await _generator.CreateMigrationFileAsync(
                _migrationsPath, 
                migrationName, 
                changes);

            // Update snapshot with new migration
            var migrationId = _generator.GenerateMigrationId(migrationName);
            snapshot.LastMigration = migrationId;
            snapshot.AppliedMigrations.Add(migrationId);
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
                var migration = JsonSerializer.Deserialize<Migration>(json, _jsonOptions);
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
            return JsonSerializer.Deserialize<Snapshot>(json, _jsonOptions) ?? new Snapshot();
        }

        private async Task SaveSnapshotAsync(Snapshot snapshot)
        {
            var json = JsonSerializer.Serialize(snapshot, _jsonOptions);
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