using CommandLine;

namespace AdxUtils.Options;

public interface IEndpointOptions
{
    [Option('c', "cluster", Required = true, HelpText = "The cluster id to export from")]
    public string Endpoint { get; set; }
    
    [Option('d', "database", Required = true, HelpText = "Specifies the database to be exported")]
    public string DatabaseName { get; set; }
}