using AdxUtils.Options;
using Kusto.Data.Common;

namespace AdxUtils.Export;

/// <summary>
/// Provides methods for exporting items from an Azure Data Explorer instance.
/// </summary>
public class DatabaseExporter
{
    private readonly ICslQueryProvider _queryProvider;

    private readonly ICslAdminProvider _adminProvider;
    
    public DatabaseExporter(ICslQueryProvider queryProvider, ICslAdminProvider adminProvider)
    {
        _queryProvider = queryProvider;
        _adminProvider = adminProvider;
    }

    /// <summary>
    /// Extracts a database scheme to a CSL script and writes to the provided stream.
    /// </summary>
    /// <param name="options">The <see cref="ExportOptions"/> defined by the client.</param>
    /// <param name="stream">A writeable stream to output the script to.</param>
    /// <exception cref="ArgumentException">Thrown if the provided stream is not valid.</exception>
    public async Task ToCslStreamAsync(ExportOptions options, Stream stream)
    {
        if (!stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable");
        }
        
        var admin = new KustoAdmin(_adminProvider);
        var query = new KustoQuery(_queryProvider);

        await using var writer = new StreamWriter(stream);

        var (_, databaseSchema) = (await admin.GetDatabaseSchema()).Databases.First();
        var databaseMappings = await admin.GetDatabaseIngestionMappings();
        var ingestionTimePolicies = await admin.GetIngestionTimePolicies();
        
        await writer.WriteLineAsync("//");
        await writer.WriteLineAsync("// Create tables");
        await writer.WriteLineAsync("//");
        await writer.WriteLineAsync();
        
        foreach (var tableSchema in databaseSchema.Tables)
        {
            if (options.IgnoredTablesArray.Contains(tableSchema.Key, StringComparer.OrdinalIgnoreCase)) continue;
            
            var ingestionTimePolicy = ingestionTimePolicies.FirstOrDefault(p =>
                p.DatabaseName() == options.DatabaseName && p.TableName() == tableSchema.Key);
            
            var mapping =
                databaseMappings.FirstOrDefault(m => m.Database == options.DatabaseName && m.Table == tableSchema.Key);
            
            await writer.WriteLineAsync($"// Creating {tableSchema.Key}");
            await writer.WriteLineAsync(tableSchema.Value.ToCslString());
            if (ingestionTimePolicy != null)
            {
                await writer.WriteLineAsync();
                await writer.WriteLineAsync(ingestionTimePolicy.ToCslString());
            }

            if (mapping != null)
            {
                await writer.WriteLineAsync();
                await writer.WriteLineAsync(mapping.ToCslString());
            }

            await writer.WriteLineAsync();
        }

        await writer.WriteLineAsync("//");
        await writer.WriteLineAsync("// Create functions");
        await writer.WriteLineAsync("//");
        await writer.WriteLineAsync();
        foreach (var function in databaseSchema.Functions)
        {
            var functionScript = function.Value.ToCslString();

            foreach (var (source, target) in options.RenamedTablePairs)
            {
                if (functionScript.Contains(source))
                {
                    functionScript = functionScript.Replace(source, target, StringComparison.OrdinalIgnoreCase);
                }
            }

            await writer.WriteLineAsync($"// Creating {function.Key}");
            await writer.WriteLineAsync(functionScript);
            await writer.WriteLineAsync();
        }

        await writer.WriteLineAsync("//");
        await writer.WriteLineAsync("// Ingest data");
        await writer.WriteLineAsync("//");
        await writer.WriteLineAsync();
        foreach (var tableSchema in databaseSchema.Tables.Where(tableSchema =>
                     options.ExportedDataTablesArray.Contains(tableSchema.Key, StringComparer.OrdinalIgnoreCase)))
        {
            var temp = tableSchema.Value.Clone() as TableSchema;
            temp!.Name = $"{temp.Name}_temp";
            await writer.WriteLineAsync(temp.ToCslString());
            await writer.WriteLineAsync();
            
            await writer.WriteLineAsync(await query.TableDataToCslString(tableSchema.Value, temp.Name));

            await writer.WriteLineAsync(tableSchema.Value.SetOrReplaceTableCslString(temp.Name));
            await writer.WriteLineAsync();

            await writer.WriteLineAsync(temp.DropTableCslString());
            await writer.WriteLineAsync();
        }
    }
}