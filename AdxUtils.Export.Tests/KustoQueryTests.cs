using System.Data;
using System.Data.SqlTypes;
using Newtonsoft.Json.Linq;

namespace AdxUtils.Export.Tests;

public class KustoQueryTests
{
    [Fact]
    public async Task GettingTableData_WhenCalled_ShouldGenerateValidQuery()
    {
        // Configure IDataReader mock
        var dataReader = new Mock<IDataReader>();
        dataReader.Setup(dr => dr.Read()).Returns(false);
        
        // Configure ICslQueryProvider mock
        var provider = new Mock<ICslQueryProvider>();
        provider.SetupGet(p => p.DefaultDatabaseName).Returns("db01");
        
        var generatedQuery = string.Empty;
        ClientRequestProperties? requestProperties = null;
        
        // Setup call to Execute method and capture the request properties and query being executed
        provider.Setup(p =>
                p.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ClientRequestProperties>()))
            .Callback<string, string, ClientRequestProperties>((_, query, properties) =>
            {
                requestProperties = properties;
                generatedQuery = query;
            })
            .ReturnsAsync(dataReader.Object);
        
        // Create a new instance of the class under test
        var query = new KustoQuery(provider.Object);

        // Execute query
        var sourceTable = new TableSchema("table 1");
        var result = await query.TableDataToCslString(sourceTable, "table1_temp");

        // Assert results and generated query with request properties
        result.Should().BeEmpty();
        generatedQuery.Should().BeEquivalentTo("['table 1']");
        requestProperties.Should().NotBeNull();
        requestProperties!.HasOption(ClientRequestProperties.OptionNoTruncation).Should().BeTrue();
        requestProperties.ClientRequestId.Should().StartWith("AdxUtils.Export;");
    }

    [Fact]
    public async Task GettingTableData_WhenCalled_ShouldReturnValueRows()
    {
        // Configure the mock IDataReader to return 2 rows
        var dataReader = new Mock<IDataReader>();
        dataReader.SetupSequence(dr => dr.Read())
            .Returns(true)
            .Returns(true)
            .Returns(false);

        // Set the input row data and the expected generated string output
        var recordData = new List<object[]>
        {
            new object[] {true, (sbyte) 1, new DateTime(2022, 10, 1, 12, 13, 14), Guid.Parse("1b7c2612-e645-4ae8-9a7c-9e4418053460"), 12, 14L, 12.3D, "string1", TimeSpan.FromDays(1), JObject.Parse("{ \"col1\": 1 }"), new SqlDecimal(3.14)},
            new object[] {false, (sbyte) 0, new DateTime(2022, 10, 1, 12, 13, 14), Guid.Parse("1b7c2612-e645-4ae8-9a7c-9e4418053460"), 12, DBNull.Value, 12.3D, "string2", TimeSpan.FromMinutes(1), JArray.Parse("[1, 2, \"a\"]"), new SqlDecimal(3.14)}
        };

        var expectedExportData = new List<string>
        {
            "1,1,2022-10-01T12:13:14.0000000,1b7c2612-e645-4ae8-9a7c-9e4418053460,12,14,12.3,\"string1\",1.00:00:00,\"{\"\"col1\"\":1}\",3.14",
            "0,0,2022-10-01T12:13:14.0000000,1b7c2612-e645-4ae8-9a7c-9e4418053460,12,,12.3,\"string2\",00:01:00,\"[1,2,\"\"a\"\"]\",3.14"
        };

        // Setup the call to GetValues so that each call returns the next row of data
        var index = 0;
        dataReader.Setup(dr => dr.GetValues(It.IsAny<object[]>()))
            .Callback(new Action<object[]>(arr =>
            {
                Array.Copy(recordData[index], 0, arr, 0, arr.Length);
                index++;
            }))
            .Returns(recordData[0].Length);
        
        // Configure the mock query provider
        var provider = new Mock<ICslQueryProvider>();
        provider.SetupGet(p => p.DefaultDatabaseName).Returns("db01");

        // Set up the call to the execute method
        provider.Setup(p =>
                p.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ClientRequestProperties>()))
            .ReturnsAsync(dataReader.Object);
        
        // Create a new instance of the class under test
        var query = new KustoQuery(provider.Object);

        // Set up the table schema to pass to the method
        var columns = new List<ColumnSchema>
        {
            ColumnSchema.FromNameAndCslType("col1", "bool"),
            ColumnSchema.FromNameAndCslType("col2", "bool"),
            ColumnSchema.FromNameAndCslType("col3", "datetime"),
            ColumnSchema.FromNameAndCslType("col4", "guid"),
            ColumnSchema.FromNameAndCslType("col5", "int"),
            ColumnSchema.FromNameAndCslType("col6", "long"),
            ColumnSchema.FromNameAndCslType("col7", "real"),
            ColumnSchema.FromNameAndCslType("col8", "string"),
            ColumnSchema.FromNameAndCslType("col9", "timespan"),
            ColumnSchema.FromNameAndCslType("col10", "dynamic"),
            ColumnSchema.FromNameAndCslType("col11", "decimal"),
        };

        var sourceTable = new TableSchema("table 1", columns);
        const string tempTable = "table1_temp";

        // Execute the method
        var result = await query.TableDataToCslString(sourceTable, tempTable);

        // Assert results
        result.Should().NotBeEmpty()
            .And.StartWith(".ingest inline into table table1_temp <|")
            .And.ContainAll(expectedExportData);
    }
}