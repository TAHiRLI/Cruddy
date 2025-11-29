using Cruddy.Cli.Core;
using Cruddy.Cli.Services;
using Cruddy.Cli.Helpers;
using ICommand = Cruddy.Cli.Core.ICommand;

namespace Cruddy.Cli.Commands
{
    /// <summary>
    /// Command for managing Cruddy migrations
    /// </summary>
    public class MigrationsCommand : ICommand
    {
        private readonly IMigrationService _migrationService;

        public string Name => "migrations";
        public string Description => "Manage Cruddy migrations";

        public MigrationsCommand()
        {
            var fileSystem = new FileSystemService();
            _migrationService = new MigrationService(fileSystem);
        }

        // Constructor for dependency injection (testing)
        public MigrationsCommand(IMigrationService migrationService)
        {
            _migrationService = migrationService;
        }

        public async Task<int> ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return 1;
            }

            var subcommand = args[0].ToLower();

            try
            {
                return subcommand switch
                {
                    "add" => await AddMigration(args.Skip(1).ToArray()),
                    "remove" => await RemoveMigration(args.Skip(1).ToArray()),
                    "list" => await ListMigrations(args.Skip(1).ToArray()),
                    _ => ShowHelp()
                };
            }
            catch (InvalidOperationException ex)
            {
                ConsoleHelper.WriteError(ex.Message);
                return 1;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"An error occurred: {ex.Message}");
                return 1;
            }
        }

        private async Task<int> AddMigration(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleHelper.WriteError("Migration name is required");
                Console.WriteLine("Usage: cruddy migrations add <MigrationName>");
                return 1;
            }

            var migrationName = args[0];

            // Validate migration name
            if (!IsValidMigrationName(migrationName))
            {
                ConsoleHelper.WriteError("Invalid migration name. Use alphanumeric characters and underscores only.");
                return 1;
            }

            ConsoleHelper.WriteInfo($"Creating migration: {migrationName}");

            var filePath = await _migrationService.CreateMigrationAsync(migrationName);

            ConsoleHelper.WriteSuccess($"Migration created: {Path.GetFileName(filePath)}");
            ConsoleHelper.WriteInfo($"Location: {filePath}");

            return 0;
        }

        private async Task<int> RemoveMigration(string[] args)
        {
            ConsoleHelper.WriteInfo("Removing last migration...");

            var removed = await _migrationService.RemoveLastMigrationAsync();

            if (removed)
            {
                ConsoleHelper.WriteSuccess("Last migration removed successfully");
            }
            else
            {
                ConsoleHelper.WriteWarning("No migrations to remove");
            }

            return 0;
        }

        private async Task<int> ListMigrations(string[] args)
        {
            var migrations = await _migrationService.ListMigrationsAsync();

            if (migrations.Count == 0)
            {
                ConsoleHelper.WriteInfo("No migrations found");
                return 0;
            }

            ConsoleHelper.WriteHeader("Migrations");

            foreach (var migration in migrations)
            {
                Console.WriteLine($"  {migration.MigrationId}");
                Console.WriteLine($"    Name: {migration.Name}");
                Console.WriteLine($"    Timestamp: {migration.Timestamp}");
                Console.WriteLine($"    Changes: {migration.Changes.Count}");
                Console.WriteLine();
            }

            ConsoleHelper.WriteInfo($"Total migrations: {migrations.Count}");

            return 0;
        }

        private int ShowHelp()
        {
            ConsoleHelper.WriteHeader("Migrations Command");

            Console.WriteLine("Usage: cruddy migrations <subcommand> [options]");
            Console.WriteLine();
            Console.WriteLine("Subcommands:");
            ConsoleHelper.WriteList("add <MigrationName>    - Create a new migration");
            ConsoleHelper.WriteList("remove                 - Remove the last migration");
            ConsoleHelper.WriteList("list                   - List all migrations");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  cruddy migrations add InitialCreate");
            Console.WriteLine("  cruddy migrations add AddUserEmail");
            Console.WriteLine("  cruddy migrations remove");
            Console.WriteLine("  cruddy migrations list");

            return 0;
        }

        private bool IsValidMigrationName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && 
                   name.All(c => char.IsLetterOrDigit(c) || c == '_');
        }
    }
}