# Cruddy - CRUD Boilerplate Generator for .NET & React

## Project Overview

**Cruddy** is a CLI tool and library that generates production-ready React components from .NET entity models, allowing solo developers to skip boilerplate CRUD code and focus on unique business logic. The system uses a fluent configuration API (similar to Entity Framework and FluentValidation) to keep entity classes clean while providing rich UI generation capabilities.

---

## Target Audience

**Primary:** Solo developers building MVPs who need speed without sacrificing code quality or customization ability.

**Value Proposition:**
- ⚡ Minutes from model to working UI
- 🎨 Full customization control - your changes are safe
- 🔄 Update boilerplate without breaking custom work  
- 🎯 Focus on unique business logic, not repetitive CRUD

---

## Core Architecture

### Components

1. **Cruddy.Core** - NuGet library
   - Fluent configuration API
   - Metadata generation
   - Convention-based defaults

2. **Cruddy CLI** - Global .NET tool (`dotnet tool install -g cruddy`)
   - Project scanning
   - Code generation
   - Developer workflow management

---

## Technical Design

### 1. Fluent Configuration API

Developers configure entities using a clean, type-safe fluent API:

```csharp
// Models/User.cs - Clean entity class
public class User 
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Post> Posts { get; set; }
}

// Cruddy/UserCruddyConfig.cs - Separate configuration
public class UserCruddyConfig : CruddyEntityConfig<User>
{
    public UserCruddyConfig()
    {
        ForEntity()
            .HasDisplayName("User")
            .HasPluralName("Users")
            .HasIcon("user")
            .HasDefaultSort(x => x.CreatedAt, descending: true);

        ForProperty(x => x.Name)
            .IsRequired()
            .HasDisplayName("Full Name")
            .HasMaxLength(100)
            .ShowInList()
            .ShowInForm();

        ForProperty(x => x.Email)
            .IsRequired()
            .HasFieldType("email")
            .HasValidation(v => v.Email())
            .ShowInList();

        ForProperty(x => x.CreatedAt)
            .IsReadOnly()
            .HasDisplayName("Member Since")
            .HasFormat("date");

        ForProperty(x => x.Posts)
            .HasRelation(r => r
                .ToMany<Post>()
                .ShowInline()
            );

        Ignore(x => x.PasswordHash);
    }
}
```

### 2. Configuration Discovery

```csharp
// Program.cs / Startup.cs
services.AddCruddy(options =>
{
    options.ScanAssembly(Assembly.GetExecutingAssembly());
    options.DefaultDateFormat = "MM/dd/yyyy";
    options.DefaultPageSize = 25;
});
```

### 3. Convention-Based Fallbacks

If no configuration exists, Cruddy uses intelligent defaults:
- String properties → text inputs, max 255 chars
- Email-named properties → email field type
- DateTime properties → date formatting, readonly for "CreatedAt"
- Relationships → auto-detected from navigation properties

---

## Developer Workflow

### Phase 1: Initial Setup

```bash
# Install CLI
dotnet tool install -g cruddy

# Initialize in project
cd MyProject
cruddy init

# Prompts for:
# - Backend path (./MyApp.Api)
# - Frontend path (./client/src)
# - Creates cruddy.config.json
```

**Generated `cruddy.config.json`:**
```json
{
  "backend": {
    "path": "./MyApp.Api",
  },
  "frontend": {
    "path": "./client/src",
    "outputDir": "./client/src/components",
    "baseUrl": "/api"
  },
  "generate": {
    "extension": ".cruddy.tsx",
  },
  "customized": []
}
```

### Phase 2: Scanning & Generation

```bash
# Scan backend and generate components
cruddy generate

# Output:
# Scanning MyApp.Api...
# ✓ Found 5 entities
#   - User (UserCruddyConfig)
#   - Product (ProductCruddyConfig)  
#   - Order (convention-based)
#   - Invoice (InvoiceCruddyConfig)
#   - Customer (convention-based)
#
# Generating components...
# ✓ UserList.cruddy.tsx
# ✓ UserForm.cruddy.tsx
# ✓ ProductList.cruddy.tsx
# ✓ ProductForm.cruddy.tsx
# ... (10 components total)
#
# Generated metadata: .cruddy/metadata.json
#
# To customize a component:
#   cp components/UserList.cruddy.tsx components/UserList.tsx
```

