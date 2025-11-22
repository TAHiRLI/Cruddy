using Cruddy.Core.Extensions;
using Cruddy.Core.Models;

namespace Cruddy.Core.Interfaces;

/// <summary>
/// Interface for accessing scanned entity metadata
/// </summary>
public interface IEntityMetadataProvider
{
    /// <summary>
    /// Get all metadata discovered by Cruddy
    /// </summary>
    List<EntityMetadata> GetAllMetadata();

    /// <summary>
    /// Get metadata for a specific entity type
    /// </summary>
    EntityMetadata? GetMetadata<TEntity>() where TEntity : class;

    /// <summary>
    /// Get metadata for an entity by its name
    /// </summary>
    EntityMetadata? GetMetadata(string entityName);

    /// <summary>
    /// Options used to configure Cruddy scanning
    /// </summary>
    CruddyOptions Options { get; }
}