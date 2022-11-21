using System.Data;

namespace AdxUtils.Export.Tests;

public class KustoAdminTests
{
    [Fact]
    public async Task GettingDatabaseSchema_WhenCalled_ReturnsValidSchemaObject()
    {
        var responseContent = await File.ReadAllTextAsync("TestData/SimpleDatabaseSchema.json");
        
        var dataReader = new Mock<IDataReader>();

        dataReader.SetupSequence(dr => dr.Read())
            .Returns(true)
            .Returns(false);

        dataReader.Setup(dr => dr.GetString(It.Is<int>(x => x == 0)))
            .Returns(responseContent);
        
        var provider = new Mock<ICslAdminProvider>();
        provider.SetupGet(p => p.DefaultDatabaseName).Returns("db01");

        var database = string.Empty;
        var generatedQuery = string.Empty;

        provider.Setup(p => p.ExecuteControlCommandAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ClientRequestProperties>()))
            .Callback<string, string, ClientRequestProperties>((db, query, _) =>
            {
                database = db;
                generatedQuery = query;
            })
            .ReturnsAsync(dataReader.Object);

        var client = new KustoAdmin(provider.Object);
        var result = await client.GetDatabaseSchema();

        database.Should().NotBeNull().And.Be("db01");
        generatedQuery.Should().NotBeNull().And.BeEquivalentTo(".show database db01 schema as json");
        
        result.Should().NotBeNull();
        result.Databases.Should().HaveCount(1);
        result.Databases["db01"].Tables.Should().HaveCount(1)
            .And.ContainKey("table01");
        result.Databases["db01"].Functions.Should().HaveCount(1)
            .And.ContainKey("simpleFunc");
    }
    
    [Theory]
    [InlineData("invalid", "Unable to parse response into a schema")]
    [InlineData("", "Unable to load schema for database*")]
    [InlineData("{ \"Databases\": { } }", "Unable to load schema for database*")]
    [InlineData("{ }", "Unable to load schema for database*")]
    public async Task GettingDatabaseSchema_WhenADXReturnsInvalidData_ACustomExceptionIsThrown(string input, string expectedMessage)
    {
        var dataReader = new Mock<IDataReader>();

        dataReader.SetupSequence(dr => dr.Read())
            .Returns(true)
            .Returns(false);

        dataReader.Setup(dr => dr.GetString(It.Is<int>(x => x == 0)))
            .Returns(input);
        
        var provider = new Mock<ICslAdminProvider>();
        provider.SetupGet(p => p.DefaultDatabaseName).Returns("db01");

        provider.Setup(p => p.ExecuteControlCommandAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ClientRequestProperties>()))
            .ReturnsAsync(dataReader.Object);

        var client = new KustoAdmin(provider.Object);
        var act = async () => await client.GetDatabaseSchema();

        await act.Should().ThrowAsync<KustoAdminException>().WithMessage(expectedMessage);
    }

    [Fact]
    public async Task GettingDatabaseIngestionMapping_WhenCalled_ReturnsValidMappingInformation()
    {
        var dataReader = new Mock<IDataReader>();

        dataReader.SetupSequence(dr => dr.Read())
            .Returns(true)
            .Returns(false);

        dataReader.Setup(dr => dr.GetString(It.Is<int>(i => i == 0))).Returns("Mapping1");
        dataReader.Setup(dr => dr.GetString(It.Is<int>(i => i == 1))).Returns("Json");
        dataReader.Setup(dr => dr.GetString(It.Is<int>(i => i == 2))).Returns("[{\"column\":\"Col1\",\"path\":\"$.col1\",\"datatype\":\"\",\"transform\":null}]");
        dataReader.Setup(dr => dr.GetDateTime(It.Is<int>(i => i == 3))).Returns(new DateTime(2022, 10, 1, 11, 12, 13));
        dataReader.Setup(dr => dr.GetString(It.Is<int>(i => i == 4))).Returns("db01");
        dataReader.Setup(dr => dr.GetString(It.Is<int>(i => i == 5))).Returns("table1");
        
        var provider = new Mock<ICslAdminProvider>();
        provider.SetupGet(p => p.DefaultDatabaseName).Returns("db01");

        var generatedQuery = string.Empty;

        provider.Setup(p => p.ExecuteControlCommandAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ClientRequestProperties>()))
            .Callback<string, string, ClientRequestProperties>((_, query, _) =>
            {
                generatedQuery = query;
            })
            .ReturnsAsync(dataReader.Object);

        var client = new KustoAdmin(provider.Object);
        var result = await client.GetDatabaseIngestionMappings();

        var expected = new List<IngestionMappingShowCommandResult>
        {
            new()
            {
                Name = "Mapping1",
                Kind = "Json",
                Mapping = "[{\"column\":\"Col1\",\"path\":\"$.col1\",\"datatype\":\"\",\"transform\":null}]",
                LastUpdatedOn = new DateTime(2022, 10, 1, 11, 12, 13),
                Database = "db01",
                Table = "table1"
            }
        };

        generatedQuery.Should().BeEquivalentTo(".show databases (db01) ingestion mappings with (onlyLatestPerTable=true)");
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GettingDatabaseIngestionTimePolicies_WhenCalled_ReturnsValidPolicyInformation()
    {
        var dataReader = new Mock<IDataReader>();

        dataReader.SetupSequence(dr => dr.Read())
            .Returns(true)
            .Returns(false);

        string? childEntities = null;

        dataReader.Setup(dr => dr.GetString(It.Is<int>(i => i == 0))).Returns("IngestionTimePolicy");
        dataReader.Setup(dr => dr.GetString(It.Is<int>(i => i == 1))).Returns("[db01].[table1]");
        dataReader.Setup(dr => dr.GetString(It.Is<int>(i => i == 2))).Returns("{ \"IsEnabled\": true }");
        dataReader.Setup(dr => dr.GetString(It.Is<int>(i => i == 3))).Returns(childEntities);
        dataReader.Setup(dr => dr.GetString(It.Is<int>(i => i == 4))).Returns("Table");
        
        var provider = new Mock<ICslAdminProvider>();
        provider.SetupGet(p => p.DefaultDatabaseName).Returns("db01");

        var generatedQuery = string.Empty;

        provider.Setup(p => p.ExecuteControlCommandAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ClientRequestProperties>()))
            .Callback<string, string, ClientRequestProperties>((_, query, _) =>
            {
                generatedQuery = query;
            })
            .ReturnsAsync(dataReader.Object);

        var client = new KustoAdmin(provider.Object);
        var result = await client.GetIngestionTimePolicies();

        var expected = new List<PolicyShowCommandResult>
        {
            new()
            {
                PolicyName = "IngestionTimePolicy",
                EntityName = "[db01].[table1]",
                Policy = "{ \"IsEnabled\": true }",
                ChildEntities = null,
                EntityType = "Table"
            }
        };

        generatedQuery.Should().BeEquivalentTo(".show table * policy ingestiontime");
        result.Should().BeEquivalentTo(expected);
        result[0].IsEnabled().Should().BeTrue();
    }
}