### Phase 3: Customization

**File Differentiation Strategy:**

```
components/
  ├── UserList.cruddy.tsx      # GENERATED - Don't edit directly
  ├── UserList.tsx             # YOUR CUSTOMIZATION (if exists)
  ├── UserForm.cruddy.tsx      # GENERATED
  ├── ProductList.cruddy.tsx   # GENERATED (not customized)
```

**Option 1: Copy and customize**
```bash
cp components/UserList.cruddy.tsx components/UserList.tsx
# Edit UserList.tsx with your customizations
```

**Option 2: Use helper command**
```bash
cruddy customize UserList

# Output:
# ✓ Created UserList.tsx from UserList.cruddy.tsx
# ✓ Added to customized list
# 
# Import in your app:
#   import { UserList } from '@/components/UserList'
```

**Customization Pattern - Wrapping:**
```tsx
// UserList.tsx (your customized version)
import { UserList as BaseUserList } from './UserList.cruddy';
import { useAuth } from '@/hooks/auth';

export function UserList(props) {
  const { hasPermission } = useAuth();
  
  return (
    <div className="custom-wrapper">
      <h1>My Custom Header</h1>
      <BaseUserList 
        {...props}
        onDelete={hasPermission('delete') ? props.onDelete : undefined}
      />
    </div>
  );
}
```

### Phase 4: Updates & Iteration

```bash
# Regenerate after backend changes
cruddy generate

# Output:
# Scanning MyApp.Api...
# ✓ Found 5 entities (1 modified)
#
# Updating components...
# ✓ UserList.cruddy.tsx (updated - 2 new fields added)
# ⚠ UserList.cruddy.tsx was manually edited - overwriting
# ✓ ProductForm.cruddy.tsx (unchanged)
#
# Customized components detected:
#   UserList.tsx exists - compare with UserList.cruddy.tsx
#
# To see changes:
#   cruddy diff UserList
```

**Diff Command:**
```bash
cruddy diff UserList

# Shows side-by-side diff between:
# - Old UserList.cruddy.tsx (before regenerate)
# - New UserList.cruddy.tsx (after regenerate)
#
# Helps you merge changes into UserList.tsx
```

---

## File Structure

```
MyProject/
├── MyApp.Api/                          # .NET Backend
│   ├── Models/
│   │   ├── User.cs                     # Clean entity
│   │   ├── Product.cs
│   │   └── Order.cs
│   ├── Cruddy/                         # Configuration folder
│   │   ├── UserCruddyConfig.cs
│   │   ├── ProductCruddyConfig.cs
│   │   └── InvoiceCruddyConfig.cs
│   └── Program.cs                      # services.AddCruddy()
│
├── client/                             # React Frontend
│   ├── src/
│   │   ├── components/
│   │   │   ├── UserList.cruddy.tsx     # GENERATED
│   │   │   ├── UserList.tsx            # CUSTOMIZED
│   │   │   ├── UserForm.cruddy.tsx     # GENERATED
│   │   │   ├── ProductList.cruddy.tsx  # GENERATED
│   │   │   └── ProductForm.cruddy.tsx  # GENERATED
│   │   ├── hooks/
│   │   │   └── api.cruddy.ts           # Generated API hooks (future)
│   │   └── types/
│   │       └── api.cruddy.ts           # Generated TypeScript types
│   └── package.json
│
├── .cruddy/                            # Cruddy workspace
│   ├── metadata.json                   # Scanned entity metadata
│   └── customized.json                 # Tracks customized components
│
└── cruddy.config.json                  # Project configuration
```

---

## Generated Component Quality

Components are production-ready with best practices:

