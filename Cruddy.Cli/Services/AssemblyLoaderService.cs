using System.Diagnostics;
using System.Reflection;
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
            var absolutePath = Path.IsPathRooted(backendPath)
       ? backendPath
       : Path.GetFullPath(backendPath, Directory.GetCurrentDirectory());

            var projectFile = FindProjectFile(absolutePath);
            if (projectFile == null)
            {
                throw new InvalidOperationException(
                    $"No .csproj file found in '{absolutePath}'. Please check your backend path in cruddy.config.json");
            }

            // Get target framework and assembly name
            var (targetFramework, assemblyName) = GetProjectInfo(projectFile);

            // Build if requested
            if (build)
            {
                await BuildProjectAsync(projectFile);
            }

            // Find the compiled .dll
            var dllPath = FindCompiledAssembly(absolutePath, assemblyName, targetFramework);
            if (dllPath == null)
            {
                throw new InvalidOperationException(
                    $"Could not find compiled assembly for '{assemblyName}'. " +
                    $"Expected path: {absolutePath}/bin/Debug/{targetFramework}/{assemblyName}.dll\n" +
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
        /// Loads all assemblies from the specified path
        /// </summary>
        /// <param name="backendPath">Path to the backend project directory</param>
        /// <param name="build">Whether to build the project first (default: true)</param>
        /// <returns>List of successfully loaded assemblies</returns>
        public async Task<List<Assembly>> LoadAllAssembliesAsync(string backendPath, bool build = true)
        {
            var absolutePath = Path.IsPathRooted(backendPath)
                ? backendPath
                : Path.GetFullPath(backendPath, Directory.GetCurrentDirectory());

            // Find .csproj file
            var projectFile = FindProjectFile(absolutePath);
            if (projectFile == null)
            {
                throw new InvalidOperationException(
                    $"No .csproj file found in '{absolutePath}'. Please check your backend path in cruddy.config.json");
            }

            // Get target framework
            var (targetFramework, assemblyName) = GetProjectInfo(projectFile);

            // Build if requested
            if (build)
            {
                await BuildProjectAsync(projectFile);
            }

            // Find the output directory
            var outputDir = FindOutputDirectory(absolutePath, targetFramework);
            if (outputDir == null || !Directory.Exists(outputDir))
            {
                throw new InvalidOperationException(
                    $"Could not find output directory for target framework '{targetFramework}'. " +
                    $"Try building your project first with 'dotnet build'");
            }

            // Set up assembly resolution for the output directory
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var assemblyName = new AssemblyName(args.Name);
                var dllPath = Path.Combine(outputDir, $"{assemblyName.Name}.dll");

                if (File.Exists(dllPath))
                {
                    try
                    {
                        return Assembly.LoadFrom(dllPath);
                    }
                    catch
                    {
                        return null;
                    }
                }
                return null;
            };

            // Load all assemblies from the output directory
            var assemblies = new List<Assembly>();
            var dlls = Directory.GetFiles(outputDir, "*.dll");


            foreach (var dll in dlls)
            {
                try
                {
                    var asm = Assembly.LoadFrom(dll);
                    assemblies.Add(asm);
                    Console.WriteLine($"  ✓ Loaded: {Path.GetFileName(dll)}");
                }
                catch (Exception ex)
                {
                    // Only log, don't fail - some DLLs might be native or incompatible
                    Console.WriteLine($"  ⊗ Skipped: {Path.GetFileName(dll)} ({ex.GetType().Name})");
                }
            }

            if (assemblies.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No assemblies could be loaded from '{outputDir}'");
            }

            return assemblies;
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
            Console.WriteLine($"Building and publishing project: {Path.GetFileName(projectFile)}...");

            var projectDir = Path.GetDirectoryName(projectFile)!;
            var publishDir = Path.Combine(projectDir, "bin", "publish");

            // Use publish instead of build to get all dependencies
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"publish \"{projectFile}\" -o \"{publishDir}\" --verbosity quiet --no-restore",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start dotnet publish process");
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Publish failed with exit code {process.ExitCode}\n" +
                    $"Output: {output}\n" +
                    $"Error: {error}");
            }

            Console.WriteLine("Publish completed successfully");
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

        /// <summary>
        /// Finds the output directory containing compiled assemblies
        /// </summary>
        private string? FindOutputDirectory(string backendPath, string targetFramework)
        {
            // First try the publish directory
            var publishDir = Path.Combine(backendPath, "bin", "publish");
            if (Directory.Exists(publishDir))
            {
                return publishDir;
            }

            // Fall back to Debug/Release
            var configurations = new[] { "Debug", "Release" };

            foreach (var config in configurations)
            {
                var outputDir = _fileSystem.CombinePaths(
                    backendPath,
                    "bin",
                    config,
                    targetFramework);

                if (_fileSystem.DirectoryExists(outputDir))
                {
                    return outputDir;
                }
            }

            return null;
        }
    }
}
