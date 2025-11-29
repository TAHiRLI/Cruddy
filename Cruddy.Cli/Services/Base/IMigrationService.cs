using Cruddy.Cli.Models;

namespace Cruddy.Cli.Services
{
    /// <summary>
    /// Service for managing migrations
    /// </summary>
    public interface IMigrationService
    {
        /// <summary>
        /// Creates a new migration with the given name
        /// </summary>
        Task<string> CreateMigrationAsync(string migrationName);

        /// <summary>
        /// Removes the last migration
        /// </summary>
        Task<bool> RemoveLastMigrationAsync();

        /// <summary>
        /// Lists all migrations
        /// </summary>
        Task<List<Migration>> ListMigrationsAsync();

        /// <summary>
        /// Gets the current snapshot
        /// </summary>
        Task<Snapshot> GetSnapshotAsync();

        /// <summary>
        /// Validates if migration directory exists and is initialized
        /// </summary>
        bool IsMigrationDirectoryInitialized();
    }
}