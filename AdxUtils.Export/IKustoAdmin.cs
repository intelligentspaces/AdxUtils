using Kusto.Data;
using Kusto.Data.Common;

namespace AdxUtils.Export;

public interface IKustoAdmin
{
    public Task<ClusterSchema> GetDatabaseSchema();

    public Task<IList<IngestionMappingShowCommandResult>> GetDatabaseIngestionMappings();

    public Task<IList<PolicyShowCommandResult>> GetIngestionTimePolicies();
}