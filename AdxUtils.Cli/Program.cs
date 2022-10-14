using System.Text.RegularExpressions;
using AdxUtils.Export;
using AdxUtils.Options;
using CommandLine;
using CommandLine.Text;
using Kusto.Data.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AdxUtils.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var parser = new Parser(with => { with.CaseInsensitiveEnumValues = true; });

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
                        opts.Validate();
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

    private static IServiceProvider BuildServiceProvider(IAuthenticationOptions options)
    {
        var kustoConnectionStringBuilder = Authentication.GetConnectionStringBuilder(options);
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(KustoClientFactory.CreateCslAdminProvider(kustoConnectionStringBuilder));
                services.AddSingleton(KustoClientFactory.CreateCslQueryProvider(kustoConnectionStringBuilder));
                services.AddScoped<IKustoAdmin, KustoAdmin>();
                services.AddScoped<IKustoQuery, KustoQuery>();
                services.AddScoped<DatabaseExporter>();
                services.AddScoped<NotebookGenerator>();
            })
            .Build();

        var servicesScope = host.Services.CreateScope();
        return servicesScope.ServiceProvider;
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
        var provider = BuildServiceProvider(options);

        var scriptName = $"{Regex.Replace(options.DatabaseName.ToLower(), "\\s+", "_")}.csl";

        FileInfo outputFilePath;

        try
        {
            outputFilePath = new FileInfo(Path.Join(options.OutputDirectory.FullName, scriptName));
        }
        catch (Exception ex)
        {
            throw new ArgumentValidationException("Unable to process output location.", ex);
        }

        if (outputFilePath.Exists) outputFilePath.Delete();

        await using var stream = outputFilePath.OpenWrite();

        var exporter = provider.GetRequiredService<DatabaseExporter>();

        await exporter.ToCslStreamAsync(options, stream);
        Console.WriteLine($"Script written to: {outputFilePath.FullName}");

        return 0;
    }

    private static async Task<int> RunNotebookGenerationAndReturnCode(NotebookOptions options)
    {
        var provider = BuildServiceProvider(options);
        var exporter = provider.GetRequiredService<NotebookGenerator>();

        var notebookName = string.IsNullOrEmpty(options.Name) 
            ? "adx-query" 
            : Regex.Replace(options.Name, "\\s", "-");

        var scriptName = $"{notebookName}.{exporter.GetFileExtension(options)}";

        FileInfo outputFilePath;

        try
        {
            outputFilePath = new FileInfo(Path.Join(options.OutputDirectory.FullName, scriptName));
        }
        catch (Exception ex)
        {
            throw new ArgumentValidationException("Unable to process output location.", ex);
        }

        if (outputFilePath.Exists) outputFilePath.Delete();

        await using var stream = outputFilePath.OpenWrite();
        
        await exporter.GenerateNotebook(options, stream);
        Console.WriteLine($"Script written to: {outputFilePath.FullName}");

        return 0;
    }
}