```tsx
// UserList.cruddy.tsx (example output)
import { useState } from 'react';
import type { User } from '@/types/api.cruddy';

interface UserListProps {
  users: User[];
  onEdit?: (user: User) => void;
  onDelete?: (id: string) => void;
  loading?: boolean;
}

export function UserList({ 
  users, 
  onEdit, 
  onDelete,
  loading = false 
}: UserListProps) {
  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState<keyof User>('createdAt');
  
  const filtered = users.filter(u => 
    u.name.toLowerCase().includes(search.toLowerCase()) ||
    u.email.toLowerCase().includes(search.toLowerCase())
  );

  const sorted = [...filtered].sort((a, b) => {
    const aVal = a[sortBy];
    const bVal = b[sortBy];
    return aVal > bVal ? -1 : 1;
  });

  if (loading) {
    return <div className="loading">Loading users...</div>;
  }

  return (
    <div className="user-list">
      <div className="list-header">
        <input 
          type="search"
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Search users..."
          className="search-input"
        />
        <select 
          value={sortBy} 
          onChange={e => setSortBy(e.target.value as keyof User)}
        >
          <option value="name">Name</option>
          <option value="email">Email</option>
          <option value="createdAt">Date Created</option>
        </select>
      </div>
      
      <table className="data-table">
        <thead>
          <tr>
            <th>Full Name</th>
            <th>Email</th>
            <th>Member Since</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {sorted.map(user => (
            <tr key={user.id}>
              <td>{user.name}</td>
              <td>{user.email}</td>
              <td>{new Date(user.createdAt).toLocaleDateString()}</td>
              <td className="actions">
                {onEdit && (
                  <button 
                    onClick={() => onEdit(user)}
                    className="btn-edit"
                  >
                    Edit
                  </button>
                )}
                {onDelete && (
                  <button 
                    onClick={() => onDelete(user.id)}
                    className="btn-delete"
                  >
                    Delete
                  </button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      
      {sorted.length === 0 && (
        <div className="empty-state">
          No users found
        </div>
      )}
    </div>
  );
}
```

---

## Metadata Format

Generated `.cruddy/metadata.json`:

```json
{
  "version": "1.0.0",
  "generatedAt": "2025-11-16T10:30:00Z",
  "entities": [
    {
      "name": "User",
      "displayName": "User",
      "pluralName": "Users",
      "icon": "user",
      "defaultSort": {
        "field": "createdAt",
        "descending": true
      },
      "searchFields": ["name", "email"],
      "fields": [
        {
          "name": "id",
          "type": "int",
          "tsType": "number",
          "isPrimaryKey": true,
          "isReadOnly": true
        },
        {
          "name": "name",
          "type": "string",
          "tsType": "string",
          "displayName": "Full Name",
          "isRequired": true,
          "maxLength": 100,
          "showInList": true,
          "showInForm": true,
          "order": 1
        },
        {
          "name": "email",
          "type": "string",
          "tsType": "string",
          "displayName": "Email",
          "fieldType": "email",
          "isRequired": true,
          "validations": ["email"],
          "showInList": true,
          "showInForm": true,
          "order": 2
        },
        {
          "name": "createdAt",
          "type": "DateTime",
          "tsType": "string",
          "displayName": "Member Since",
          "format": "date",
          "isReadOnly": true,
          "showInList": true
        }
      ],
      "relationships": [
        {
          "name": "posts",
          "type": "oneToMany",
          "targetEntity": "Post",
          "foreignKey": "userId",
          "showInline": true
        }
      ],
      "permissions": {
        "create": ["Admin", "Manager"],
        "read": ["Any"],
        "update": ["Admin", "Manager", "Owner"],
        "delete": ["Admin"]
      }
    }
  ]
}
```

---

## CLI Commands Reference

### Core Commands

```bash
# Initial setup
cruddy init
  # Interactive setup wizard
  # Creates cruddy.config.json

# Scan and generate
cruddy generate
  # Scans backend entities
  # Generates/updates .cruddy.tsx files
  # Updates metadata.json

# Customize a component
cruddy customize <ComponentName>
  # Copies Component.cruddy.tsx → Component.tsx
  # Tracks in customized.json

# Compare changes
cruddy diff <ComponentName>
  # Shows diff between old and new .cruddy.tsx versions
```

### Utility Commands

```bash
# Validate configuration
cruddy check
  # Validates cruddy.config.json
  # Checks for conflicts
  # Verifies customized files are compatible

# List entities
cruddy list
  # Shows all discovered entities
  # Indicates which have custom configs
  # Shows which components are customized

# Generate config scaffold
cruddy config <EntityName>
  # Generates EntityCruddyConfig.cs template
  # Includes all properties with defaults
```

### Future Commands (Post-MVP)

```bash
# Watch mode
cruddy watch
  # Auto-regenerates on backend file changes

# Style variants
cruddy generate --style=shadcn
cruddy generate --style=tanstack-table

# Full ejection
cruddy eject <ComponentName>
  # Removes from Cruddy management
  # Fully custom from this point

# Re-adoption
cruddy adopt <ComponentName>
  # Brings custom component back under management
```

