
using Cruddy.Cli.Commands;
using Cruddy.Cli.Core;

namespace Cruddy.Cli;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: cruddy <command>");
            return;
        }

        var commands = new List<ICommand>
        {
            new CheckCommand(),
            // new ScanEntitiesCommand(), 
            new InitCommand(), 
            // future: new GenerateCommand(), new ScanCommand(), etc.
        };

        var commandName = args[0];
        var command = commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

        if (command == null)
        {
            Console.WriteLine($"Unknown command: {commandName}");
            return;
        }

        command.ExecuteAsync([.. args.Skip(1)]);
    }
}