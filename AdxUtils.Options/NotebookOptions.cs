using CommandLine;

namespace AdxUtils.Options;

[Verb("notebook", HelpText = "Generates a spark notebook")]
public class NotebookOptions : IAuthenticationOptions
{
    /// <summary>
    /// Gets, sets the Azure Data Explorer cluster URL.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets, sets the name of the database to connect to.
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets, sets a flag indicating if the Azure CLI credentials should be used for authentication.
    /// </summary>
    public bool UseAzureCli { get; set; }
    
    /// <summary>
    /// Gets, sets the client id (application id) of the client to use for authentication.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets, sets the client secret of the client to use for authentication.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets, sets the authority to authenticate against.
    /// </summary>
    public string Authority { get; set; } = string.Empty;
    
    [Option('n', "name", Required = false, HelpText = "The name of the notebook")]
    public string Name { get; set; }

    [Option('l', "language", Required = false, HelpText = "Specifies the language for the notebook. Default is Python.")]
    public LanguageType Language { get; set; } = LanguageType.Python;

    [Option('s', "service", Required = false, HelpText = "Specifies which service the notebook will be deployed to. Default is Databricks.")]
    public ServiceType Service { get; set; } = ServiceType.Databricks;
    
    [Option('q', "query", Required = false, Group = "Query Source", HelpText = "The query to execute as part of the generated notebook")]
    public string Query { get; set; }
    
    [Option('i', "query-path", Required = false, Group = "Query Source", HelpText = "A path to a file containing the query to execute as part of the generated notebook")]
    public string QueryFilePath { get; set; }

    [Option('o', "output", Required = false, Default = ".", HelpText = "Path for the output file to be written to")]
    public string OutputPath { get; set; } = ".";

    public DirectoryInfo OutputDirectory { get; private set; } = new (".");

    public async Task<string> GetQuery()
    {
        if (!string.IsNullOrEmpty(Query))
        {
            return Query;
        }

        return (await File.ReadAllTextAsync(QueryFilePath)).Trim().Trim('\n', '\r');
    }

    public void Validate()
    {
        ((IAuthenticationOptions)this).ValidateAuthenticationOptions();

        if (!string.IsNullOrEmpty(QueryFilePath) && !File.Exists(QueryFilePath))
        {
            throw new ArgumentValidationException("The specified query path is not valid");
        }
        
        try
        {
            OutputDirectory = new DirectoryInfo(OutputPath);
            if (!OutputDirectory.Exists)
            {
                OutputDirectory.Create();
            }
        }
        catch (Exception e)
        {
            throw new ArgumentValidationException("Unable to create output directory", e);
        }
    }
}

public enum LanguageType
{
    Scala,
    Python
}

public enum ServiceType
{
    Databricks,
    Synapse,
    StandAlone
}