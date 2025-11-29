using System.Reflection;
using Cruddy.Core.Interfaces;
using Cruddy.Core.Models;
using Cruddy.Core.Providers;
using Cruddy.Core.Scanner;
using Microsoft.Extensions.DependencyInjection;

namespace Cruddy.Core.Extensions;

/// <summary>
/// Options for configuring Cruddy
/// </summary>
public class CruddyOptions
{
    internal List<Assembly> AssembliesToScan { get; } = new();
    public string DefaultDateFormat { get; set; } = "MM/dd/yyyy";
    public int DefaultPageSize { get; set; } = 25;

    public CruddyOptions ScanAssembly(Assembly assembly)
    {
        AssembliesToScan.Add(assembly);
        return this;
    }
}

/// <summary>
/// Service collection extensions for Cruddy
/// </summary>
public static class CruddyOptionExtensions
{
    public static IServiceCollection AddCruddy(
        this IServiceCollection services,
        Action<CruddyOptions> configure)
    {
        var options = new CruddyOptions();
        configure(options);

        var scanner = new ConfigurationScanner();
        var allMetadata = new List<EntityMetadata>();

        foreach (var assembly in options.AssembliesToScan)
        {
            var metadata = scanner.ScanAssembly(assembly);
            allMetadata.AddRange(metadata);
        }

        services.AddSingleton<IEntityMetadataProvider>(
            new EntityMetadataProvider(allMetadata, options));

        return services;
    }
}