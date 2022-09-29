using CommandLine;

namespace AdxUtils.Options;

[Verb("notebook", HelpText = "Generates a spark notebook")]
public class NotebookOptions : IAuthenticationOptions, IEndpointOptions
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

    [Option('l', "language", Required = false, HelpText = "Specifies the language for the notebook. Default is Python.")]
    public LanguageType Language { get; set; } = LanguageType.Python;

    [Option('s', "service", Required = false, HelpText = "Specifies which service the notebook will be deployed to. Default is Databricks.")]
    public ServiceType Service { get; set; } = ServiceType.Databricks;
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