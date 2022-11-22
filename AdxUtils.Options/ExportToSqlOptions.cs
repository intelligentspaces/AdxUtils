using CommandLine;

namespace AdxUtils.Options;

[Verb("sql", isDefault:false, HelpText = "Export the results of the provided query to a sqlite database")]
public class ExportToSqlOptions : IAuthenticationOptions
{
    public string Endpoint { get; set; } = string.Empty;
    
    public string DatabaseName { get; set; } = string.Empty;
    
    public bool UseAzureCli { get; set; }
    
    public string ClientId { get; set; } = string.Empty;
    
    public string ClientSecret { get; set; } = string.Empty;
    
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Gets, sets the Azure Data Explorer cluster URL.
    /// </summary>
    [Option('q', "query", Required = false, HelpText = "The query to run against ADX")]
    public string Query { get; set; }
    
    [Option('o', "output", Required = false, Default = "export.db", HelpText = "Path for the output file to be written to")]
    public string OutputPath { get; set; } = "export.db";

    public FileInfo OutputFile => new FileInfo(OutputPath);
    
    public void ValidateExportOptions()
    {
        ((IAuthenticationOptions)this).Validate();
    }
}