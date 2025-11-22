using Cruddy.Cli.Core;

namespace Cruddy.Cli.Commands
{
    public class CheckCommand : ICommand
    {
        public string Name => "check";

        public Task<int> ExecuteAsync(string[] args)
        {
            Console.WriteLine("ok (async)");
            return Task.FromResult(0);
        }
    }
}