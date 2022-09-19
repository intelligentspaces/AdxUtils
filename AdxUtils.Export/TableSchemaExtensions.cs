using Kusto.Data.Common;

namespace AdxUtils.Export;

/// <summary>
/// Defines extension methods for the <see cref="TableSchema"/> type.
/// </summary>
public static class TableSchemaExtensions
{
    /// <summary>
    /// Normalizes the table name to either the table name, or with the database prefix if available. For example.
    ///
    /// my table => ['my table']
    /// </summary>
    /// <param name="tableSchema">The <see cref="TableSchema"/> instance.</param>
    /// <param name="database">Optional database name, default is null.</param>
    /// <returns>A normalized table name.</returns>
    public static string NormalizeTableName(this TableSchema tableSchema, string? database = null)
    {
        return string.IsNullOrEmpty(database)
            ? CslSyntaxGenerator.NormalizeTableName(tableSchema.Name)
            : $"{CslSyntaxGenerator.NormalizeDatabaseName(database)}.{CslSyntaxGenerator.NormalizeTableName(tableSchema.Name)}";
    }

    /// <summary>
    /// Gets the 'set-or-replace' CSL statement for the current instance. 
    /// </summary>
    /// <param name="tableSchema">The <see cref="TableSchema"/> instance.</param>
    /// <param name="tempTableName">The name of the temporary table.</param>
    /// <returns>The CSL statement as a string.</returns>
    public static string SetOrReplaceTableCslString(this TableSchema tableSchema, string tempTableName)
    {
        return CslCommandGenerator.GenerateTableSetOrReplaceCommand(
            CslSyntaxGenerator.NormalizeTableName(tableSchema.Name),
            CslSyntaxGenerator.NormalizeTableName(tempTableName),
            false);
    }
    
    /// <summary>
    /// Creates a drop table CSL statement for the table.
    /// </summary>
    /// <param name="tableSchema">The <see cref="TableSchema"/> instance.</param>
    /// <returns>The drop table CSL statement.</returns>
    public static string DropTableCslString(this TableSchema tableSchema)
    {
        return CslCommandGenerator.GenerateTableDropCommand(
            CslSyntaxGenerator.NormalizeTableName(tableSchema.Name),
            true);
    }
}