using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cruddy.Cli.Services.Base;

namespace Cruddy.Cli.Services
{
    /// <summary>
    /// Default implementation of file system operations
    /// </summary>
    public class FileSystemService : IFileSystemService
    {
        public bool DirectoryExists(string path) => Directory.Exists(path);

        public bool FileExists(string path) => File.Exists(path);

        public void CreateDirectory(string path) => Directory.CreateDirectory(path);

        public async Task<string> ReadFileAsync(string path) =>
            await File.ReadAllTextAsync(path);

        public async Task WriteFileAsync(string path, string content) =>
            await File.WriteAllTextAsync(path, content);

        public void DeleteFile(string path) => File.Delete(path);

        public string[] GetFiles(string path, string searchPattern) =>
            Directory.GetFiles(path, searchPattern);

        public string GetCurrentDirectory() => Directory.GetCurrentDirectory();

        public string CombinePaths(params string[] paths) => Path.Combine(paths);
    }
}