using System.Text.Json;
using Cruddy.Cli.Services.Base;
using Cruddy.Core.Configuration;

namespace Cruddy.Cli.Services
{
    /// <summary>
    /// Service for reading and managing cruddy.config.json
    /// </summary>
    public class ConfigService
    {
        private readonly IFileSystemService _fileSystem;
        private readonly string _configPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public ConfigService(IFileSystemService fileSystem, string? configPath = null)
        {
            _fileSystem = fileSystem;
            var currentDir = _fileSystem.GetCurrentDirectory();
            _configPath = configPath ?? _fileSystem.CombinePaths(currentDir, "cruddy.config.json");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Loads the cruddy.config.json file
        /// </summary>
        public async Task<CruddyConfig> LoadConfigAsync()
        {
            if (!_fileSystem.FileExists(_configPath))
            {
                throw new InvalidOperationException(
                    "cruddy.config.json not found. Run 'cruddy init' first.");
            }

            var json = await _fileSystem.ReadFileAsync(_configPath);
            var config = JsonSerializer.Deserialize<CruddyConfig>(json, _jsonOptions);

            if (config == null)
            {
                throw new InvalidOperationException("Failed to parse cruddy.config.json");
            }

            return config;
        }

        /// <summary>
        /// Saves the cruddy.config.json file
        /// </summary>
        public async Task SaveConfigAsync(CruddyConfig config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await _fileSystem.WriteFileAsync(_configPath, json);
        }

        /// <summary>
        /// Checks if config file exists
        /// </summary>
        public bool ConfigExists()
        {
            return _fileSystem.FileExists(_configPath);
        }
    }
}
