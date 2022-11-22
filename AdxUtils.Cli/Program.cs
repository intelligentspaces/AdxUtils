using AdxUtils.Export;
using AdxUtils.Options;
using CommandLine;
using Kusto.Data.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;

namespace AdxUtils.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var result = await Parser.Default.ParseArguments<ExportOptions, ExportToSqlOptions>(args)
                .MapResult(
                    (ExportOptions opts) =>
                    {
                        opts.ValidateExportOptions();
                        return RunExportAndReturnCode(opts);
                    },
                    (ExportToSqlOptions opts) =>
                    {
                        opts.ValidateExportOptions();
                        return RunSqlExportAndReturnCode(opts);
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
                services.AddScoped<OutputToSqlite>();
            })
            .Build();

        var servicesScope = host.Services.CreateScope();
        return servicesScope.ServiceProvider;
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

    private static async Task<int> RunSqlExportAndReturnCode(ExportToSqlOptions options)
    {
        var provider = BuildServiceProvider(options);

        if (options.OutputFile.Exists)
        {
            options.OutputFile.Delete();
        }

        var exporter = provider.GetRequiredService<OutputToSqlite>();
        await exporter.ToSqliteDb(options);

        return 0;
    }
}