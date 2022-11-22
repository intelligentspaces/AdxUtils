﻿using System.Data;
using AdxUtils.Options;
using Microsoft.Data.Sqlite;

namespace AdxUtils.Export;

public class OutputToSqlite
{
    private readonly IKustoQuery _queryService;

    public OutputToSqlite(IKustoQuery queryService)
    {
        _queryService = queryService;
    }

    public async Task ToSqliteDb(ExportToSqlOptions exportToSqlOptions)
    {
        var records = await _queryService.ExecuteQuery(exportToSqlOptions.Query);

        var connectionBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = exportToSqlOptions.OutputFile.FullName
        };

        await using var connection = new SqliteConnection(connectionBuilder.ConnectionString);

        try
        {
            await connection.OpenAsync();
                
            var isFirst = true;

            foreach (var record in records)
            {
                if (isFirst)
                {
                    var columns = string.Join(", ", record.Fields.Select(f => $"[{f}] TEXT"));
                    var createTableStatement = $"CREATE TABLE export ({columns})";

                    var command = connection.CreateCommand();
                    command.CommandText = createTableStatement;
                    command.CommandType = CommandType.Text;

                    await command.ExecuteNonQueryAsync();
                
                    isFirst = false;
                }

                var insertColumns = string.Join(", ", record.Fields.Select(f => $"[{f}]"));
                var parameters = string.Join(", ", Enumerable.Range(1, record.FieldCount).Select(i => $"$p{i}"));

                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = $"INSERT INTO export ({insertColumns}) VALUES ({parameters})";

                for (var i = 1; i <= record.FieldCount; i++)
                {
                    insertCommand.Parameters.Add(new SqliteParameter($"$p{i}", record.Values[i - 1]));
                }

                await insertCommand.ExecuteNonQueryAsync();
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}