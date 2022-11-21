using System.Text.RegularExpressions;
using Kusto.Data;
using Kusto.Data.Common;
using Newtonsoft.Json.Linq;

namespace AdxUtils.Export;

/// <summary>
/// Defines extension methods for the <see cref="PolicyShowCommandResult"/> type.
/// </summary>
public static class PolicyShowCommandResultExtensions
{
    /// <summary>
    /// Defines a pattern for extracting parts from a name.
    /// </summary>
    private static readonly Regex EntityPartsPattern = new(@"\['?(?<name>.+?)'?\]", RegexOptions.Compiled);

    /// <summary>
    /// Gets the database name, if available, from the command result.
    /// </summary>
    /// <param name="result">The <see cref="PolicyShowCommandResult"/> instance.</param>
    /// <returns>The database name of the command, or an empty string if one is not available.</returns>
    public static string DatabaseName(this PolicyShowCommandResult result)
    {
        var matches = EntityPartsPattern.Matches(result.EntityName);
        return matches.Count == 2
            ? matches[0].Groups["name"].Value
            : string.Empty;
    }

    /// <summary>
    /// Gets the table name from the command result.
    /// </summary>
    /// <param name="result">The <see cref="PolicyShowCommandResult"/> instance.</param>
    /// <returns>The table name from the result.</returns>
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

    /// <summary>
    /// Gets a flag value indicating if the command is enabled or not.
    /// </summary>
    /// <param name="result">The <see cref="PolicyShowCommandResult"/> instance.</param>
    /// <returns>True if the command is enabled, otherwise false.</returns>
    public static bool IsEnabled(this PolicyShowCommandResult result)
    {
        try
        {
            var parsed = JObject.Parse(result.Policy);
            return parsed.ContainsKey("IsEnabled") && Convert.ToBoolean(parsed["IsEnabled"]!.Value<string>());
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Outputs the <see cref="PolicyShowCommandResult"/> instance as a CSL string.
    /// </summary>
    /// <param name="result">The <see cref="PolicyShowCommandResult"/> instance.</param>
    /// <returns>The current instance as a CSL string.</returns>
    public static string ToCslString(this PolicyShowCommandResult result)
    {
        return CslCommandGenerator.GenerateIngestionTimePolicyAlterCommand(result.TableName(), result.IsEnabled());
    }
}