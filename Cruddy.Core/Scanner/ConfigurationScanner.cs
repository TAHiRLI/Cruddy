using System.Reflection;
using Cruddy.Core.Configuration;
using Cruddy.Core.Models;

namespace Cruddy.Core.Scanner;

/// <summary>
/// Scans assemblies for entity configurations
/// </summary>
public class ConfigurationScanner
{
    /// <summary>
    /// Scan an assembly for CruddyEntityConfig implementations
    /// </summary>
    public List<EntityMetadata> ScanAssembly(Assembly assembly)
    {
        var metadata = new List<EntityMetadata>();

        // Find all types that inherit from CruddyEntityConfig<T>
        var configTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && IsEntityConfigType(t))
            .ToList();

        foreach (var configType in configTypes)
        {
            try
            {
                // Create instance of the configuration class
                var configInstance = Activator.CreateInstance(configType);
                if (configInstance == null) continue;

                // Get the GetMetadata method
                var getMetadataMethod = configType.BaseType?
                    .GetMethod("GetMetadata", BindingFlags.NonPublic | BindingFlags.Instance);

                if (getMetadataMethod != null)
                {
                    var entityMetadata = (EntityMetadata)getMetadataMethod.Invoke(configInstance, null)!;
                    
                    // Apply convention-based defaults for unconfigured properties
                    ApplyConventions(entityMetadata);
                    
                    metadata.Add(entityMetadata);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration {configType.Name}: {ex.Message}");
            }
        }

        return metadata;
    }

    /// <summary>
    /// Check if a type is CruddyEntityConfig<T>
    /// </summary>
    private static bool IsEntityConfigType(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && 
                baseType.GetGenericTypeDefinition() == typeof(CruddyEntityConfig<>))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Apply convention-based defaults for properties not explicitly configured
    /// </summary>
    private void ApplyConventions(EntityMetadata metadata)
    {
        // If no display name set, use entity name
        if (string.IsNullOrEmpty(metadata.DisplayName))
        {
            metadata.DisplayName = metadata.Name;
        }

        // If no plural name set, add 's' to entity name
        if (string.IsNullOrEmpty(metadata.PluralName))
        {
            metadata.PluralName = metadata.Name + "s";
        }

        // Get all public properties from the entity type
        var allProperties = metadata.ClrType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in allProperties)
        {
            // Skip ignored properties
            if (metadata.IgnoredProperties.Contains(property.Name))
                continue;

            // Check if property is already configured
            var existingConfig = metadata.Properties.FirstOrDefault(p => p.Name == property.Name);
            if (existingConfig != null)
            {
                // Property is configured - only apply conventions to unset values
                ApplyConventionsToExistingProperty(existingConfig, property);
                continue;
            }

            // Apply conventions for unconfigured properties
            var propertyMetadata = new PropertyMetadata
            {
                Name = property.Name,
                ClrType = property.PropertyType,
                DisplayName = property.Name
            };

            ApplyConventionsToProperty(propertyMetadata, property);
            metadata.Properties.Add(propertyMetadata);
        }
    }

    /// <summary>
    /// Apply conventions to a property that was explicitly configured
    /// Only fill in values that weren't explicitly set
    /// </summary>
    private void ApplyConventionsToExistingProperty(PropertyMetadata propertyMetadata, PropertyInfo property)
    {
        // Convention: Properties named "Email" should be email field type (if not set)
        if (string.IsNullOrEmpty(propertyMetadata.FieldType) && 
            property.Name.Equals("Email", StringComparison.OrdinalIgnoreCase))
        {
            propertyMetadata.FieldType = "email";
        }

        // Convention: DateTime properties should have date format (if not set)
        if (string.IsNullOrEmpty(propertyMetadata.Format) &&
            (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?)))
        {
            propertyMetadata.Format = "date";
            
            // CreatedAt/UpdatedAt should be readonly (if not explicitly set otherwise)
            if (!propertyMetadata.IsReadOnly &&
                (property.Name.EndsWith("CreatedAt", StringComparison.OrdinalIgnoreCase) ||
                 property.Name.EndsWith("UpdatedAt", StringComparison.OrdinalIgnoreCase)))
            {
                propertyMetadata.IsReadOnly = true;
            }
        }

        // Convention: String properties have max length 255 (if not set)
        if (property.PropertyType == typeof(string) && !propertyMetadata.MaxLength.HasValue)
        {
            propertyMetadata.MaxLength = 255;
        }

        // Set field type to text if not set
        if (string.IsNullOrEmpty(propertyMetadata.FieldType))
        {
            propertyMetadata.FieldType = "text";
        }
    }

    /// <summary>
    /// Apply conventions to a new unconfigured property
    /// </summary>
    private void ApplyConventionsToProperty(PropertyMetadata propertyMetadata, PropertyInfo property)
    {
        // Convention: Properties named "Email" should be email field type
        if (property.Name.Equals("Email", StringComparison.OrdinalIgnoreCase))
        {
            propertyMetadata.FieldType = "email";
        }
        else
        {
            propertyMetadata.FieldType = "text";
        }

        // Convention: DateTime properties should have date format
        if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
        {
            propertyMetadata.Format = "date";
            
            // CreatedAt/UpdatedAt should be readonly
            if (property.Name.EndsWith("CreatedAt", StringComparison.OrdinalIgnoreCase) ||
                property.Name.EndsWith("UpdatedAt", StringComparison.OrdinalIgnoreCase))
            {
                propertyMetadata.IsReadOnly = true;
            }
        }

        // Convention: String properties have max length 255
        if (property.PropertyType == typeof(string))
        {
            propertyMetadata.MaxLength = 255;
        }
    }
}