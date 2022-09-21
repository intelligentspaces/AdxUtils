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

    [Fact] public void WhenOptionsIncludeEmptyStrings_WhenPropertyIsCalled_ThenTheInvalidDataIsIgnored()
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
}