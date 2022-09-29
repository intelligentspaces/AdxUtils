using AdxUtils.Options;

namespace AdxUtils.Spark;

public static class NotebookGenerator
{
    private const string TemplateFolder = "templates";

    public static async Task GenerateNotebook(NotebookOptions options)
    {
        var assembly = typeof(NotebookGenerator).Assembly;
        var namespaceName = typeof(NotebookGenerator).Namespace;

        var language = Enum.GetName(typeof(LanguageType), options.Language);
        var service = Enum.GetName(typeof(ServiceType), options.Service);

        var templateName = $"{namespaceName}.{TemplateFolder}.{language!.ToLower()}-{service!.ToLower()}.txt";

        await using var stream = assembly.GetManifestResourceStream(templateName);
        if (stream == null)
        {
            throw new Exception("Couldn't find template");
        }
        
        using var reader = new StreamReader(stream);
        var templateContent = await reader.ReadToEndAsync();

        templateContent = templateContent.Replace("{{cluster-id}}", options.Endpoint, StringComparison.Ordinal)
            .Replace("{{database}}", options.DatabaseName);

        Console.WriteLine(templateContent);
    }
}