---

## Fluent API Reference

### Entity-Level Configuration

```csharp
ForEntity()
    .HasDisplayName(string name)
    .HasPluralName(string name)
    .HasDescription(string description)
    .HasIcon(string icon)
    .HasDefaultSort(Expression<Func<T, object>> field, bool descending = false)
    .HasSearchFields(params Expression<Func<T, object>>[] fields)
    .HasPermissions(Action<PermissionBuilder<T>> configure)
```

### Property-Level Configuration

```csharp
ForProperty(x => x.PropertyName)
    // Display
    .HasDisplayName(string name)
    .HasPlaceholder(string placeholder)
    .HasHelpText(string text)
    .HasIcon(string icon)
    
    // Validation
    .IsRequired(string? message = null)
    .HasMinLength(int length)
    .HasMaxLength(int length)
    .HasValidation(Action<ValidationBuilder> configure)
    
    // UI Behavior
    .ShowInList(int? order = null)
    .ShowInForm(int? order = null)
    .ShowInDetail()
    .ShowInFilter()
    .IsReadOnly()
    .IsHidden()
    
    // Field Types
    .HasFieldType(string type)  // "email", "tel", "url", "textarea", "select"
    .HasFormat(string format)   // "date", "datetime", "currency", "percentage"
    .HasOptions(params object[] options)
    .HasOptionsFrom<TEnum>()
    
    // Relationships
    .HasRelation(Action<RelationBuilder> configure)
    
    // Conditional
    .When(Expression<Func<T, bool>> condition)
```

### Relationships

```csharp
ForProperty(x => x.RelatedEntity)
    .HasRelation(r => r
        .ToOne<TRelated>()
        .WithDisplay(x => x.DisplayField)
        .WithSearchFields(x => x.Field1, x => x.Field2)
        .Required()
    );

ForProperty(x => x.RelatedCollection)
    .HasRelation(r => r
        .ToMany<TRelated>()
        .WithForeignKey(x => x.ForeignKeyProperty)
        .ShowInline()
        .WithMinItems(1)
        .WithMaxItems(10)
        .WithAddButton("Add Item")
    );
```

### Actions

```csharp
HasAction("ActionName")
    .WithIcon(string icon)
    .WithLabel(string label)
    .WithConfirmation(string message)
    .WithPermission(params string[] roles)
    .When(Expression<Func<T, bool>> condition)
    .UpdatesProperty(Expression<Func<T, object>> prop, object value)
```

### Permissions

```csharp
ForEntity()
    .HasPermissions(p => p
        .CanCreate(params string[] roles)
        .CanRead(params string[] roles)
        .CanUpdate(params string[] roles)
        .CanDelete(params string[] roles)
        .CustomRule(Func<User, T, bool> rule)
    );
```

---

## Implementation Roadmap

### Phase 1: MVP
**Goal:** Core workflow functional for solo developers

- [x] Project planning
- [ ] Cruddy.Core library
  - [ ] Fluent API base classes
  - [ ] Configuration discovery
  - [ ] Metadata generation
- [ ] Cruddy CLI
  - [ ] `cruddy init` command
  - [ ] `cruddy generate` command
  - [ ] File watching for manual edits
- [ ] Code generation
  - [ ] React component templates (List, Form)
  - [ ] TypeScript type generation
  - [ ] Basic styling (unstyled/minimal)
- [ ] Documentation
  - [ ] Getting started guide
  - [ ] Fluent API reference
  - [ ] Example projects

### Phase 2: Developer Experience
**Goal:** Make it delightful to use

- [ ] `cruddy customize` command
- [ ] `cruddy diff` command
- [ ] `cruddy check` validation
- [ ] Better error messages
- [ ] Progress indicators
- [ ] Color-coded CLI output

### Phase 3: Advanced Features
**Goal:** Handle complex scenarios

- [ ] Relationship UI (dropdowns, inline editing)
- [ ] Validation feedback in forms
- [ ] Permissions in generated components
- [ ] Custom actions support
- [ ] Conditional rendering
- [ ] API hooks generation (React Query)

### Phase 4: Polish 
**Goal:** Production-ready v1.0

