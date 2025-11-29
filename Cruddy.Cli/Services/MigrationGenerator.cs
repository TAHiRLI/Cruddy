using System.Text.Json;
using Cruddy.Cli.Models;
using Cruddy.Cli.Services.Base;
using Cruddy.Core.Models;

namespace Cruddy.Cli.Services
{
    /// <summary>
    /// Generates migration files by comparing current state with snapshot
    /// </summary>
    public class MigrationGenerator
    {
        private readonly IFileSystemService _fileSystem;
        private readonly JsonSerializerOptions _jsonOptions;

        public MigrationGenerator(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Generates a migration ID based on timestamp and name
        /// </summary>
        public string GenerateMigrationId(string migrationName)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"{timestamp}_{migrationName}";
        }

        /// <summary>
        /// Creates a migration file
        /// </summary>
        public async Task<string> CreateMigrationFileAsync(
            string migrationsPath, 
            string migrationName, 
            List<MigrationChange> changes)
        {
            var migrationId = GenerateMigrationId(migrationName);
            var migration = new Migration
            {
                Version = "1.0.0",
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Name = migrationName,
                MigrationId = migrationId,
                Changes = changes
            };

            var fileName = $"{migrationId}.json";
            var filePath = _fileSystem.CombinePaths(migrationsPath, fileName);
            
            var json = JsonSerializer.Serialize(migration, _jsonOptions);
            await _fileSystem.WriteFileAsync(filePath, json);

            return filePath;
        }

        /// <summary>
        /// Detects changes between current entities and snapshot
        /// </summary>
        public List<MigrationChange> DetectChanges(
            List<EntityMetadata> currentEntities,
            List<EntityMetadata> snapshotEntities)
        {
            var changes = new List<MigrationChange>();

            // Create lookup dictionaries for faster comparisons
            var currentEntitiesDict = currentEntities.ToDictionary(e => e.Name);
            var snapshotEntitiesDict = snapshotEntities.ToDictionary(e => e.Name);

            // Check for new entities and modifications
            foreach (var currentEntity in currentEntities)
            {
                if (!snapshotEntitiesDict.ContainsKey(currentEntity.Name))
                {
                    // Entity was added
                    changes.Add(new EntityAddedChange
                    {
                        Type = "EntityAdded",
                        EntityName = currentEntity.Name,
                        Entity = currentEntity
                    });
                }
                else
                {
                    // Entity exists - check for property changes
                    var snapshotEntity = snapshotEntitiesDict[currentEntity.Name];
                    var propertyChanges = DetectPropertyChanges(currentEntity, snapshotEntity);
                    changes.AddRange(propertyChanges);
                }
            }

            // Check for removed entities
            foreach (var snapshotEntity in snapshotEntities)
            {
                if (!currentEntitiesDict.ContainsKey(snapshotEntity.Name))
                {
                    // Entity was removed
                    changes.Add(new EntityRemovedChange
                    {
                        Type = "EntityRemoved",
                        EntityName = snapshotEntity.Name
                    });
                }
            }

            return changes;
        }

        /// <summary>
        /// Detects property-level changes for an entity
        /// </summary>
        private List<MigrationChange> DetectPropertyChanges(
            EntityMetadata currentEntity,
            EntityMetadata snapshotEntity)
        {
            var changes = new List<MigrationChange>();

            var currentPropsDict = currentEntity.Properties.ToDictionary(p => p.Name);
            var snapshotPropsDict = snapshotEntity.Properties.ToDictionary(p => p.Name);

            // Check for new and modified properties
            foreach (var currentProp in currentEntity.Properties)
            {
                if (!snapshotPropsDict.ContainsKey(currentProp.Name))
                {
                    // Property was added
                    changes.Add(new FieldAddedChange
                    {
                        Type = "FieldAdded",
                        EntityName = currentEntity.Name,
                        Field = currentProp
                    });
                }
                else
                {
                    // Property exists - check for modifications
                    var snapshotProp = snapshotPropsDict[currentProp.Name];
                    var propertyModifications = CompareProperties(currentProp, snapshotProp);

                    if (propertyModifications.Count > 0)
                    {
                        changes.Add(new FieldModifiedChange
                        {
                            Type = "FieldModified",
                            EntityName = currentEntity.Name,
                            FieldName = currentProp.Name,
                            Changes = propertyModifications
                        });
                    }
                }
            }

            // Check for removed properties
            foreach (var snapshotProp in snapshotEntity.Properties)
            {
                if (!currentPropsDict.ContainsKey(snapshotProp.Name))
                {
                    // Property was removed
                    changes.Add(new FieldRemovedChange
                    {
                        Type = "FieldRemoved",
                        EntityName = currentEntity.Name,
                        FieldName = snapshotProp.Name
                    });
                }
            }

            return changes;
        }

        /// <summary>
        /// Compares two PropertyMetadata objects and returns differences
        /// </summary>
        private Dictionary<string, FieldPropertyChange> CompareProperties(
            PropertyMetadata current,
            PropertyMetadata snapshot)
        {
            var changes = new Dictionary<string, FieldPropertyChange>();

            // Compare important fields
            CompareField(changes, "DisplayName", current.DisplayName, snapshot.DisplayName);
            CompareField(changes, "HelpText", current.HelpText, snapshot.HelpText);
            CompareField(changes, "Placeholder", current.Placeholder, snapshot.Placeholder);
            CompareField(changes, "FieldType", current.FieldType, snapshot.FieldType);
            CompareField(changes, "Format", current.Format, snapshot.Format);
            CompareField(changes, "IsRequired", current.IsRequired, snapshot.IsRequired);
            CompareField(changes, "IsReadOnly", current.IsReadOnly, snapshot.IsReadOnly);
            CompareField(changes, "IsUnique", current.IsUnique, snapshot.IsUnique);
            CompareField(changes, "MinLength", current.MinLength, snapshot.MinLength);
            CompareField(changes, "MaxLength", current.MaxLength, snapshot.MaxLength);
            CompareField(changes, "MinValue", current.MinValue, snapshot.MinValue);
            CompareField(changes, "MaxValue", current.MaxValue, snapshot.MaxValue);
            CompareField(changes, "ShowInList", current.ShowInList, snapshot.ShowInList);
            CompareField(changes, "ShowInForm", current.ShowInForm, snapshot.ShowInForm);
            CompareField(changes, "ShowInDetail", current.ShowInDetail, snapshot.ShowInDetail);
            CompareField(changes, "RequiredMessage", current.RequiredMessage, snapshot.RequiredMessage);
            CompareField(changes, "ValidationPattern", current.ValidationPattern, snapshot.ValidationPattern);
            CompareField(changes, "ValidationMessage", current.ValidationMessage, snapshot.ValidationMessage);

            return changes;
        }

        /// <summary>
        /// Compares a single field and adds to changes if different
        /// </summary>
        private void CompareField<T>(
            Dictionary<string, FieldPropertyChange> changes,
            string fieldName,
            T? currentValue,
            T? snapshotValue)
        {
            if (!EqualityComparer<T>.Default.Equals(currentValue, snapshotValue))
            {
                changes[fieldName] = new FieldPropertyChange
                {
                    Old = snapshotValue,
                    New = currentValue
                };
            }
        }
    }
}