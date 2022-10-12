using CommandLine;

namespace AdxUtils.Options;

public interface IAuthenticationOptions
{
    /// <summary>
    /// Gets, sets the Azure Data Explorer cluster URL.
    /// </summary>
    [Option('c', "cluster", Required = true, HelpText = "The cluster id to export from")]
    public string Endpoint { get; set; }
    
    /// <summary>
    /// Gets, sets the name of the database to connect to.
    /// </summary>
    [Option('d', "database", Required = true, HelpText = "Specifies the database to be exported")]
    public string DatabaseName { get; set; }
    
    /// <summary>
    /// Gets, sets a flag indicating if the Azure CLI credentials should be used for authentication.
    /// </summary>
    [Option("use-cli", Required = false, Group = "Authentication Method", HelpText = "Use the Azure CLI for authentication")]
    public bool UseAzureCli { get; set; }
    
    /// <summary>
    /// Gets, sets the client id (application id) of the client to use for authentication.
    /// </summary>
    [Option("client-id", Required = false, Group = "Authentication Method", HelpText = "The application id (client id) of the service principal being used for authentication")]
    public string ClientId { get; set; }

    /// <summary>
    /// Gets, sets the client secret of the client to use for authentication.
    /// </summary>
    [Option("client-secret", Required = false, HelpText = "The client secret of the service principal being used for authentication")]
    public string ClientSecret { get; set; }

    /// <summary>
    /// Gets, sets the authority to authenticate against.
    /// </summary>
    [Option("authority", Required = false, HelpText = "The authority (e.g. contoso.com) or AAD tenant id to authenticate against")]
    public string Authority { get; set; }
}