using Cruddy.Core.Models;

namespace Cruddy.Cli.Models
{
    /// <summary>
    /// Represents the current complete state of all entities and applied migrations
    /// </summary>
    public class Snapshot
    {
        public string Version { get; set; } = "1.0.0";
        public string? LastMigration { get; set; }
        public List<string> AppliedMigrations { get; set; } = new();
        public List<EntityMetadata> Entities { get; set; } = new();
    }
}