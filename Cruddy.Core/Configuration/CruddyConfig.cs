namespace Cruddy.Core.Configuration;

/// <summary>
/// Root configuration for Cruddy project
/// </summary>
public class CruddyConfig
{
    public BackendConfig Backend { get; set; } = new();
    public FrontendConfig Frontend { get; set; } = new();
    public GenerateConfig Generate { get; set; } = new();
    public List<string> Customized { get; set; } = new();
}

/// <summary>
/// Backend configuration
/// </summary>
public class BackendConfig
{
    public string Path { get; set; } = "./MyApp.Api";
}

/// <summary>
/// Frontend configuration
/// </summary>
public class FrontendConfig
{
    public string Path { get; set; } = "./client/src";
    public string OutputDir { get; set; } = "./client/src/components";
    public string BaseUrl { get; set; } = "/api";
}

/// <summary>
/// Generation configuration
/// </summary>
public class GenerateConfig
{
    public string Extension { get; set; } = ".cruddy.tsx";
    public string TemplatePath { get; set; } = "./templates/react-ts/"; 
}