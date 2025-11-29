using System.Linq.Expressions;
using Cruddy.Core.Configuration.FluentBuilders.Base;
using Cruddy.Core.Models;

namespace Cruddy.Core.Configuration.FluentBuilders;

/// <summary>
/// Fluent builder for entity-level configuration
/// </summary>
public class EntityBuilder<TEntity> where TEntity : class
{
    private readonly EntityMetadata _metadata;
    private readonly Dictionary<string, IPropertyBuilder> _propertyBuilders;
    private readonly Dictionary<string, IRelationshipBuilder> _relationshipBuilders; // Add this

    public EntityBuilder()
    {
        _metadata = new EntityMetadata
        {
            Name = typeof(TEntity).Name,
            ClrType = typeof(TEntity),
            Relationships = new(),
        };
        _propertyBuilders = new Dictionary<string, IPropertyBuilder>();
        _relationshipBuilders = new Dictionary<string, IRelationshipBuilder>(); // Add this


    }

    public EntityBuilder<TEntity> HasDisplayName(string displayName)
    {
        _metadata.DisplayName = displayName;
        return this;
    }

    public EntityBuilder<TEntity> HasPluralName(string pluralName)
    {
        _metadata.PluralName = pluralName;
        return this;
    }

    public EntityBuilder<TEntity> HasDescription(string description)
    {
        _metadata.Description = description;
        return this;
    }

    public EntityBuilder<TEntity> HasIcon(string icon)
    {
        _metadata.Icon = icon;
        return this;
    }

    public EntityBuilder<TEntity> HasDefaultSort<TProperty>(
        Expression<Func<TEntity, TProperty>> field,
        bool descending = false)
    {
        var propertyName = GetPropertyName(field);
        _metadata.DefaultSort = new SortConfiguration
        {
            Field = propertyName,
            Descending = descending
        };
        return this;
    }

    /// <summary>
    /// Configure a one-to-many relationship (this entity has many related entities)
    /// Example: Department has many Employees
    /// </summary>
    public RelationshipBuilder<TEntity, TRelated> HasMany<TRelated>(
        Expression<Func<TEntity, ICollection<TRelated>?>> navigationProperty)
        where TRelated : class
    {
        var propertyName = GetPropertyName(navigationProperty);
        var relationship = new RelationshipMetadata
        {
            Name = propertyName,
            TargetEntity = typeof(TRelated).Name,
            Type = RelationType.OneToMany
        };

        var builder = new RelationshipBuilder<TEntity, TRelated>(relationship, this);
        _relationshipBuilders[propertyName] = builder;
        return builder;
    }

    /// <summary>
    /// Configure a many-to-one relationship (this entity belongs to one related entity)
    /// Example: Employee belongs to one Department
    /// </summary>
    public RelationshipBuilder<TEntity, TRelated> HasOne<TRelated>(
        Expression<Func<TEntity, TRelated?>> navigationProperty)
        where TRelated : class
    {
        var propertyName = GetPropertyName(navigationProperty);
        var relationship = new RelationshipMetadata
        {
            Name = propertyName,
            TargetEntity = typeof(TRelated).Name,
            Type = RelationType.ManyToOne
        };

        var builder = new RelationshipBuilder<TEntity, TRelated>(relationship, this);
        _relationshipBuilders[propertyName] = builder;
        return builder;
    }

    /// <summary>
    /// Configure a one-to-one relationship (this entity has one related entity)
    /// Example: Employee has one IdentityCard
    /// </summary>
    public RelationshipBuilder<TEntity, TRelated> HasOneToOne<TRelated>(
        Expression<Func<TEntity, TRelated?>> navigationProperty)
        where TRelated : class
    {
        var propertyName = GetPropertyName(navigationProperty);
        var relationship = new RelationshipMetadata
        {
            Name = propertyName,
            TargetEntity = typeof(TRelated).Name,
            Type = RelationType.OneToOne
        };

        var builder = new RelationshipBuilder<TEntity, TRelated>(relationship, this);
        _relationshipBuilders[propertyName] = builder;
        return builder;
    }

    /// <summary>
    /// Configure a many-to-many relationship 
    /// Example: Student has many Courses, Course has many Students
    /// </summary>
    public ManyToManyRelationshipBuilder<TEntity, TRelated> HasManyToMany<TRelated>(
        Expression<Func<TEntity, ICollection<TRelated>?>> navigationProperty)
        where TRelated : class
    {
        var propertyName = GetPropertyName(navigationProperty);
        var relationship = new RelationshipMetadata
        {
            Name = propertyName,
            TargetEntity = typeof(TRelated).Name,
            Type = RelationType.ManyToMany
        };

        var builder = new ManyToManyRelationshipBuilder<TEntity, TRelated>(relationship, this);
        _relationshipBuilders[propertyName] = builder;
        return builder;
    }


    internal PropertyBuilder<TEntity, TProperty> GetPropertyBuilder<TProperty>(string propertyName)
    {
        if (!_propertyBuilders.ContainsKey(propertyName))
        {
            var propertyInfo = typeof(TEntity).GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property {propertyName} not found on type {typeof(TEntity).Name}");
            }

            var propertyBuilder = new PropertyBuilder<TEntity, TProperty>(propertyName, propertyInfo.PropertyType);
            _propertyBuilders[propertyName] = propertyBuilder;
        }

        return (PropertyBuilder<TEntity, TProperty>)_propertyBuilders[propertyName];
    }

    internal void IgnoreProperty(string propertyName)
    {
        if (!_metadata.IgnoredProperties.Contains(propertyName))
        {
            _metadata.IgnoredProperties.Add(propertyName);
        }
    }

    internal EntityMetadata Build()
    {
        // Clear existing properties to avoid duplicates
        _metadata.Properties.Clear();
        _metadata.Relationships.Clear();

        // Add all configured properties
        foreach (var builder in _propertyBuilders.Values)
        {
            var propertyMetadata = builder.Build();
            _metadata.Properties.Add(propertyMetadata);
        }

        // Build all configured relationships
        foreach (var builder in _relationshipBuilders.Values)
        {
            var relationshipMetadata = builder.Build();
            _metadata.Relationships.Add(relationshipMetadata);
        }

        return _metadata;
    }

    private static string GetPropertyName<TProperty>(Expression<Func<TEntity, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        throw new ArgumentException("Expression must be a member expression");
    }
}
