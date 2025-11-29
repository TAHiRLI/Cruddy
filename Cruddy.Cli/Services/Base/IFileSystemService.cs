
namespace Cruddy.Cli.Services.Base
{
    /// <summary>
    /// Abstraction for file system operations to enable testing and modularity
    /// </summary>
    public interface IFileSystemService
    {
        bool DirectoryExists(string path);
        bool FileExists(string path);
        void CreateDirectory(string path);
        Task<string> ReadFileAsync(string path);
        Task WriteFileAsync(string path, string content);
        void DeleteFile(string path);
        string[] GetFiles(string path, string searchPattern);
        string GetCurrentDirectory();
        string CombinePaths(params string[] paths);
    }
}