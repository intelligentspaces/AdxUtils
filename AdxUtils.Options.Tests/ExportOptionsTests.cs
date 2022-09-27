namespace AdxUtils.Options.Tests;

public class ExportOptionsTests
{
    [Fact]
    public void WhenOptionsIncludeRenamePairs_WhenPropertyIsCalled_ThenValueIsConvertedToPairs()
    {
        var expected = new List<(string, string)>
        {
            ("table1", "table2"),
            ("table3", "table4")
        };
        
        var options = new ExportOptions
        {
            RenamedTables = new []{ "table1=table2", "table3=table4" }
        };

        var parsedPairs = options.RenamedTablePairs;

        parsedPairs.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void WhenOptionsIncludeInvalidRenamePairs_WhenPropertyIsCalled_ThenInvalidValuesAreIgnored()
    {
        var expected = new List<(string, string)>
        {
            ("table1", "table2")
        };

        var options = new ExportOptions
        {
            RenamedTables = new[] { "table1=table2", "table3->table4" }
        };

        var parsedPairs = options.RenamedTablePairs;

        parsedPairs.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void WhenOptionsIncludeMultipleInplaceRenamePairs_WhenPropertyIsCalled_ThenTheAdditionalTableIsIgnored()
    {
        var expected = new List<(string, string)>
        {
            ("table1", "table2"),
            ("table3", "table4")
        };

        var options = new ExportOptions
        {
            RenamedTables = new[] { "table1=table2", "table3=table4=table5" }
        };

        var parsedPairs = options.RenamedTablePairs;

        parsedPairs.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void WhenOptionsIncludeEmptyStrings_WhenPropertyIsCalled_ThenTheInvalidDataIsIgnored()
    {
        var expected = new List<(string, string)>
        {
            ("table1", "table2"),
            ("table3", "table4")
        };

        var options = new ExportOptions
        {
            RenamedTables = new[] { "table1=table2", "table3=table4", "table5=", "=", "=table6", "" }
        };

        var parsedPairs = options.RenamedTablePairs;

        parsedPairs.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void WhenOptionsIncludeNullRename_WhenPropertyIsCalled_ThenAnEmptyArrayisReturned()
    {
        var options = new ExportOptions
        {
            RenamedTables = null
        };

        var parsedPairs = options.RenamedTablePairs;

        parsedPairs.Should().BeEmpty();
    }

    [Fact]
    public void WhenOptionsIncludeEmptyRename_WhenPropertyIsCalled_ThenAnEmptyArrayisReturned()
    {
        var options = new ExportOptions
        {
            RenamedTables = Array.Empty<string>()
        };

        var parsedPairs = options.RenamedTablePairs;

        parsedPairs.Should().BeEmpty();
    }

    [Theory]
    [InlineData(new[] {"table1"}, new[] { "table1" })]
    [InlineData(null, new string[0])]
    [InlineData(new string[0], new string[0])]
    public void WhenOptionsIncludeIgnoredTables_WhenAsArrayPropertyIsCalled_ThenDataIsParsedCorrectly(string[] input, string[] expected)
    {
        var options = new ExportOptions
        {
            IgnoredTables = input
        };

        var parsedIgnored = options.IgnoredTablesArray;

        parsedIgnored.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(new[] {"table1"}, new[] { "table1" })]
    [InlineData(null, new string[0])]
    [InlineData(new string[0], new string[0])]
    public void WhenOptionsIncludeExportDataTables_WhenAsArrayPropertyIsCalled_ThenDataIsParsedCorrectly(string[] input, string[] expected)
    {
        var options = new ExportOptions
        {
            ExportedDataTables = input
        };

        var parsedIgnored = options.ExportedDataTablesArray;

        parsedIgnored.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void WhenOptionsIncludeValidSettings_WhenValidated_ThenSuccessful()
    {
        var options = new ExportOptions
        {
            UseAzureCli = true,
            Endpoint = "https://a.valid.url/"
        };

        var act = () => options.Validate();

        act.Should().NotThrow<ArgumentValidationException>();
    }

    //[Fact]
    //public void WhenOptionsIncludeInvalidUrl_WhenValidated_AnExceptionIsThrown()
    //{
    //    var options = new ExportOptions
    //    {
    //        UseAzureCli = true,
    //        Endpoint = "/my-endpoint"
    //    };

    //    var act = () => options.Validate();

    //    act.Should().Throw<ArgumentValidationException>()
    //        .WithMessage("*cluster should be a valid, absolute, uri*");
    //}

    [Theory]
    [InlineData("client_id", "secret", "")]
    [InlineData("client_id", "secret", "   ")]
    [InlineData("client_id", "secret", null)]
    [InlineData("client_id", "", "authority")]
    [InlineData("client_id", "   ", "authority")]
    [InlineData("client_id", null, "authority")]
    [InlineData("", "secret", "authority")]
    [InlineData("    ", "secret", "authority")]
    [InlineData(null, "secret", "authority")]
    public void WhenOptionsIncludeInvalidCredentials_WhenValidated_AnExceptionIsThrown(string clientId,
        string clientSecret, string authority)
    {
        var options = new ExportOptions
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            Authority = authority,
            Endpoint = "https://valid.url"
        };

        var act = () => options.Validate();

        act.Should().Throw<ArgumentValidationException>()
            .WithMessage("*id, secret, and authority must be specified*");
    }
}