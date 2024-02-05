using Kusto.Data.Common;
using System.Text;

namespace AdxUtils.Export;

public interface IKustoQuery
{
    //public Task<string> TableDataToCslString(TableSchema table, string tempTableName);
    public Task<string> TableDataToCslString(TableSchema table, string queryBuilder);
    Task InsertNewColumnInTable(TableSchema table, string newColumnToInsert, string columnType);
    Task DropColumnInTable(TableSchema table, string columnToDrop);
}