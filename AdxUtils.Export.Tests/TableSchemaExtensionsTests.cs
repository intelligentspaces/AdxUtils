namespace AdxUtils.Export.Tests;

public class TableSchemaExtensionsTests
{
    [Theory]
    [InlineData("table", "db01", "db01.['table']")]
    [InlineData("table", null, "['table']")]
    [InlineData("table", "", "['table']")]
    [InlineData("example table", "test database", "['test database'].['example table']")]
    public void WhenNormalizingTableName_WhenCalled_ThenCorrectNormalizedValueIsReturned(string tableName, string? databaseName, string expected)
    {
        var schema = new TableSchema(tableName);
        var result = schema.NormalizeTableName(databaseName);

        result.Should().Be(expected);
    }

    [Fact]
    public void WhenSetOrReplaceMethodCalled_WithTempTable_ThenCorrectStringReturned()
    {
        const string expected = ".set-or-replace  ['example table'] with(policy_ingestiontime = true, distributed = false) <| temp_table";
        
        var schema = new TableSchema("example table");
        var result = schema.SetOrReplaceTableCslString("temp_table");

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void WhenDropTableMethodCalled_ForSchema_ThenCorrectStringReturned()
    {
        const string expected = ".drop table ['example table'] ifexists";

        var schema = new TableSchema("example table");
        var result = schema.DropTableCslString();

        result.Should().BeEquivalentTo(expected);
    }
}