- [ ] `cruddy watch` mode
- [ ] Component quality improvements
- [ ] Performance optimization
- [ ] Comprehensive testing
- [ ] Video tutorials
- [ ] Real-world example apps

### Future (Post v1.0)
- [ ] `--style` variants (shadcn, TanStack Table, etc.)
- [ ] Multi-framework support (Vue, Svelte)
- [ ] Visual configuration editor
- [ ] Plugin system
- [ ] Cloud-hosted template gallery

---

## Success Metrics

### For MVP Launch
- Generate functional CRUD for 5-entity project in < 5 minutes
- Developers can customize without breaking updates
- Zero attribute pollution in entity classes
- Documentation clear enough for first-time user

### For v1.0
- 100 developers using in production
- Average time saved: 4+ hours per project
- 90%+ positive feedback on customization workflow

---

## Technical Constraints

### Requirements
- .NET 6.0+ (for backend scanning)
- Node 16+ (for React generation)
- Git (recommended for tracking changes)

### Limitations
- React only (initially)
- Client-side routing assumed
- No built-in authentication (assumes developer adds)

---

## Key Differentiators

1. **Clean entity classes** - No attribute pollution
2. **Update-safe customization** - .cruddy.tsx vs .tsx pattern
3. **Type-safe configuration** - Expression trees, not strings
4. **Convention over configuration** - Works without config files
5. **.NET ecosystem fit** - Feels like EF Core, FluentValidation

---

## Marketing Messaging

**Tagline:** "Skip the CRUD, build the features that matter"

**Positioning:** 
- Not a UI framework (use your own components)
- Not a backend framework (works with your existing .NET app)
- Not a code generator you throw away (keeps generating)
- **IS** a productivity multiplier that respects your code

**Use Cases:**
- MVP/prototype development
- Internal admin panels
- Startup CRUD applications
- Hackathon projects
- Agency client projects (consistent structure)

---

## Next Steps

1. Set up repository structure
2. Implement Fluent API base classes
3. Build metadata scanner
4. Create first component template (List)
5. Build basic CLI with `init` and `generate`
6. Test on sample .NET project
7. Iterate on generated component quality

---

## Getting Started

### Installation

```bash
# Install the CLI tool
dotnet tool install -g cruddy

# Verify installation
cruddy --version
```

### Quick Start

```bash
# 1. Navigate to your project
cd MyProject

# 2. Initialize Cruddy
cruddy init

# 3. Create entity configurations (optional)
# Add files like Cruddy/UserCruddyConfig.cs

# 4. Generate React components
cruddy generate

# 5. Customize as needed
cruddy customize UserList
```

### Example Configuration

```csharp
public class UserCruddyConfig : CruddyEntityConfig<User>
{
    public UserCruddyConfig()
    {
        ForEntity()
            .HasDisplayName("User")
            .HasPluralName("Users");

        ForProperty(x => x.Name)
            .IsRequired()
            .HasDisplayName("Full Name")
            .ShowInList()
            .ShowInForm();

        ForProperty(x => x.Email)
            .HasFieldType("email")
            .IsRequired();
    }
}
```

---
OLD  IDEAS 


## CLI Commands

### `cruddy init --admin-path <path>`
Creates a new Vite + React + TypeScript admin panel.

**Output:**
- Generates blank app structure
- Configures Redux Toolkit + Persist
- Sets up routing and i18n
- Saves path to `cruddy.config.json`

### `cruddy scan <migration_name>`
Scans your backend and creates a migration.

**Example:**
```bash
cruddy scan "add_departments"
```

**Generated files:**
- `cruddy.migrations/20250129_add_departments.json` - Migration file
- `cruddy_snapshot.json` - Current state snapshot

**What it scans:**
- Entities with `[CruddyEntity]` attribute
- Actions with `[CruddyAction]` attribute
- Fluent configurations in `Cruddy/` folder

### `cruddy apply`
Applies pending migrations to the frontend.

**Generated files** (per entity):
```
app/src/
├── lib/types/Department.types.cruddy.ts
├── APIs/services/Department.service.cruddy.ts
├── store/slices/Department.slice.cruddy.ts
├── components/Department/
│   ├── AddDepartment.cruddy.tsx
│   ├── EditDepartment.cruddy.tsx
│   └── ViewDepartment.cruddy.tsx
└── pages/Department/Department.page.cruddy.tsx
```

