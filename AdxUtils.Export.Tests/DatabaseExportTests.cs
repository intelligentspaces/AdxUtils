using System.Text;
using AdxUtils.Options;

namespace AdxUtils.Export.Tests;

public class DatabaseExportTests
{
    private const string DatabaseName = "db01";
    
    [Fact]
    public async Task WhenExportingADatabase_GivenASimpleStructure_ThenContentIsExportedCorrectly()
    {
        var (adminService, queryService) = CreateMocks();

        var options = new ExportOptions
        {
            Endpoint = "https://valid.url",
            UseAzureCli = true,
            DatabaseName = DatabaseName,
            ExportedDataTables = new[] { "table1" }
        };

        var expected = (await File.ReadAllLinesAsync("TestData/SimpleExportResult.csl"))
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();

        var exporter = new DatabaseExporter(adminService.Object, queryService.Object);
        var ms = new MemoryStream();

        await exporter.ToCslStreamAsync(options, ms);

        var result = Encoding.UTF8.GetString(ms.GetBuffer()).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();

        result.Should().NotBeEmpty().And.Contain(expected);
    }
    
    [Fact]
    public async Task WhenExportingADatabase_WhenTableIsIgnored_ThenItIsExcludedFromOutput()
    {
        var (adminService, queryService) = CreateMocks();

        var options = new ExportOptions
        {
            Endpoint = "https://valid.url",
            UseAzureCli = true,
            DatabaseName = DatabaseName,
            ExportedDataTables = new[] { "table1" },
            IgnoredTables = new[] { "table2" }
        };

        var expected = (await File.ReadAllLinesAsync("TestData/SimpleExportIgnoredTable.csl"))
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();

        var exporter = new DatabaseExporter(adminService.Object, queryService.Object);
        var ms = new MemoryStream();

        await exporter.ToCslStreamAsync(options, ms);

        var result = Encoding.UTF8.GetString(ms.GetBuffer()).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();

        result.Should().NotBeEmpty().And.Contain(expected);
    }
    
    [Fact]
    public async Task WhenExportingADatabase_WhenTableIsRenamed_ThenFunctionsAreModified()
    {
        var (adminService, queryService) = CreateMocks();

        var options = new ExportOptions
        {
            Endpoint = "https://valid.url",
            UseAzureCli = true,
            DatabaseName = DatabaseName,
            ExportedDataTables = new[] { "table1" },
            IgnoredTables = new[] { "table2" },
            RenamedTables = new[] { "table2=table1" }
        };

        var expected = (await File.ReadAllLinesAsync("TestData/SimpleExportRenamedTable.csl"))
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();

        var exporter = new DatabaseExporter(adminService.Object, queryService.Object);
        var ms = new MemoryStream();

        await exporter.ToCslStreamAsync(options, ms);

        var result = Encoding.UTF8.GetString(ms.GetBuffer()).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();

        result.Should().NotBeEmpty().And.Contain(expected);
    }

    [Fact]
    public async Task WhenExportingADatabase_WhenStreamIsNotWriteable_ThenExceptionIsThrown()
    {
        var options = new ExportOptions();
        var adminService = new Mock<IKustoAdmin>();
        var queryService = new Mock<IKustoQuery>();

        var exporter = new DatabaseExporter(adminService.Object, queryService.Object);

        var testStream = new NonWriteableStream();
        var act = () => exporter.ToCslStreamAsync(options, testStream);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Stream must be writable");
    }

