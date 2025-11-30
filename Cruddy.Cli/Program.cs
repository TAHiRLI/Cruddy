using Cruddy.Cli.Commands;
using Cruddy.Cli.Core;

namespace Cruddy.Cli;

class Program
{
    static async Task<int> Main(string[] args)  // ✅ Make it async Task<int>
    {
        try
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: cruddy <command>");
                Console.WriteLine("\nAvailable commands:");
                Console.WriteLine("  init       - Initialize Cruddy in the current project");
                Console.WriteLine("  check      - Check project configuration");
                Console.WriteLine("  migrations - Manage migrations");
                return 1;
            }

            var commands = new List<ICommand>
            {
                new CheckCommand(),
                new InitCommand(), 
                new MigrationsCommand(), 
            };

            var commandName = args[0];
            var command = commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

            if (command == null)
            {
                Console.WriteLine($"Unknown command: {commandName}");
                Console.WriteLine("\nAvailable commands:");
                foreach (var cmd in commands)
                {
                    Console.WriteLine($"  {cmd.Name,-12}");
                }
                return 1;
            }

            // ✅ AWAIT the async method!
            return await command.ExecuteAsync([.. args.Skip(1)]);
        }
        catch (Exception ex)
        {
            // This will now catch exceptions from commands
            await Console.Error.WriteLineAsync($"\n❌ Fatal error: {ex.Message}");
            await Console.Error.WriteLineAsync($"\nStack trace:\n{ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                await Console.Error.WriteLineAsync($"\nInner exception: {ex.InnerException.Message}");
            }
            
            await Console.Error.FlushAsync();
            return 1;
        }
    }
}