using System.Text.RegularExpressions;
using Kusto.Data;
using Kusto.Data.Common;
using Newtonsoft.Json.Linq;

namespace AdxUtils.Export;

public static class PolicyShowCommandResultExtensions
{
    private static readonly Regex EntityPartsPattern = new(@"\['?(?<name>.+?)'?\]", RegexOptions.Compiled);

    public static string DatabaseName(this PolicyShowCommandResult result)
    {
        var matches = EntityPartsPattern.Matches(result.EntityName);
        return matches.Count == 2
            ? matches[0].Groups["name"].Value
            : string.Empty;
    }

    public static string TableName(this PolicyShowCommandResult result)
    {
        var matches = EntityPartsPattern.Matches(result.EntityName);
        return matches.Count switch
        {
            1 => matches[0].Groups["name"].Value,
            2 => matches[1].Groups["name"].Value,
            _ => string.Empty
        };
    }

    public static bool IsEnabled(this PolicyShowCommandResult result)
    {
        var parsed = JObject.Parse(result.Policy);
        return parsed.ContainsKey("IsEnabled") && Convert.ToBoolean(parsed["IsEnabled"]!.Value<string>());
    }

    public static string ToCslString(this PolicyShowCommandResult result)
    {
        return CslCommandGenerator.GenerateIngestionTimePolicyAlterCommand(result.TableName(), result.IsEnabled());
    }
}