    private static (Mock<IKustoAdmin>, Mock<IKustoQuery>) CreateMocks()
    {
        var dbSchema = new DatabaseSchema(DatabaseName)
        {
            Tables = new Dictionary<string, TableSchema>
            {
                {
                    "table1", new TableSchema("table1", new[]
                    {
                        ColumnSchema.FromNameAndCslType("col1", "string"),
                        ColumnSchema.FromNameAndCslType("col2", "datetime")
                    })
                },
                {
                "table2", new TableSchema("table2", new[]
                {
                    ColumnSchema.FromNameAndCslType("col1", "string"),
                    ColumnSchema.FromNameAndCslType("col2", "guid")
                })
            }
            },
            Functions = new Dictionary<string, FunctionSchema>
            {
                {
                    "simpleFunc1",
                    new FunctionSchema
                    (
                        "simpleFunc1",
                        null,
                        "\r\ntable1\r\n| limit 10",
                        "test",
                        "Simple test function",
                        FunctionSchema.FunctionKind.UnknownFunction, null
                    )
                },
                {
                    "simpleFunc2",
                    new FunctionSchema
                    (
                        "simpleFunc2",
                        null,
                        "\r\ntable2\r\n| limit 100",
                        "test",
                        "Another simple test function",
                        FunctionSchema.FunctionKind.UnknownFunction, null
                    )
                }
            }
        };

        var clusterSchema = new ClusterSchema
        {
            Databases = {{DatabaseName, dbSchema}}
        };

        var ingestionMappings = new List<IngestionMappingShowCommandResult>
        {
            new ()
            {
                Database = DatabaseName,
                Kind = "json",
                Table = "table1",
                Name = "table1_mapping",
                Mapping =
                    "[{\"column\":\"col1\",\"path\":\"$.col1\",\"datatype\":\"string\",\"transform\":null},{\"column\":\"col2\",\"path\":\"$.col2\",\"datatype\":\"datetime\",\"transform\":null}]"
            },
            new ()
            {
                Database = DatabaseName,
                Kind = "json",
                Table = "table2",
                Name = "table2_mapping",
                Mapping =
                    "[{\"column\":\"col1\",\"path\":\"$.col1\",\"datatype\":\"string\",\"transform\":null},{\"column\":\"col2\",\"path\":\"$.col2\",\"datatype\":\"guid\",\"transform\":null}]"
            },
            new ()
            {
                Database = DatabaseName,
                Kind = "json",
                Table = "table3",
                Name = "table3_mapping",
                Mapping =
                    "[{\"column\":\"col1\",\"path\":\"$.col1\",\"datatype\":\"datetime\",\"transform\":null},{\"column\":\"col2\",\"path\":\"$.col2\",\"datatype\":\"timespan\",\"transform\":null}]"
            }
        };

        var ingestionTimePolicies = new List<PolicyShowCommandResult>
        {
            new ()
            {
                PolicyName = "IngestionTimePolicy", EntityName = $"[{DatabaseName}].[table1]",
                Policy = "{ \"IsEnabled\": true }", ChildEntities = null, EntityType = "Table"
            },
            new ()
            {
                PolicyName = "IngestionTimePolicy", EntityName = $"[{DatabaseName}].[table2]",
                Policy = "{ \"IsEnabled\": true }", ChildEntities = null, EntityType = "Table"
            },
            new ()
            {
                PolicyName = "IngestionTimePolicy", EntityName = $"[{DatabaseName}].[table3]",
                Policy = "{ \"IsEnabled\": true }", ChildEntities = null, EntityType = "Table"
            }
        };

        var adminService = new Mock<IKustoAdmin>();

        adminService.Setup(s => s.GetDatabaseSchema()).ReturnsAsync(clusterSchema);
        adminService.Setup(s => s.GetDatabaseIngestionMappings()).ReturnsAsync(ingestionMappings);
        adminService.Setup(s => s.GetIngestionTimePolicies()).ReturnsAsync(ingestionTimePolicies);
        
        var queryService = new Mock<IKustoQuery>();

        var table1DataBuilder = new StringBuilder();
        table1DataBuilder.AppendLine(".ingest inline into table table1_temp <|");
        table1DataBuilder.AppendLine("\"row1\",2022-10-21T19:21:23.000000");
        table1DataBuilder.AppendLine("\"row2\",2022-10-21T19:22:27.000000");

        queryService.Setup(q => q.TableDataToCslString(
                It.Is<TableSchema>(ts => ts.Name == dbSchema.Tables["table1"].Name),
                It.IsAny<string>()))
            .ReturnsAsync(table1DataBuilder.ToString());

        return (adminService, queryService);
    }
}