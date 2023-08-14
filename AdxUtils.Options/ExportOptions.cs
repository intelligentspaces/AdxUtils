using CommandLine;

namespace AdxUtils.Options;

/// <summary>
/// Represents the export options which can be specified at the command line.
/// </summary>
[Verb("export", true, HelpText = "Export a database schema with optional data items")]
public class ExportOptions : IAuthenticationOptions
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
    
    /// <summary>
    /// Gets, sets a collection of table names which should be excluded from the export.
    /// </summary>
    [Option('i', "ignore", Required = false, Separator = ',', HelpText = "Comma-separated list of tables to ignore")]
    public IEnumerable<string>? IgnoredTables { get; set; }

    /// <summary>
    /// Gets, sets a collection of functions names which should not be created.
    /// </summary>
    [Option('f', "function", Required = false, Separator = ',', HelpText = "Comma-separated list of functions or function folders(followed by /) to ignore (e.g. function1,folder1/)")]
    public IEnumerable<string>? IgnoredFunctions { get; set; }
    
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

    [Option('o', "output", Required = false, Default = ".", HelpText = "Path for the output file to be written to")]
    public string OutputPath { get; set; } = ".";

    [Option('u', "update", Required = false, HelpText = "Table to be updated (e.g. table=table1,columnType=int, columnToAdd=column,columnToDrop=column)\"")]
    public IEnumerable<string>? Update { get; set; }

    public DirectoryInfo OutputDirectory { get; private set; } = new (".");

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
    /// Gets the columns to be added or removed in the table
    /// </summary>
    public IEnumerable<(string, string)> ManageColumnsToUpdateInTable
    {
        get
        {
            if (Update == null || !Update.Any())
            {
                return Array.Empty<(string, string)>();
            }

            var result = from updateTable in Update
                         let pair = updateTable.Split('=').Take(2).ToArray()
                         where pair.Length == 2 && pair.All(i => !string.IsNullOrEmpty(i))
                         select (pair[0], pair[1]);

            return result.ToArray();
        }
    }
    /// <summary>
    /// Gets the collection of ignored tables as a non-nullable array.
    /// </summary>
    public IEnumerable<string> IgnoredTablesArray => IgnoredTables?.Select(t => t.Trim()).ToArray() ?? Array.Empty<string>();

    /// <summary>
    /// Gets the collection of ignored functions as a non-nullable array.
    /// </summary>
    public IEnumerable<string> IgnoredFunctionsArray => IgnoredFunctions?.Select(t => t.Trim()).ToArray() ?? Array.Empty<string>();

    /// <summary>
    /// Gets the collection of tables to have their data exported as a non-nullable array.
    /// </summary>
    public IEnumerable<string> ExportedDataTablesArray => ExportedDataTables?.Select(t => t.Trim()).ToArray() ?? Array.Empty<string>();

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

        if (!UseAzureCli && (string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecret) ||
                             string.IsNullOrWhiteSpace(Authority)))
        {
            throw new ArgumentValidationException("When using client secret authentication then the id, secret, and authority must be specified");
        }
    }
}