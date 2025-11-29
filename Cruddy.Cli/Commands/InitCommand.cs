using System.Text.Json;
using Cruddy.Cli.Core;
using Cruddy.Cli.Helpers;
using Cruddy.Cli.Models;
using Cruddy.Cli.Services;
using Cruddy.Cli.Services.Base;
using Cruddy.Core.Configuration;
using Cruddy.Core.Scanner;

namespace Cruddy.Cli.Commands;

public class InitCommand : ICommand
{
    public string Name => "init";
    public string Description => "Initialize Cruddy in the current project";

    public async Task<int> ExecuteAsync(string[] args)
    {
        Console.WriteLine("🚀 Initializing Cruddy in your project...\n");

        // Check if config already exists
        if (File.Exists("cruddy.config.json"))
        {
            Console.WriteLine("⚠️  cruddy.config.json already exists!");
            Console.Write("Do you want to overwrite it? (y/N): ");
            var response = Console.ReadLine()?.ToLower();
            if (response != "y" && response != "yes")
            {
                Console.WriteLine("Cancelled.");
                return 0;
            }
            Console.WriteLine();
        }

        // Prompt for backend path
        Console.Write("Backend path (default: ./MyApp.Api): ");
        var backendPath = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(backendPath))
            backendPath = "./MyApp.Api";

        // Prompt for frontend path
        Console.Write("Frontend path (default: ./client/src): ");
        var frontendPath = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(frontendPath))
            frontendPath = "./client/src";

        // Prompt for output directory
        Console.Write("Component output directory (default: ./client/src/components): ");
        var outputDir = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(outputDir))
            outputDir = "./client/src/components";

        // Prompt for base API URL
        Console.Write("Base API URL (default: /api): ");
        var baseUrl = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(baseUrl))
            baseUrl = "/api";

        // Create configuration
        var config = new CruddyConfig
        {
            Backend = new BackendConfig
            {
                Path = backendPath,
            },
            Frontend = new FrontendConfig
            {
                Path = frontendPath,
                OutputDir = outputDir,
                BaseUrl = baseUrl
            },
            Generate = new GenerateConfig
            {
                Extension = ".cruddy.tsx",
                TemplatePath = "./.cruddy/templates/react-ts/"
            },

            Customized = new List<string>()
        };
        await CreateDefaultTemplatesAsync(config.Generate.TemplatePath);

        // Serialize to JSON
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(config, options);

        // Write to file
        await File.WriteAllTextAsync("cruddy.config.json", json);

        ConsoleHelper.WriteSuccess("Configuration created successfully!");

        // Create .cruddy directory structure
        await CreateCruddyDirectoryStructureAsync();

        // Create initial snapshot
        await CreateInitialSnapshotAsync(config);

        Console.WriteLine("\nNext steps:");
        Console.WriteLine("  1. Create entity configurations in your backend (e.g., Cruddy/UserCruddyConfig.cs)");
        Console.WriteLine("  2. Run 'cruddy migrations add <Name>' to track your entity changes");
        Console.WriteLine("  3. Run 'cruddy generate' to create React components");

        return 0;
    }

    private async Task CreateCruddyDirectoryStructureAsync()
    {
        var cruddyDir = ".cruddy";
        var migrationsDir = Path.Combine(cruddyDir, "migrations");

        if (!Directory.Exists(cruddyDir))
        {
            Directory.CreateDirectory(cruddyDir);
            ConsoleHelper.WriteInfo($"Created {cruddyDir}/ directory");
        }

        if (!Directory.Exists(migrationsDir))
        {
            Directory.CreateDirectory(migrationsDir);
            ConsoleHelper.WriteInfo($"Created {migrationsDir}/ directory");
        }

        await Task.CompletedTask;
    }

    private async Task CreateInitialSnapshotAsync(CruddyConfig config)
    {
        try
        {
            var fileSystem = new FileSystemService();
            var assemblyLoader = new AssemblyLoaderService(fileSystem);

            ConsoleHelper.WriteInfo("Scanning backend for entity configurations...");

            // Load backend assembly
            var assembly = await assemblyLoader.LoadBackendAssemblyAsync(config.Backend.Path, build: true);

            // Scan for entity configurations
            var scanner = new ConfigurationScanner();
            var entities = scanner.ScanAssembly(assembly);

            // Create initial snapshot
            var snapshot = new Snapshot
            {
                Version = "1.0.0",
                LastMigration = null,
                AppliedMigrations = new List<string>(),
                Entities = entities
            };

            var snapshotPath = Path.Combine(".cruddy", "snapshot.json");
            var snapshotJson = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(snapshotPath, snapshotJson);

            ConsoleHelper.WriteSuccess($"Initial snapshot created with {entities.Count} entity/entities");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteWarning($"Could not create initial snapshot: {ex.Message}");
            ConsoleHelper.WriteInfo("You can create a snapshot later by running 'cruddy migrations add <Name>'");
        }
    }

    private async Task CreateDefaultTemplatesAsync(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        await Task.CompletedTask;
    }
}