using Kusto.Data.Common;

namespace AdxUtils.Export;

public interface IKustoQuery
{
    public Task<string> TableDataToCslString(TableSchema table, string tempTableName);

    public Task<(bool, string?)> IsValidQuery(string query);
}