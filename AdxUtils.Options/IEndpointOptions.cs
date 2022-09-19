using CommandLine;

namespace AdxUtils.Options;

public interface IEndpointOptions
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
}