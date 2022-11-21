using Kusto.Data;
using Kusto.Data.Common;

namespace AdxUtils.Export;

/// <summary>
/// Defines extension methods for the <see cref="IngestionMappingShowCommandResult"/> type.
/// </summary>
public static class IngestionMappingShowCommandResultExtensions
{
    /// <summary>
    /// Generates a CSL string from the mapping.
    /// </summary>
    /// <param name="mapping">The <see cref="IngestionMappingShowCommandResult"/> instance.</param>
    /// <returns>A CSL string for the creation (or modification) of the mapping.</returns>
    public static string ToCslString(this IngestionMappingShowCommandResult mapping)
    {
        var mappingKind = mapping.Kind.ToLower();
        var tableName = CslSyntaxGenerator.NormalizeTableName(mapping.Table);
        var parsedMapping = mapping.Mapping.Replace("'", "\\'");
        return
            $".create-or-alter table {tableName} ingestion {mappingKind} mapping \"{mapping.Name}\" '{parsedMapping}'";
    }
}