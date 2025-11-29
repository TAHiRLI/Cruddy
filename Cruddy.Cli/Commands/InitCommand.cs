using System.Text.Json;
using Cruddy.Cli.Core;
using Cruddy.Core.Configuration;

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

        Console.WriteLine("\n✅ Configuration created successfully!");
        Console.WriteLine("\nNext steps:");
        Console.WriteLine("  1. Create entity configurations in your backend (e.g., Cruddy/UserCruddyConfig.cs)");
        Console.WriteLine("  2. Run 'dotnet cruddy migrations add <Name>' to create React components");
        Console.WriteLine("  2. Run 'dotnet cruddy generate' to create React components");


        return 0;
    }

    private async Task CreateDefaultTemplatesAsync(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}