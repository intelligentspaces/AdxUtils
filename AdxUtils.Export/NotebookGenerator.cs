using AdxUtils.Options;

namespace AdxUtils.Export;

public class NotebookGenerator
{
    private const string TemplateFolder = "templates.spark";
    private static readonly char[] NewLineChars = {'\n', '\r'};

    private readonly IKustoQuery _queryService;

    public NotebookGenerator(IKustoQuery queryService)
    {
        _queryService = queryService;
    }

    public async Task GenerateNotebook(NotebookOptions options, Stream stream)
    {
        if (!stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable");
        }

        await using var writer = new StreamWriter(stream);

        var assembly = typeof(NotebookGenerator).Assembly;
        var namespaceName = typeof(NotebookGenerator).Namespace;

        var language = Enum.GetName(typeof(LanguageType), options.Language);
        var service = Enum.GetName(typeof(ServiceType), options.Service);

        var templateName = $"{namespaceName}.{TemplateFolder}.{language!.ToLower()}-{service!.ToLower()}.txt";

        await using var templateStream = assembly.GetManifestResourceStream(templateName);
        if (templateStream == null)
        {
            throw new Exception("Couldn't find template");
        }

        using var reader = new StreamReader(templateStream);
        var templateContent = await reader.ReadToEndAsync();

        var query = await options.GetQuery();
        var (isValid, error) = await _queryService.IsValidQuery(query);

        if (!isValid)
        {
            Console.WriteLine(error);
            return;
        }

        var languageSpecificQuery = options.Language switch
        {
            LanguageType.Scala when NewLineChars.Any(query.Contains) => $"\"\"\"{query}\"\"\"",
            LanguageType.Scala => $"\"{query}\"",
            LanguageType.Python when NewLineChars.Any(query.Contains) => $"'''{query}'''",
            _ => $"'{query}'"
        };

        templateContent = templateContent.Replace("{{cluster-id}}", options.Endpoint, StringComparison.Ordinal)
            .Replace("{{database}}", options.DatabaseName)
            .Replace("{{query}}", languageSpecificQuery);

        await writer.WriteAsync(templateContent);
    }

    public string GetFileExtension(NotebookOptions options)
    {
        return options.Language switch
        {
            LanguageType.Scala => "scala",
            _ => ".py"
        };
    }
}