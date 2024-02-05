using System.Data.SqlTypes;
using System.Globalization;
using System.Text;
using AdxUtils.Options;
using Kusto.Data.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdxUtils.Export;

public class KustoQuery : IKustoQuery
{
    private readonly ICslQueryProvider _client;

    private readonly ICslAdminProvider _adminProvider;

    private readonly string _databaseName;

    public KustoQuery(ICslQueryProvider queryProvider, ICslAdminProvider adminProvider)
    {
        _client = queryProvider;
        _databaseName = _client.DefaultDatabaseName;
        _adminProvider = adminProvider;
    }

    public async Task<string> TableDataToCslString(TableSchema table, string schema)
    {
        var queryBuilder = new StringBuilder();

        Console.WriteLine($"Retrieving table data for: {table.NormalizeTableName(_databaseName)}");
        var tableData = await GetTableData(table);

        if (!tableData.Any()) return string.Empty;

        queryBuilder.AppendLine($".set-or-replace {table.Name}");
        queryBuilder.AppendLine($"with(policy_ingestiontime = true, distributed = False) <| datatable ({schema})");
        foreach (var record in tableData)
        {
            queryBuilder.AppendLine(record);
        }

        return queryBuilder.ToString();
    }

    private async Task<IList<string>> GetTableData(TableSchema table)
    {
        var query = CslSyntaxGenerator.NormalizeTableName(table.Name);

        var records = new List<string>();
        
        var clientRequestProperties = new ClientRequestProperties
        {
            ClientRequestId = $"AdxUtils.Export;{Guid.NewGuid().ToString()}"
        };
        clientRequestProperties.SetOption(ClientRequestProperties.OptionNoTruncation, true);

        var results = await _client.ExecuteQueryAsync(_databaseName, query, clientRequestProperties);

        while (results.Read())
        {
            var row = new object[table.Columns.Count];
            results.GetValues(row);
            var ingestRecord = ToCslIngestInlineRecord(row);
            records.Add(ingestRecord);
        }

        return records;
    }

    private static string ToCslIngestInlineRecord(IReadOnlyList<object> row)
    {
        var values = new List<string>();
        for (var index = 0; index < row.Count; index++)
        {
            var value = row[index];

            var ingestValue = value switch
            {
                sbyte s => s.ToString(),
                bool b => b ? "1": "0",
                DateTime dateTime => dateTime.ToString("o"),
                Guid guid => guid.ToString(),
                int i => i.ToString(),
                long l => l.ToString(),
                double d => d.ToString(CultureInfo.InvariantCulture),
                string s => $"\"{s.Replace("\"", "\"\"")}\"",
                TimeSpan ts => ts.ToString(),
                SqlDecimal d => d.ToString(),
                JObject jObject => $"\"{jObject.ToString(Formatting.None).Replace("\"", "\"\"")}\"",
                JArray jArray => $"\"{jArray.ToString(Formatting.None).Replace("\"", "\"\"")}\"",
                _ => string.Empty
            };

            values.Add(ingestValue);
        }

        return string.Join(",", values);
    }

    public async Task DropColumnInTable(TableSchema table, string columnToDrop)
    {
       
        var query = $".drop table {table.Name} columns({columnToDrop})";
        var clientRequestProperties = new ClientRequestProperties
        {
            ClientRequestId = $"AdxUtils.Export;{Guid.NewGuid().ToString()}"
        };
        clientRequestProperties.SetOption(ClientRequestProperties.OptionNoTruncation, true);
        try
        {
            await _adminProvider.ExecuteControlCommandAsync(_databaseName, query, clientRequestProperties);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException("Unable to drop the column from the specified table.", ex);
        }
         
    }
    public async Task InsertNewColumnInTable(TableSchema table, string newColumnToInsert, string columnType)
    {
        var queryBuilder = new StringBuilder();
        foreach (var record in table.Columns)
        {
            queryBuilder.AppendLine($"{record.Key} : {record.Value.CslType},");
        }
        var query = $".create-merge table {table.Name} ( {queryBuilder} {newColumnToInsert}: {columnType})";

        var clientRequestProperties = new ClientRequestProperties
        {
            ClientRequestId = $"AdxUtils.Export;{Guid.NewGuid().ToString()}"
        };
        clientRequestProperties.SetOption(ClientRequestProperties.OptionNoTruncation, true);
        try
        {
            await _adminProvider.ExecuteControlCommandAsync(_databaseName, query, clientRequestProperties);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException("Unable to insert the column in the specified table.", ex);
        }

    }
}