using System.Linq.Expressions;
using Cruddy.Core.Models;

namespace Cruddy.Core.Configuration;

/// <summary>
/// Fluent builder for entity-level configuration
/// </summary>
public class EntityBuilder<TEntity> where TEntity : class
{
    private readonly EntityMetadata _metadata;
    private readonly Dictionary<string, object> _propertyBuilders;

    public EntityBuilder()
    {
        _metadata = new EntityMetadata
        {
            Name = typeof(TEntity).Name,
            ClrType = typeof(TEntity)
        };
        _propertyBuilders = new Dictionary<string, object>();
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
        // Add all configured properties
        foreach (var kvp in _propertyBuilders)
        {
            var builder = kvp.Value;
            var buildMethod = builder.GetType().GetMethod("Build");
            if (buildMethod != null)
            {
                var propertyMetadata = (PropertyMetadata)buildMethod.Invoke(builder, null)!;
                _metadata.Properties.Add(propertyMetadata);
            }
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

/// <summary>
/// Fluent builder for property-level configuration
/// </summary>
public class PropertyBuilder<TEntity, TProperty> where TEntity : class
{
    private readonly PropertyMetadata _metadata;

    internal PropertyBuilder(string propertyName, Type propertyType)
    {
        _metadata = new PropertyMetadata
        {
            Name = propertyName,
            ClrType = propertyType,
            DisplayName = propertyName // Default to property name
        };
    }

    public PropertyBuilder<TEntity, TProperty> HasDisplayName(string displayName)
    {
        _metadata.DisplayName = displayName;
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> HasPlaceholder(string placeholder)
    {
        _metadata.Placeholder = placeholder;
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> HasHelpText(string helpText)
    {
        _metadata.HelpText = helpText;
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> IsRequired(string? message = null)
    {
        _metadata.IsRequired = true;
        _metadata.RequiredMessage = message;
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> HasMaxLength(int length)
    {
        _metadata.MaxLength = length;
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> HasMinLength(int length)
    {
        _metadata.MinLength = length;
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> ShowInList(int? order = null)
    {
        _metadata.ShowInList = true;
        _metadata.ListOrder = order;
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> ShowInForm(int? order = null)
    {
        _metadata.ShowInForm = true;
        _metadata.FormOrder = order;
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> IsReadOnly()
    {
        _metadata.IsReadOnly = true;
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> IsHidden()
    {
        _metadata.IsHidden = true;
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> HasFieldType(string fieldType)
    {
        _metadata.FieldType = fieldType;
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> HasFormat(string format)
    {
        _metadata.Format = format;
        return this;
    }

    internal PropertyMetadata Build()
    {
        return _metadata;
    }
}