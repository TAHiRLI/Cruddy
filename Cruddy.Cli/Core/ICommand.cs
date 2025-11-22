using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cruddy.Cli.Core
{
    public interface ICommand
    {
        string Name { get; }
        Task<int> ExecuteAsync(string[] args);
    }
}