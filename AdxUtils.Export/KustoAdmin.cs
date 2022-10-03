using Kusto.Data;
using Kusto.Data.Common;
using Newtonsoft.Json;

namespace AdxUtils.Export;

public class KustoAdmin
{
    private readonly ICslAdminProvider _client;

    private readonly string _databaseName;

    public KustoAdmin(ICslAdminProvider adminProvider)
    {
        _client = adminProvider;
        _databaseName = _client.DefaultDatabaseName;
    }

    public async Task<ClusterSchema> GetDatabaseSchema()
    {
        var query = CslCommandGenerator.GenerateDatabaseSchemaShowAsJsonCommand(_databaseName);
        Console.WriteLine($"Querying database for schema: {query}");

        using var schemaReader =
            await _client.ExecuteControlCommandAsync(_databaseName, query);

        schemaReader.Read();
        var schemaContent = schemaReader.GetString(0);
        try
        {
            var schema = JsonConvert.DeserializeObject<ClusterSchema>(schemaContent);

            if (schema == null || schema.Databases.Count == 0)
            {
                throw new KustoAdminException($"Unable to load schema for database {_databaseName}");
            }

            return schema;
        }
        catch (JsonReaderException ex)
        {
            throw new KustoAdminException("Unable to parse response into a schema", ex);
        }
    }

    public async Task<IList<IngestionMappingShowCommandResult>> GetDatabaseIngestionMappings()
    {
        var query = CslCommandGenerator.GenerateDatabasesIngestionMappingsShowCommand(
            new[] {_databaseName},
            true);
        Console.WriteLine($"Retrieving database ingestion mappings: {query}");

        using var mappingsReader = await _client.ExecuteControlCommandAsync(_databaseName, query);
        var mappings = new List<IngestionMappingShowCommandResult>();

        while (mappingsReader.Read())
        {
            mappings.Add(new IngestionMappingShowCommandResult
            {
                Name = mappingsReader.GetString(0),
                Kind = mappingsReader.GetString(1),
                Mapping = mappingsReader.GetString(2),
                LastUpdatedOn = mappingsReader.GetDateTime(3),
                Database = mappingsReader.GetString(4),
                Table = mappingsReader.GetString(5)
            });
        }

        return mappings;
    }

    public async Task<IList<PolicyShowCommandResult>> GetIngestionTimePolicies()
    {
        var query = CslCommandGenerator.GenerateTableShowIngestionTimePolicyCommand();
        Console.WriteLine($"Retrieving ingestion time policies: {query}");

        using var policyReader = await _client.ExecuteControlCommandAsync(_databaseName, query);
        var policies = new List<PolicyShowCommandResult>();

        while (policyReader.Read())
        {
            policies.Add(new PolicyShowCommandResult
            {
                PolicyName = policyReader.GetString(0),
                EntityName = policyReader.GetString(1),
                Policy = policyReader.GetString(2),
                ChildEntities = policyReader.GetString(3),
                EntityType = policyReader.GetString(4)
            });
        }

        return policies;
    }
}