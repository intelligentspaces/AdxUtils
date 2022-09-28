using CommandLine;

namespace AdxUtils.Options;

/// <summary>
/// Represents the export options which can be specified at the command line.
/// </summary>
[Verb("export", true, HelpText = "Export a database schema with optional data items")]
public class ExportOptions : IAuthenticationOptions, IEndpointOptions
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
    /// Gets, sets the authority to authenticate agaisnt.
    /// </summary>
    public string Authority { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets, sets a collection of table names which should be excluded from the export.
    /// </summary>
    [Option('i', "ignore", Required = false, Separator = ',', HelpText = "Comma-separated list of tables to ignore")]
    public IEnumerable<string>? IgnoredTables { get; set; }
    
    /// <summary>
    /// Gets, sets a collection of key=value pairs of tables to be renamed.
    /// </summary>
    [Option('r', "rename", Required = false, Separator = ',', HelpText = "Comma-separated list of tables to be renamed (e.g. table1=table2,table3=table4)")]
    public IEnumerable<string>? RenamedTables { get; set; }
    
    /// <summary>
    /// Gets, sets a collection of tables to export the data for.
    /// </summary>
    [Option('e', "export", Required = false, Separator = ',', HelpText = "Comma-separated list of tables to export data for")]
    public IEnumerable<string>? ExportedDataTables { get; set; }

    /// <summary>
    /// Gets the tables to be renamed as pairs of values.
    /// </summary>
    public IEnumerable<(string, string)> RenamedTablePairs
    {
        get
        {
            if (RenamedTables == null || !RenamedTables.Any())
            {
                return Array.Empty<(string, string)>();
            }

            var result = from renamedTable in RenamedTables
                let pair = renamedTable.Split('=').Take(2).ToArray()
                where pair.Length == 2 && pair.All(i => !string.IsNullOrEmpty(i))
                select (pair[0], pair[1]);

            return result.ToArray();
        }
    }

    /// <summary>
    /// Gets the collection of ignored tables as a non-nullable array.
    /// </summary>
    public IEnumerable<string> IgnoredTablesArray => IgnoredTables?.ToArray() ?? Array.Empty<string>();

    /// <summary>
    /// Gets the collection of tables to have their data exported as a non-nullable array.
    /// </summary>
    public IEnumerable<string> ExportedDataTablesArray => ExportedDataTables?.ToArray() ?? Array.Empty<string>();

    /// <summary>
    /// Validates the values provided for the export options.
    /// </summary>
    /// <exception cref="ArgumentValidationException">Thrown when the option values are invalid.</exception>
    public void Validate()
    {
        // Validate the endpoint contains a valid URL
        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out var validatedUri))
        {
            throw new ArgumentValidationException("The cluster should be a valid, absolute, uri such as 'https://<adx name>.<region>.kusto.windows.net'");
        }

        Endpoint = validatedUri.ToString();

        if (!UseAzureCli && (string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecret) ||
                             string.IsNullOrWhiteSpace(Authority)))
        {
            throw new ArgumentValidationException("When using client secret authentication then the id, secret, and authority must be specified");
        }
    }
}