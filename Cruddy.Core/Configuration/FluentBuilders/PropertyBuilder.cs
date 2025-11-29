using Cruddy.Core.Configuration.FluentBuilders.Base;
using Cruddy.Core.Models;

namespace Cruddy.Core.Configuration.FluentBuilders;


/// <summary>
/// Fluent builder for property-level configuration
/// </summary>

public class PropertyBuilder<TEntity, TProperty> : IPropertyBuilder
 where TEntity : class
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

    PropertyMetadata IPropertyBuilder.Build()
    {
        return _metadata;
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
}
