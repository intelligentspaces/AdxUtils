using CommandLine;

namespace AdxUtils.Options;

public interface IAuthenticationOptions
{
    [Option("use-cli", Required = false, Group = "Authentication Method", HelpText = "Use the Azure CLI for authentication")]
    public bool UseAzureCli { get; set; }
    
    [Option("client-id", Required = false, Group = "Authentication Method", HelpText = "The application id (client id) of the service principal being used for authentication")]
    public string ClientId { get; set; }

    [Option("client-secret", Required = false, HelpText = "The client secret of the service principal being used for authentication")]
    public string ClientSecret { get; set; }

    [Option("authority", Required = false, HelpText = "The authority (e.g. contoso.com) or AAD tenant id to authenticate against")]
    public string Authority { get; set; }
}