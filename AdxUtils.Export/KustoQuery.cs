using System.Data.SqlTypes;
using System.Globalization;
using System.Text;
using AdxUtils.Options;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;

namespace AdxUtils.Export;

public class KustoQuery : IDisposable
{
    private readonly ICslQueryProvider _client;

    private readonly string _databaseName;

    public KustoQuery(IAuthenticationOptions authenticationOptions, IEndpointOptions endpointOptions)
    {
        _databaseName = endpointOptions.DatabaseName;

        var connectionStringBuilder = Authentication.GetConnectionStringBuilder(authenticationOptions, endpointOptions);
        _client = KustoClientFactory.CreateCslQueryProvider(connectionStringBuilder);
    }

    public KustoQuery(KustoConnectionStringBuilder connectionStringBuilder)
    {
        _client = KustoClientFactory.CreateCslQueryProvider(connectionStringBuilder);
        _databaseName = _client.DefaultDatabaseName;
    }

    public async Task<string> TableDataToCslString(TableSchema table, string tempTableName)
    {
        var queryBuilder = new StringBuilder();

        Console.WriteLine($"Retrieving table data for: {table.NormalizeTableName(_databaseName)}");
        var tableData = await GetTableData(table);

        if (!tableData.Any()) return string.Empty;
        
        queryBuilder.AppendLine($".ingest inline into table {CslSyntaxGenerator.NormalizeTableName(tempTableName)} <|");
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
                _ => string.Empty
            };

            values.Add(ingestValue);
        }

        return string.Join(",", values);
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}