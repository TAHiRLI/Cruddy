
namespace Cruddy.Cli.Core
{
    public interface ICommand
    {
        string Name { get; }
        Task<int> ExecuteAsync(string[] args);
    }
}