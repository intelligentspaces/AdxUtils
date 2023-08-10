using Kusto.Data.Common;

namespace AdxUtils.Export;

public interface IKustoQuery
{
    public Task<string> TableDataToCslString(TableSchema table, string tempTableName);
    Task InsertNewColumnInTable(TableSchema table, string newColumnToInsert, string columnType);
    Task DropColumnInTable(TableSchema table, string columnToDrop);
}