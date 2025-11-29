using System.Linq.Expressions;
using Cruddy.Core.Configuration.FluentBuilders;
using Cruddy.Core.Models;

namespace Cruddy.Core.Configuration;

/// <summary>
/// Base class for entity configuration using fluent API
/// </summary>
/// <typeparam name="TEntity">The entity type to configure</typeparam>
public abstract class CruddyEntityConfig<TEntity> where TEntity : class
{
    protected EntityBuilder<TEntity> EntityBuilder { get; }

    protected CruddyEntityConfig()
    {
        EntityBuilder = new EntityBuilder<TEntity>();
    }

    /// <summary>
    /// Configure the entity itself
    /// </summary>
    protected EntityBuilder<TEntity> ForEntity()
    {
        return EntityBuilder;
    }

    /// <summary>
    /// Configure a specific property
    /// </summary>
    protected PropertyBuilder<TEntity, TProperty> ForProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        var propertyName = GetPropertyName(propertyExpression);
        return EntityBuilder.GetPropertyBuilder<TProperty>(propertyName);
    }

    /// <summary>
    /// Ignore a property
    /// </summary>
    protected void Ignore<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        var propertyName = GetPropertyName(propertyExpression);
        EntityBuilder.IgnoreProperty(propertyName);
    }

    /// <summary>
    /// Get the metadata built from this configuration
    /// </summary>
    internal EntityMetadata GetMetadata()
    {
        return EntityBuilder.Build();
    }

    private static string GetPropertyName<TProperty>(Expression<Func<TEntity, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        throw new ArgumentException("Expression must be a member expression", nameof(expression));
    }
}