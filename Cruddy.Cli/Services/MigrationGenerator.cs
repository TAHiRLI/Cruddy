using System.Text.Json;
using Cruddy.Cli.Models;
using Cruddy.Cli.Services.Base;
using Cruddy.Core.Models;

namespace Cruddy.Cli.Services
{
    /// <summary>
    /// Generates migration files by comparing current state with snapshot
    /// </summary>
    public class MigrationGenerator
    {
        private readonly IFileSystemService _fileSystem;
        private readonly JsonSerializerOptions _jsonOptions;

        public MigrationGenerator(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Generates a migration ID based on timestamp and name
        /// </summary>
        public string GenerateMigrationId(string migrationName)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"{timestamp}_{migrationName}";
        }

        /// <summary>
        /// Creates a migration file
        /// </summary>
        public async Task<string> CreateMigrationFileAsync(
            string migrationsPath, 
            string migrationName, 
            List<MigrationChange> changes)
        {
            var migrationId = GenerateMigrationId(migrationName);
            var migration = new Migration
            {
                Version = "1.0.0",
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Name = migrationName,
                MigrationId = migrationId,
                Changes = changes
            };

            var fileName = $"{migrationId}.json";
            var filePath = _fileSystem.CombinePaths(migrationsPath, fileName);
            
            var json = JsonSerializer.Serialize(migration, _jsonOptions);
            await _fileSystem.WriteFileAsync(filePath, json);

            return filePath;
        }

        /// <summary>
        /// Detects changes between current entities and snapshot
        /// </summary>
        public List<MigrationChange> DetectChanges(
            List<EntityMetadata> currentEntities, 
            List<EntityMetadata> snapshotEntities)
        {
            var changes = new List<MigrationChange>();

            // For now, return empty changes - this will be implemented
            // when we have entity scanning functionality
            // TODO: Implement change detection logic

            return changes;
        }
    }
}