**Updated files:**
- `app/src/store/store.ts` - Adds slice
- `app/src/router/routes.ts` - Registers route
- `app/src/router/router.tsx` - Adds component
- `app/src/locales/en.json` - Adds translations
- `app/src/components/layout/sidebar.tsx` - Adds menu item

### `cruddy regenerate`
Rebuilds the entire admin panel from scratch using `cruddy_snapshot.json`.

**Use when:**
- Starting fresh
- Major refactor
- Onboarding new team members

### `cruddy diff [entity]`
Shows changes between current and previous migration.
```bash
cruddy diff Department
```

### `cruddy customize <ComponentName>`
Copies a `.cruddy.tsx` file for customization.
```bash
cruddy customize Department.page
# Creates Department.page.tsx from Department.page.cruddy.tsx
```

## Entity Configuration

### Simple (Attributes)
```csharp
using Cruddy.Core.Attributes;

[CruddyEntity]
public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int EmployeeCount { get; set; }
}
```

### Advanced (Fluent Configuration)
```csharp
using Cruddy.Core;

public class DepartmentCruddyConfig : CruddyEntityConfig<Department>
{
    public DepartmentCruddyConfig()
    {
        ForEntity()
            .HasDisplayName("Department")
            .HasIcon("building");

        ForProperty(x => x.Name)
            .IsRequired()
            .HasMaxLength(100)
            .ShowInList()
            .ShowInForm();

        ForProperty(x => x.EmployeeCount)
            .HasDisplayName("# of Employees")
            .IsReadOnly()
            .ShowInList();
    }
}
```

## Action Configuration
```csharp
using Cruddy.Core.Attributes;
using Cruddy.Core.Enums;

[HttpGet("departments")]
[CruddyAction(Actions.List)]
public IActionResult GetDepartments([FromQuery] int page = 1)
{
    // Return type auto-detected
}

[HttpPost("departments")]
[CruddyAction(Actions.Create)]
public IActionResult CreateDepartment([FromBody] CreateDepartmentDto dto)
{
    // ...
}
```

**Available Actions:**
- `Actions.List` - List/grid view
- `Actions.Details` - Detail/view page
- `Actions.Create` - Create form
- `Actions.Update` - Edit form
- `Actions.Delete` - Delete confirmation

## Customization Workflow

1. **Generated files** (`.cruddy.tsx`) are regenerated on each `cruddy apply`
2. **Your files** (`.tsx`) are never touched
3. **Copy to customize:**
```bash
# Option 1: Use helper
cruddy customize Department.page

# Option 2: Manual copy
cp Department.page.cruddy.tsx Department.page.tsx
```

4. **Import your customized version:**
```tsx
// ❌ Don't import .cruddy files directly
import { DepartmentPage } from './Department.page.cruddy'

// ✅ Import your customized version
import { DepartmentPage } from './Department.page'
```

5. **Update safely:**
```bash
cruddy apply
# ✓ Department.page.cruddy.tsx updated
# ℹ Department.page.tsx exists (your customization preserved)
```

## Migration Files

### Structure
```json
{
  "version": "1.0.0",
  "timestamp": "2025-01-29T10:30:00Z",
  "name": "add_departments",
  "changes": [
    {
      "type": "entity_added",
      "entity": "Department",
      "fields": [...]
    }
  ]
}
```

### Snapshot

`cruddy_snapshot.json` contains the current complete state:
```json
{
  "version": "1.0.0",
  "lastMigration": "20250129_add_departments",
  "entities": [...],
  "actions": [...]
}
```

## Project Structure
```
YourProject/
├── YourApp.Api/              # .NET Backend
│   ├── Models/
│   │   └── Department.cs
│   ├── Cruddy/               # Fluent configs (optional)
│   │   └── DepartmentCruddyConfig.cs
│   ├── Controllers/
│   │   └── DepartmentController.cs
│   └── cruddy.migrations/    # Generated
│       ├── 20250129_add_departments.json
│       └── cruddy_snapshot.json
│
└── admin-panel/              # React Frontend
    ├── src/
    │   ├── components/
    │   │   └── Department/
    │   │       ├── AddDepartment.cruddy.tsx
    │   │       └── AddDepartment.tsx  (your customization)
    │   ├── pages/
    │   ├── store/
    │   ├── APIs/
    │   └── lib/
    └── cruddy.config.json
```

