using AdxUtils.Export;
using AdxUtils.Options;
using CommandLine;

namespace AdxUtils.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var result = await Parser.Default.ParseArguments<ExportOptions>(args)
                .MapResult(
                    (ExportOptions opts) =>
                    {
                        opts.Validate();
                        return RunExportAndReturnCode(opts);
                    },
                    errs => Task.FromResult(1));

            return result;
        }
        catch (ArgumentValidationException ex)
        {
            Console.WriteLine($"Invalid arguments: {ex}");
            return 1;
        }
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
}
