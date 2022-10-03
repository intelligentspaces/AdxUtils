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
}