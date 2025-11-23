# Migration-Based Approach

## File Structure


```
MyProject/
├── cruddy.config.json
├── .cruddy/
│   ├── migrations/
│   │   ├── 20251123103045_InitialCreate.json
│   │   ├── 20251123145230_AddUserEmail.json
│   │   └── 20251124091520_AddProductEntity.json
│   └── snapshot.json                    # Current complete state

```

## Full Command Hierarchy

```bash
cruddy init                              # Initialize project generate config

cruddy migrations add <Name>             # Create migration
cruddy migrations remove                 # Remove last migration
cruddy migrations list                   # List all migrations

cruddy generate                          # Apply migrations & generate components

cruddy customize <Component>             # Copy .cruddy.tsx to .tsx
```


## Migration States
The snapshot can track which migrations have been applied:

```json
{
  "version": "1.0.0",
  "lastMigration": "20251123145230_AddUserEmail",
  "appliedMigrations": [
    "20251123103045_InitialCreate",
    "20251123145230_AddUserEmail"
  ],
  "entities": [
    {
    "name": "Employee",
    "clrType": "CruddyTest.Core.Entities.Employee",
    "properties": [
      {
        "name": "Name",
        "type": "String",
        "displayName": "The name of the employee 1",
        "placeholder": "Enter employee name placeholder",
        "helpText": "",
        "isRequired": true,
        "requiredMessage": "This is a custom required filed",
        "maxLength": 200,
        "minLength": 12
      },
      {
        "name": "Email",
        "type": "String",
        "displayName": "Email",
        "placeholder": "",
        "helpText": "",
        "isRequired": true,
        "requiredMessage": null,
        "maxLength": 255,
        "minLength": null
      }

    ]
  }
  ]
}

```

Subsequent Migration Example

```json
// .cruddy/migrations/20251123145230_AddUserEmail.json
{
  "version": "1.0.0",
  "timestamp": "2025-11-23T14:52:30Z",
  "name": "AddUserEmail",
  "migrationId": "20251123145230_AddUserEmail",
  "changes": [
    {
      "type": "field_added",
      "entityName": "User",
      "field": {
        "name": "email",
        "clrType": "string",
        "tsType": "string",
        "displayName": "Email",
        "fieldType": "email",
        "isRequired": true,
        "showInList": true,
        "showInForm": true,
        "order": 2
      }
    },
    {
      "type": "field_modified",
      "entityName": "User",
      "fieldName": "name",
      "changes": {
        "maxLength": {
          "old": 100,
          "new": 200
        }
      }
    }
  ]
}
```

## Change Types

```typescript
type ChangeType = 
  | "entity_added"
  | "entity_removed"
  | "entity_modified"
  | "field_added"
  | "field_removed"
  | "field_modified"
  | "relationship_added"
  | "relationship_removed"
  | "relationship_modified";
```



