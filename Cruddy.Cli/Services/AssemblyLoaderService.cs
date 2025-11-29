using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Cruddy.Cli.Services.Base;

namespace Cruddy.Cli.Services
{
    /// <summary>
    /// Service for loading backend assemblies
    /// </summary>
    public class AssemblyLoaderService
    {
        private readonly IFileSystemService _fileSystem;

        public AssemblyLoaderService(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// Loads the backend assembly from the specified path
        /// </summary>
        /// <param name="backendPath">Path to the backend project directory</param>
        /// <param name="build">Whether to build the project first (default: true)</param>
        /// <returns>Loaded assembly</returns>
        public async Task<Assembly> LoadBackendAssemblyAsync(string backendPath, bool build = true)
        {
            // Find .csproj file
            var projectFile = FindProjectFile(backendPath);
            if (projectFile == null)
            {
                throw new InvalidOperationException(
                    $"No .csproj file found in '{backendPath}'. Please check your backend path in cruddy.config.json");
            }

            // Get target framework and assembly name
            var (targetFramework, assemblyName) = GetProjectInfo(projectFile);

            // Build if requested
            if (build)
            {
                await BuildProjectAsync(projectFile);
            }

            // Find the compiled .dll
            var dllPath = FindCompiledAssembly(backendPath, assemblyName, targetFramework);
            if (dllPath == null)
            {
                throw new InvalidOperationException(
                    $"Could not find compiled assembly for '{assemblyName}'. " +
                    $"Expected path: {backendPath}/bin/Debug/{targetFramework}/{assemblyName}.dll\n" +
                    "Try building your project first with 'dotnet build'");
            }

            // Load the assembly
            try
            {
                var assembly = Assembly.LoadFrom(dllPath);
                return assembly;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to load assembly from '{dllPath}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Finds the .csproj file in the given directory
        /// </summary>
        private string? FindProjectFile(string path)
        {
            if (!_fileSystem.DirectoryExists(path))
            {
                throw new DirectoryNotFoundException($"Directory not found: {path}");
            }

            var csprojFiles = _fileSystem.GetFiles(path, "*.csproj");
            return csprojFiles.FirstOrDefault();
        }

        /// <summary>
        /// Extracts target framework and assembly name from .csproj file
        /// </summary>
        private (string targetFramework, string assemblyName) GetProjectInfo(string projectFile)
        {
            var projectContent = File.ReadAllText(projectFile);
            var doc = XDocument.Parse(projectContent);

            // Get TargetFramework
            var targetFramework = doc.Descendants("TargetFramework").FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(targetFramework))
            {
                throw new InvalidOperationException(
                    $"Could not find TargetFramework in {projectFile}");
            }

            // Get AssemblyName (or use project file name)
            var assemblyName = doc.Descendants("AssemblyName").FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(assemblyName))
            {
                assemblyName = Path.GetFileNameWithoutExtension(projectFile);
            }

            return (targetFramework, assemblyName);
        }

        /// <summary>
        /// Builds the project using dotnet build
        /// </summary>
        private async Task BuildProjectAsync(string projectFile)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{projectFile}\" --verbosity quiet",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start dotnet build process");
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Build failed with exit code {process.ExitCode}\n" +
                    $"Output: {output}\n" +
                    $"Error: {error}");
            }
        }

        /// <summary>
        /// Finds the compiled assembly .dll file
        /// </summary>
        private string? FindCompiledAssembly(string backendPath, string assemblyName, string targetFramework)
        {
            // Try Debug first, then Release
            var configurations = new[] { "Debug", "Release" };

            foreach (var config in configurations)
            {
                var dllPath = _fileSystem.CombinePaths(
                    backendPath,
                    "bin",
                    config,
                    targetFramework,
                    $"{assemblyName}.dll");

                if (_fileSystem.FileExists(dllPath))
                {
                    return dllPath;
                }
            }

            return null;
        }
    }
}
