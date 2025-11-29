using Cruddy.Core.Models;

namespace Cruddy.Cli.Models
{
    /// <summary>
    /// Represents a migration file containing changes to entities
    /// </summary>
    public class Migration
    {
        public string Version { get; set; } = "1.0.0";
        public string Timestamp { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string MigrationId { get; set; } = string.Empty;
        public List<MigrationChange> Changes { get; set; } = new();
    }

    /// <summary>
    /// Base class for migration changes
    /// </summary>
    public class MigrationChange
    {
        public string Type { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents adding a new entity
    /// </summary>
    public class EntityAddedChange : MigrationChange
    {
        public EntityMetadata Entity { get; set; } = new();
    }

    /// <summary>
    /// Represents removing an entity
    /// </summary>
    public class EntityRemovedChange : MigrationChange
    {
        // EntityName from base class is sufficient
    }

    /// <summary>
    /// Represents adding a field to an entity
    /// </summary>
    public class FieldAddedChange : MigrationChange
    {
        public PropertyMetadata Field { get; set; } = new();
    }

    /// <summary>
    /// Represents removing a field from an entity
    /// </summary>
    public class FieldRemovedChange : MigrationChange
    {
        public string FieldName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents modifying an existing field
    /// </summary>
    public class FieldModifiedChange : MigrationChange
    {
        public string FieldName { get; set; } = string.Empty;
        public Dictionary<string, FieldPropertyChange> Changes { get; set; } = new();
    }

    /// <summary>
    /// Tracks old and new values for a field property
    /// </summary>
    public class FieldPropertyChange
    {
        public object? Old { get; set; }
        public object? New { get; set; }
    }
}