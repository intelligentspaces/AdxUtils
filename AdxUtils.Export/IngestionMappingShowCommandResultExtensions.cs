using Kusto.Data;
using Kusto.Data.Common;

namespace AdxUtils.Export;

public static class IngestionMappingShowCommandResultExtensions
{
    public static string ToCslString(this IngestionMappingShowCommandResult mapping)
    {
        var mappingKind = mapping.Kind.ToLower();
        var tableName = CslSyntaxGenerator.NormalizeTableName(mapping.Table);
        var parsedMapping = mapping.Mapping.Replace("'", "\\'");
        return
            $".create-or-alter table {tableName} ingestion {mappingKind} mapping \"{mapping.Name}\" '{parsedMapping}'";
    }
}