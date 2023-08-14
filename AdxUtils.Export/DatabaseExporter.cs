using AdxUtils.Options;
using Kusto.Data.Common;

namespace AdxUtils.Export;

/// <summary>
/// Provides methods for exporting items from an Azure Data Explorer instance.
/// </summary>
public class DatabaseExporter
{
    private readonly IKustoAdmin _adminService;

    private readonly IKustoQuery _queryService;
    
    public DatabaseExporter(IKustoAdmin adminService, IKustoQuery queryService)
    {
        _adminService = adminService;
        _queryService = queryService;
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

        await using var writer = new StreamWriter(stream);

        var (_, databaseSchema) = (await _adminService.GetDatabaseSchema()).Databases.First();
        var databaseMappings = await _adminService.GetDatabaseIngestionMappings();
        var ingestionTimePolicies = await _adminService.GetIngestionTimePolicies();
        
        await writer.WriteLineAsync("//");
        await writer.WriteLineAsync("// Create tables");
        await writer.WriteLineAsync("//");
        await writer.WriteLineAsync();
        
        foreach (var tableSchema in databaseSchema.Tables)
        {
            var currentTableSchema = tableSchema;
            if (options.IgnoredTablesArray.Contains(tableSchema.Key, StringComparer.OrdinalIgnoreCase)) continue;
            
            var tableScript = tableSchema.Value.ToCslString();
            if (options.Update != null)
            {
                string? columnType = options.ManageColumnsToUpdateInTable.FirstOrDefault(t => t.Item1.Contains("columnType")).Item2;
                string? columnToAdd = options.ManageColumnsToUpdateInTable.FirstOrDefault(t => t.Item1.Contains("columnToAdd")).Item2;
                string? tableToUpdate = options.ManageColumnsToUpdateInTable.FirstOrDefault(t => t.Item1.Contains("table")).Item2;
                string? columnToDelete = options.ManageColumnsToUpdateInTable.FirstOrDefault(t => t.Item1.Contains("columnToDrop")).Item2;
                if (!String.IsNullOrEmpty(tableToUpdate)  && tableScript.Contains(tableToUpdate))
                {
                    //Verify the column to add is not already defined
                    var existingColumns = tableSchema.Value.Columns.Keys.ToList();
                    if (!String.IsNullOrEmpty(columnToAdd) && !existingColumns.Contains(columnToAdd) && !String.IsNullOrEmpty(columnType))
                    {
                        await _queryService.InsertNewColumnInTable(tableSchema.Value, columnToAdd, columnType);
                    }
                    if(!String.IsNullOrEmpty(columnToDelete))
                    {
                        await _queryService.DropColumnInTable(tableSchema.Value, columnToDelete);
                    }
                    //Update Database Schema
                     (_, databaseSchema) = (await _adminService.GetDatabaseSchema()).Databases.First();
                     var tableUpdated = databaseSchema.Tables.ToList().FirstOrDefault(t => t.Key.Equals(tableToUpdate));
                     currentTableSchema = tableUpdated;
                }
            }
            if (options.IgnoredTablesArray.Contains(currentTableSchema.Key, StringComparer.OrdinalIgnoreCase)) continue;
            
            var ingestionTimePolicy = ingestionTimePolicies.FirstOrDefault(p =>
                p.DatabaseName() == options.DatabaseName && p.TableName() == currentTableSchema.Key);
            
            var mapping =
                databaseMappings.FirstOrDefault(m => m.Database == options.DatabaseName && m.Table == currentTableSchema.Key);
            
            await writer.WriteLineAsync($"// Creating {currentTableSchema.Key}");
            await writer.WriteLineAsync(currentTableSchema.Value.ToCslString());
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
            if (options.IgnoredFunctionsArray.Contains(function.Key, StringComparer.OrdinalIgnoreCase) || options.IgnoredFunctionsArray.Contains($"{function.Value.Folder}/", StringComparer.OrdinalIgnoreCase)) continue;

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
        foreach (var currentTableSchema in databaseSchema.Tables.Where(currentTableSchema =>
                     options.ExportedDataTablesArray.Contains(currentTableSchema.Key, StringComparer.OrdinalIgnoreCase)))
        {
            var temp = currentTableSchema.Value.Clone() as TableSchema;
            temp!.Name = $"{temp.Name}_temp";
            await writer.WriteLineAsync(temp.ToCslString());
            await writer.WriteLineAsync();
            
            await writer.WriteLineAsync(await _queryService.TableDataToCslString(currentTableSchema.Value, temp.Name));

            await writer.WriteLineAsync(currentTableSchema.Value.SetOrReplaceTableCslString(temp.Name));
            await writer.WriteLineAsync();

            await writer.WriteLineAsync(temp.DropTableCslString());
            await writer.WriteLineAsync();
        }
    }
}