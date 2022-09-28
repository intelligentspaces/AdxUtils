namespace AdxUtils.Export.Tests;

public class PolicyShowCommandResultExtensionsTests
{
    [Theory]
    [InlineData("[db01].[table1]", "db01")]
    [InlineData("[table1]", "")]
    [InlineData("['db 01'].['table 1']", "db 01")]
    [InlineData("[db01].[table1].[broken]", "")]
    public void WhenGettingDatabaseName_CallingMethod_ReturnsValueForDatabaseName(string input, string expected)
    {
        var pscResult = new PolicyShowCommandResult
        {
            EntityName = input
        };

        var result = pscResult.DatabaseName();

        result.Should().Be(expected);
    }
    
    [Theory]
    [InlineData("[db01].[table1]", "table1")]
    [InlineData("[table1]", "table1")]
    [InlineData("['db 01'].['table 1']", "table 1")]
    [InlineData("[db01].[table1].[broken]", "")]
    public void WhenGettingTableName_CallingMethod_ReturnsValueForTableName(string input, string expected)
    {
        var pscResult = new PolicyShowCommandResult
        {
            EntityName = input
        };

        var result = pscResult.TableName();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("{\"IsEnabled\": true }", true)]
    [InlineData("{\"IsEnabled\": \"true\" }", true)]
    [InlineData("{\"IsEnabled\": \"True\" }", true)]
    [InlineData("{\"IsEnabled\": True }", false)]
    [InlineData("{\"IsEnabled\": false }", false)]
    [InlineData("{\"IsActive\": true }", false)]
    [InlineData("", false)]
    [InlineData("{\"Item1\": { \"value\": 1 } }", false)]
    public void WhenGettingEnabledStatus_CallingMethod_CorrectlyInspectsPolicyData(string policy, bool expected)
    {
        var pscResult = new PolicyShowCommandResult
        {
            Policy = policy
        };

        var result = pscResult.IsEnabled();

        result.Should().Be(expected);
    }

    [Fact]
    public void WhenPolicyShowCommandResultAvailable_CallingCslStringMethod_ReturnsValidCommand()
    {
        const string expected = ".alter table ['table 1'] policy ingestiontime true";

        var pscResult = new PolicyShowCommandResult
        {
            EntityName = "['db 01'].['table 1']",
            Policy = "{\"IsEnabled\": true }"
        };

        var command = pscResult.ToCslString();

        command.Should().BeEquivalentTo(expected);
    }
}