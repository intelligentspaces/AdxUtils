using AdxUtils.Export;
using AdxUtils.Options;
using AdxUtils.Spark;
using CommandLine;
using CommandLine.Text;

namespace AdxUtils.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var parser = new Parser(with =>
            {
                with.CaseInsensitiveEnumValues = true;
            });

            var parserResult = parser.ParseArguments<ExportOptions, NotebookOptions>(args);

            var result = await parserResult
                .MapResult(
                    (ExportOptions opts) =>
                    {
                        opts.Validate();
                        return RunExportAndReturnCode(opts);
                    },
                    (NotebookOptions opts) =>
                    {
                        return RunNotebookGenerationAndReturnCode(opts);
                    },
            errs => DisplayHelp(parserResult, errs)
                );

            return result;
        }
        catch (ArgumentValidationException ex)
        {
            Console.WriteLine($"Invalid arguments: {ex}");
            return 1;
        }
    }

    private static Task<int> DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
    {
        HelpText helpText;
        if (errs.IsVersion())
        {
            helpText = HelpText.AutoBuild(result);
        }
        else
        {
            helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = true;
                h.AddEnumValuesToHelpText = true;
                return h;
            });
        }

        Console.WriteLine(helpText);

        return Task.FromResult(1);
    }

    private static async Task<int> RunExportAndReturnCode(ExportOptions options)
    {
        var outputFilePath = new FileInfo("script.csl");
        
        if (outputFilePath.Exists) outputFilePath.Delete();

        await using var stream = outputFilePath.OpenWrite();

        await DatabaseExporter.ToCslStreamAsync(options, stream);
        Console.WriteLine($"Script written to: {outputFilePath.FullName}");

        return 0;
    }

    private static async Task<int> RunNotebookGenerationAndReturnCode(NotebookOptions options)
    {
        await NotebookGenerator.GenerateNotebook(options);
        
        return 0;
    }
}
