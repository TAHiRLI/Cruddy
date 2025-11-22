namespace Cruddy.Core.Models;

/// <summary>
/// Metadata for an entity
/// </summary>
public class EntityMetadata
{
    public string Name { get; set; } = string.Empty;
    public Type ClrType { get; set; } = null!;
    public string DisplayName { get; set; } = string.Empty;
    public string PluralName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public SortConfiguration? DefaultSort { get; set; }
    public List<PropertyMetadata> Properties { get; set; } = new();
    public List<string> IgnoredProperties { get; set; } = new();
}

/// <summary>
/// Metadata for a property
/// </summary>
public class PropertyMetadata
{
    public string Name { get; set; } = string.Empty;
    public Type ClrType { get; set; } = null!;
    public string DisplayName { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
    public string HelpText { get; set; } = string.Empty;
    
    // Validation
    public bool IsRequired { get; set; }
    public string? RequiredMessage { get; set; }
    public int? MaxLength { get; set; }
    public int? MinLength { get; set; }
    
    // UI Behavior
    public bool ShowInList { get; set; }
    public int? ListOrder { get; set; }
    public bool ShowInForm { get; set; }
    public int? FormOrder { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsHidden { get; set; }
    
    // Field Types
    public string FieldType { get; set; } = "text";
    public string Format { get; set; } = string.Empty;
}

/// <summary>
/// Sort configuration
/// </summary>
public class SortConfiguration
{
    public string Field { get; set; } = string.Empty;
    public bool Descending { get; set; }
}