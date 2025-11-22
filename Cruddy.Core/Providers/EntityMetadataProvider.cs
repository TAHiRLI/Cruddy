using Cruddy.Core.Extensions;
using Cruddy.Core.Interfaces;
using Cruddy.Core.Models;

namespace Cruddy.Core.Providers;

/// <summary>
/// Implementation of metadata provider
/// </summary>
public class EntityMetadataProvider : IEntityMetadataProvider
{
    private readonly List<EntityMetadata> _metadata;
    public CruddyOptions Options { get; }

    public EntityMetadataProvider(List<EntityMetadata> metadata, CruddyOptions options)
    {
        _metadata = metadata;
        Options = options;
    }

    public List<EntityMetadata> GetAllMetadata() => _metadata;

    public EntityMetadata? GetMetadata<TEntity>() where TEntity : class
    {
        return _metadata.FirstOrDefault(m => m.ClrType == typeof(TEntity));
    }

    public EntityMetadata? GetMetadata(string entityName)
    {
        return _metadata.FirstOrDefault(m =>
            m.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));
    }
}