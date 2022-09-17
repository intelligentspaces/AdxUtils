using CommandLine;

namespace AdxUtils.Options;

[Verb("export", true, HelpText = "Export a database schema with optional data items")]
public class ExportOptions : IAuthenticationOptions, IEndpointOptions
{
    public string Endpoint { get; set; } = string.Empty;
    
    public string DatabaseName { get; set; } = string.Empty;

    public bool UseAzureCli { get; set; }
    
    public bool Prompt { get; set; }
    
    public string ClientId { get; set; } = string.Empty;
    
    public string ClientSecret { get; set; } = string.Empty;
    
    public string Authority { get; set; } = string.Empty;
    
    [Option('i', "ignore", Required = false, Separator = ',', HelpText = "Comma-separated list of tables to ignore")]
    public IEnumerable<string>? IgnoredTables { get; set; }
    
    [Option('r', "rename", Required = false, Separator = ',', HelpText = "Comma-separated list of tables to be renamed (e.g. table1=table2,table3=table4)")]
    public IEnumerable<string>? RenamedTables { get; set; }
    
    [Option('e', "export", Required = false, Separator = ',', HelpText = "Comma-separated list of tables to export data for")]
    public IEnumerable<string>? ExportedDataTables { get; set; }

    public IEnumerable<(string, string)> RenamedTablePairs
    {
        get
        {
            if (RenamedTables == null || !RenamedTables.Any())
            {
                return Array.Empty<(string, string)>();
            }

            var result = from renamedTable in RenamedTables
                let pair = renamedTable.Split('=', 2)
                where pair.Length == 2
                select (pair[0], pair[1]);

            return result.ToArray();
        }
    }

    public IEnumerable<string> IgnoredTablesArray => IgnoredTables?.ToArray() ?? Array.Empty<string>();

    public IEnumerable<string> ExportedDataTablesArray => ExportedDataTables?.ToArray() ?? Array.Empty<string>();

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