using Kusto.Data.Common;

namespace AdxUtils.Export;

public static class TableSchemaExtensions
{
    public static string NormalizeTableName(this TableSchema tableSchema, string? database = null)
    {
        return string.IsNullOrEmpty(database)
            ? CslSyntaxGenerator.NormalizeTableName(tableSchema.Name)
            : $"{CslSyntaxGenerator.NormalizeDatabaseName(database)}.{CslSyntaxGenerator.NormalizeTableName(tableSchema.Name)}";
    }

    public static string SetOrReplaceTableCslString(this TableSchema tableSchema, string tempTableName)
    {
        return CslCommandGenerator.GenerateTableSetOrReplaceCommand(
            CslSyntaxGenerator.NormalizeTableName(tableSchema.Name),
            CslSyntaxGenerator.NormalizeTableName(tempTableName),
            false);
    }
    
    public static string DropTableCslString(this TableSchema tableSchema)
    {
        return CslCommandGenerator.GenerateTableDropCommand(
            CslSyntaxGenerator.NormalizeTableName(tableSchema.Name),
            true);
    }
}