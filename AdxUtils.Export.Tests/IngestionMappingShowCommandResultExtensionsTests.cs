namespace AdxUtils.Export.Tests
{
    public class IngestionMappingShowCommandResultExtensionsTests
    {
        [Fact]
        public void WhenGeneratingQueryFromMapping_WhenCalled_ThenCorrectQueryIsReturned()
        {
            const string expected = ".create-or-alter table table1 ingestion json mapping \"table1_mapping\" '[{\"column\":\"column1\",\"path\":\"$.column1\",\"datatype\":\"guid\",\"transform\":null}]'";
            var mappings = new IngestionMappingShowCommandResult
            {
                Kind = "json",
                Table = "table1",
                Name = "table1_mapping",
                Mapping = "[{\"column\":\"column1\",\"path\":\"$.column1\",\"datatype\":\"guid\",\"transform\":null}]"
            };

            mappings.ToCslString().Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void WhenGeneratingQueryFromMappingWithSingleQuotes_WhenCalled_ThenCorrectQueryIsReturned()
        {
            const string expected = ".create-or-alter table table1 ingestion json mapping \"table1_mapping\" '[{\"column\":\"serialNumber\",\"path\":\"$[\\'serialNumber\\']\",\"datatype\":\"string\",\"transform\":null}]'";
            var mappings = new IngestionMappingShowCommandResult
            {
                Kind = "json",
                Table = "table1",
                Name = "table1_mapping",
                Mapping = "[{\"column\":\"serialNumber\",\"path\":\"$['serialNumber']\",\"datatype\":\"string\",\"transform\":null}]"
            };

            mappings.ToCslString().Should().BeEquivalentTo(expected);
        }
    }
}
