# Cruddy Fluent Configuration Guide

## Overview

Cruddy provides a powerful fluent API for configuring entity metadata, including properties and relationships. This configuration system allows you to define how your entities should behave in CRUD operations, forms, and lists without modifying your entity classes.

## Table of Contents

- [Getting Started](#getting-started)
- [Entity Configuration](#entity-configuration)
- [Property Configuration](#property-configuration)
- [Relationship Configuration](#relationship-configuration)
  - [One-to-Many](#one-to-many-hasmany)
  - [Many-to-One](#many-to-one-hasone)
  - [One-to-One](#one-to-one-hasonetoone)
  - [Many-to-Many](#many-to-many-hasmanytomany)
- [Complete Examples](#complete-examples)

---

## Getting Started

To configure an entity, create a configuration class that inherits from `CruddyEntityConfig<TEntity>`:

```csharp
using Cruddy.Core.Configuration;
using YourApp.Entities;

namespace YourApp.Cruddy;

public class EmployeeCruddyConfig : CruddyEntityConfig<Employee>
{
    public EmployeeCruddyConfig()
    {
        // Entity-level configuration
        ForEntity()
            .HasDisplayName("Employee")
            .HasPluralName("Employees");

        // Property-level configuration
        ForProperty(x => x.Name)
            .HasDisplayName("Full Name")
            .IsRequired();
    }
}
```

---

## Entity Configuration

Configure entity-level metadata using the `ForEntity()` method:

### Available Methods

| Method | Description | Example |
|--------|-------------|---------|
| `HasDisplayName(string)` | Set the display name for the entity | `.HasDisplayName("Employee")` |
| `HasPluralName(string)` | Set the plural name | `.HasPluralName("Employees")` |
| `HasDescription(string)` | Set the description | `.HasDescription("Employee records")` |
| `HasIcon(string)` | Set the icon | `.HasIcon("person")` |
| `HasDefaultSort<TProperty>(field, descending)` | Set default sorting | `.HasDefaultSort(x => x.Name)` |

### Example

```csharp
ForEntity()
    .HasDisplayName("Employee")
    .HasPluralName("Employees")
    .HasDescription("Manage employee information")
    .HasIcon("people")
    .HasDefaultSort(x => x.LastName, descending: false);
```

---

## Property Configuration

Configure property-level metadata using the `ForProperty()` method:

### Available Methods

| Method | Description | Example |
|--------|-------------|---------|
| `HasDisplayName(string)` | Set display name | `.HasDisplayName("Full Name")` |
| `HasPlaceholder(string)` | Set placeholder text | `.HasPlaceholder("Enter name")` |
| `HasHelpText(string)` | Set help text | `.HasHelpText("First and last name")` |
| `IsRequired(string?)` | Mark as required | `.IsRequired("Name is required")` |
| `HasMaxLength(int)` | Set maximum length | `.HasMaxLength(100)` |
| `HasMinLength(int)` | Set minimum length | `.HasMinLength(2)` |
| `ShowInList(int?)` | Show in list views | `.ShowInList(order: 1)` |
| `ShowInForm(int?)` | Show in form views | `.ShowInForm(order: 1)` |
| `IsReadOnly()` | Mark as read-only | `.IsReadOnly()` |
| `IsHidden()` | Hide from UI | `.IsHidden()` |
| `HasFieldType(string)` | Set field type | `.HasFieldType("email")` |
| `HasFormat(string)` | Set display format | `.HasFormat("yyyy-MM-dd")` |

### Example

```csharp
ForProperty(x => x.Email)
    .HasDisplayName("Email Address")
    .HasFieldType("email")
    .HasPlaceholder("user@example.com")
    .HasHelpText("Work email address")
    .IsRequired("Email is required")
    .HasMaxLength(255)
    .ShowInList(order: 2)
    .ShowInForm(order: 3);

ForProperty(x => x.DateOfBirth)
    .HasDisplayName("Birth Date")
    .HasFormat("yyyy-MM-dd")
    .ShowInForm();

ForProperty(x => x.InternalId)
    .IsHidden(); // Won't show in UI
```

---

## Relationship Configuration

Cruddy supports four types of relationships with a fluent API similar to Entity Framework Core.

### One-to-Many (HasMany)

Configure when the current entity has multiple related entities.

**Example:** A Department has many Employees

```csharp
public class DepartmentCruddyConfig : CruddyEntityConfig<Department>
{
    public DepartmentCruddyConfig()
    {
        ForEntity()
            .HasDisplayName("Department")
            .HasMany(x => x.Employees)
                .WithForeignKey("DepartmentId") // FK is on Employee
                .WithInverse("Department")
                .HasDisplayName("Department Employees")
                .ShowInForm()
            .And();
    }
}
```

#### Available Methods

| Method | Description |
|--------|-------------|
| `WithForeignKey(expression)` | Specify FK property (expression) |
| `WithForeignKey(string)` | Specify FK property (name) |
| `WithInverse(expression)` | Specify inverse navigation property |
| `WithInverse(string)` | Specify inverse property (name) |
| `IsRequired()` | Mark relationship as required |
| `HasDisplayName(string)` | Set display name |
| `HasDescription(string)` | Set description |
| `ShowInList()` | Show in list views |
| `ShowInForm()` | Show in form views |
| `IsHidden()` | Hide from UI |
| `And()` | Return to entity builder for chaining |

---

### Many-to-One (HasOne)

Configure when the current entity belongs to one related entity.

**Example:** An Employee belongs to one Department

```csharp
public class EmployeeCruddyConfig : CruddyEntityConfig<Employee>
{
    public EmployeeCruddyConfig()
    {
        ForEntity()
            .HasDisplayName("Employee")
            .HasOne(x => x.Department)
                .WithForeignKey(x => x.DepartmentId) // FK is on Employee
                .WithInverse("Employees")
                .IsRequired()
                .HasDisplayName("Department")
                .ShowInList()
                .ShowInForm()
            .And();
    }
}
```

#### Available Methods

Same as `HasMany` (see table above).

---

### One-to-One (HasOneToOne)

Configure when the current entity has exactly one related entity.

**Example:** An Employee has one Address

```csharp
public class EmployeeCruddyConfig : CruddyEntityConfig<Employee>
{
    public EmployeeCruddyConfig()
    {
        ForEntity()
            .HasDisplayName("Employee")
            .HasOneToOne(x => x.Address)
                .WithForeignKey(x => x.AddressId)
                .WithInverse("Employee")
                .HasDisplayName("Home Address")
                .ShowInForm()
            .And();
    }
}
```

#### Available Methods

Same as `HasMany` (see table above).

---

### Many-to-Many (HasManyToMany)

Configure when entities have a many-to-many relationship through a join table.

**Example:** Students have many Courses, and Courses have many Students

```csharp
public class StudentCruddyConfig : CruddyEntityConfig<Student>
{
    public StudentCruddyConfig()
    {
        ForEntity()
            .HasDisplayName("Student")
            .HasManyToMany(x => x.Courses)
                .UsingJoinTable("StudentCourses")
                .WithInverse(c => c.Students)
                .HasDisplayName("Enrolled Courses")
                .ShowInForm()
            .And();
    }
}
```

#### Available Methods

| Method | Description |
|--------|-------------|
| `UsingJoinTable(string)` | Specify join/junction table name |
| `WithInverse(expression)` | Specify inverse collection property |
| `HasDisplayName(string)` | Set display name |
| `HasDescription(string)` | Set description |
| `ShowInList()` | Show in list views |
| `ShowInForm()` | Show in form views |
| `And()` | Return to entity builder for chaining |

---

## Complete Examples

### Example 1: Employee Configuration

```csharp
public class EmployeeCruddyConfig : CruddyEntityConfig<Employee>
{
    public EmployeeCruddyConfig()
    {
        // Entity configuration
        ForEntity()
            .HasDisplayName("Employee")
            .HasPluralName("Employees")
            .HasDescription("Employee management")
            .HasIcon("person")
            .HasDefaultSort(x => x.LastName)
            
            // Many-to-one: Employee → Department
            .HasOne(x => x.Department)
                .WithForeignKey(x => x.DepartmentId)
                .WithInverse("Employees")
                .IsRequired()
                .HasDisplayName("Department")
                .ShowInList()
                .ShowInForm()
            .And()
            
            // One-to-many: Employee → TimeEntries
            .HasMany(x => x.TimeEntries)
                .WithForeignKey("EmployeeId")
                .WithInverse("Employee")
                .HasDisplayName("Time Entries")
                .ShowInForm()
            .And()
            
            // One-to-one: Employee → Address
            .HasOneToOne(x => x.Address)
                .WithForeignKey(x => x.AddressId)
                .HasDisplayName("Home Address")
                .ShowInForm()
            .And()
            
            // Many-to-many: Employee ↔ Projects
            .HasManyToMany(x => x.Projects)
                .UsingJoinTable("EmployeeProjects")
                .WithInverse(p => p.Employees)
                .HasDisplayName("Assigned Projects")
                .ShowInForm();

        // Property configurations
        ForProperty(x => x.FirstName)
            .HasDisplayName("First Name")
            .HasPlaceholder("Enter first name")
            .IsRequired("First name is required")
            .HasMaxLength(50)
            .ShowInList(order: 1)
            .ShowInForm(order: 1);

        ForProperty(x => x.LastName)
            .HasDisplayName("Last Name")
            .HasPlaceholder("Enter last name")
            .IsRequired("Last name is required")
            .HasMaxLength(50)
            .ShowInList(order: 2)
            .ShowInForm(order: 2);

        ForProperty(x => x.Email)
            .HasDisplayName("Email Address")
            .HasFieldType("email")
            .HasPlaceholder("user@company.com")
            .IsRequired("Email is required")
            .HasMaxLength(255)
            .ShowInList(order: 3)
            .ShowInForm(order: 3);

        ForProperty(x => x.HireDate)
            .HasDisplayName("Hire Date")
            .HasFormat("yyyy-MM-dd")
            .IsRequired()
            .ShowInList(order: 4)
            .ShowInForm(order: 4);

        ForProperty(x => x.Salary)
            .HasDisplayName("Annual Salary")
            .HasFormat("C2")
            .HasHelpText("Annual salary in USD")
            .ShowInForm(order: 5);

        ForProperty(x => x.IsActive)
            .HasDisplayName("Active")
            .HasFieldType("checkbox")
            .ShowInList(order: 5);
    }
}
```

### Example 2: Department with Self-Referencing Relationship

```csharp
public class DepartmentCruddyConfig : CruddyEntityConfig<Department>
{
    public DepartmentCruddyConfig()
    {
        ForEntity()
            .HasDisplayName("Department")
            .HasPluralName("Departments")
            
            // One-to-many: Department → Employees
            .HasMany(x => x.Employees)
                .WithInverse("Department")
                .HasDisplayName("Department Members")
                .ShowInForm()
            .And()
            
            // Self-referential: Department → Parent Department
            .HasOne(x => x.ParentDepartment)
                .WithForeignKey(x => x.ParentDepartmentId)
                .WithInverse("SubDepartments")
                .HasDisplayName("Parent Department")
            .And()
            
            // Self-referential: Department → Sub-Departments
            .HasMany(x => x.SubDepartments)
                .WithInverse("ParentDepartment")
                .HasDisplayName("Sub-Departments");

        ForProperty(x => x.Name)
            .HasDisplayName("Department Name")
            .IsRequired()
            .ShowInList()
            .ShowInForm();

        ForProperty(x => x.Code)
            .HasDisplayName("Department Code")
            .HasMaxLength(10)
            .ShowInList()
            .ShowInForm();
    }
}
```

### Example 3: Product with Multiple Relationships

```csharp
public class ProductCruddyConfig : CruddyEntityConfig<Product>
{
    public ProductCruddyConfig()
    {
        ForEntity()
            .HasDisplayName("Product")
            .HasPluralName("Products")
            .HasDefaultSort(x => x.Name)
            
            // Many-to-one: Product → Category
            .HasOne(x => x.Category)
                .WithForeignKey(x => x.CategoryId)
                .IsRequired()
                .HasDisplayName("Category")
                .ShowInList()
                .ShowInForm()
            .And()
            
            // Many-to-one: Product → Supplier
            .HasOne(x => x.Supplier)
                .WithForeignKey(x => x.SupplierId)
                .HasDisplayName("Supplier")
                .ShowInForm()
            .And()
            
            // One-to-many: Product → Reviews
            .HasMany(x => x.Reviews)
                .HasDisplayName("Customer Reviews")
                .ShowInForm()
            .And()
            
            // Many-to-many: Product ↔ Tags
            .HasManyToMany(x => x.Tags)
                .UsingJoinTable("ProductTags")
                .HasDisplayName("Tags")
                .ShowInForm();

        ForProperty(x => x.Name)
            .HasDisplayName("Product Name")
            .IsRequired()
            .HasMaxLength(200)
            .ShowInList(order: 1)
            .ShowInForm(order: 1);

        ForProperty(x => x.SKU)
            .HasDisplayName("SKU")
            .HasPlaceholder("PROD-XXX")
            .IsRequired()
            .HasMaxLength(50)
            .ShowInList(order: 2)
            .ShowInForm(order: 2);

        ForProperty(x => x.Price)
            .HasDisplayName("Unit Price")
            .HasFormat("C2")
            .IsRequired()
            .ShowInList(order: 3)
            .ShowInForm(order: 3);

        ForProperty(x => x.Stock)
            .HasDisplayName("In Stock")
            .HasHelpText("Current inventory count")
            .ShowInList(order: 4)
            .ShowInForm(order: 4);

        ForProperty(x => x.Description)
            .HasDisplayName("Description")
            .HasFieldType("textarea")
            .HasMaxLength(1000)
            .ShowInForm(order: 5);

        ForProperty(x => x.IsActive)
            .HasDisplayName("Active")
            .HasFieldType("checkbox")
            .ShowInList(order: 5)
            .ShowInForm(order: 6);
    }
}
```

---

## Best Practices

1. **Use Descriptive Display Names**: Make your UI more user-friendly with clear names
   ```csharp
   .HasDisplayName("Employee Name")  // ✅ Good
   .HasDisplayName("Name")          // ❌ Too generic
   ```

2. **Chain Related Configurations**: Use `.And()` to keep configurations together
   ```csharp
   ForEntity()
       .HasOne(x => x.Department)
           .IsRequired()
       .And()
       .HasMany(x => x.TimeEntries);
   ```

3. **Provide Helpful Messages**: Custom validation messages improve UX
   ```csharp
   .IsRequired("Please enter the employee's email address")
   ```

4. **Control Visibility**: Show only relevant fields in lists and forms
   ```csharp
   ForProperty(x => x.Id).IsHidden();
   ForProperty(x => x.Name).ShowInList().ShowInForm();
   ```

5. **Set Appropriate Field Types**: Match field types to data types
   ```csharp
   ForProperty(x => x.Email).HasFieldType("email");
   ForProperty(x => x.Description).HasFieldType("textarea");
   ForProperty(x => x.IsActive).HasFieldType("checkbox");
   ```

---

## Summary

The Cruddy fluent configuration API provides:

- ✅ **Type-safe** configuration using strongly-typed expressions
- ✅ **Intuitive** API similar to Entity Framework Core
- ✅ **Comprehensive** support for all relationship types
- ✅ **Flexible** property and entity customization
- ✅ **Chainable** methods for clean, readable code
- ✅ **UI control** over what displays in lists and forms

This system allows you to define complete entity behavior without cluttering your domain models with attributes!
