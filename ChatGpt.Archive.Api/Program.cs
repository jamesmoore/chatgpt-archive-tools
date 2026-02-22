using ChatGpt.Archive.Api;
using ChatGpt.Archive.Api.Database;
using ChatGpt.Archive.Api.Services;
using ChatGPTExport;
using ChatGPTExport.Assets;
using ChatGPTExport.Decoders;
using Microsoft.AspNetCore.HttpOverrides;
using System.CommandLine;
using System.IO.Abstractions;

const string DefaultDataDirectory = "chatgpt-archive";
const int BrowserOpenDelayMs = 1000;

var builder = WebApplication.CreateBuilder(args);

var sourceOption = new Option<string[]>("--source", "-s")
{
    Arity = ArgumentArity.ZeroOrMore,
    Description = "One or more source directories",
    DefaultValueFactory = (argumentResult) => []
};

var dataDirectoryOption = new Option<string?>("--data-directory", "-d")
{
    Arity = ArgumentArity.ZeroOrOne,
    Description = "Data directory for SQLite database",
    DefaultValueFactory = (argumentResult) => null
};

var rootCommand = new RootCommand
{
    sourceOption,
    dataDirectoryOption
};

var parseResult = rootCommand.Parse(args);
var cliSources = parseResult.GetValue(sourceOption);
var cliDataDirectory = parseResult.GetValue(dataDirectoryOption);

var envSources = Environment.GetEnvironmentVariable("SOURCE")
    ?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

var selectedSources = (cliSources is { Length: > 0 })
    ? cliSources
    : envSources;

var envDataDirectory = Environment.GetEnvironmentVariable("DATA_DIRECTORY");

var dataDirectories = new List<string?>()
{
    cliDataDirectory,
    envDataDirectory,
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DefaultDataDirectory),
};

var selectedDataDirectory = dataDirectories.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p));

// Create ArchiveSourcesOptions directly from command line/env vars
var archiveSourcesOptions = new ArchiveSourcesOptions
{
    SourceDirectories = selectedSources?.ToList() ?? [],
    DataDirectory = selectedDataDirectory!
};

// Add services to the container.
builder.Services.AddSingleton(archiveSourcesOptions);
builder.Services.AddSingleton<DatabaseConfiguration>();

builder.Services.AddControllers();
builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddSingleton<ConversationFinder>();
builder.Services.AddSingleton<FTS5Escaper>();
builder.Services.AddSingleton<ISchemaInitializer, SqliteSchemaInitializer>();
builder.Services.AddSingleton<IArchiveRepository, ArchiveRepository>();
builder.Services.AddSingleton<IConversationsService, ConversationsService>();
builder.Services.AddSingleton<IConversationAssetsCache, ConversationAssetsCache>();
builder.Services.AddSingleton<IAssetLocator, ApiAssetLocator>();
builder.Services.AddSingleton<IMarkdownAssetRenderer, MarkdownAssetRenderer>();
builder.Services.AddSingleton<ConversationFormatterFactory>();
builder.Services.AddSingleton<AssetsCache>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Validate that at least one source directory is configured
var options = app.Services.GetRequiredService<ArchiveSourcesOptions>();
if (options.SourceDirectories.Count == 0)
{
    Console.Error.WriteLine("At least one source directory must be configured");
    return;
}

Console.WriteLine("Using data directory: " + options.DataDirectory);
Console.WriteLine("Using source directories: ");
var fileSystem = app.Services.GetRequiredService<IFileSystem>();
var directories = options.SourceDirectories.Select(p => new { Directory = p, Exists = fileSystem.Directory.Exists(p) }).ToList();

foreach (var sd in directories)
{
    Console.WriteLine("\t" + sd.Directory + "\t" + (sd.Exists ? "Exists" : "Missing"));
}

if (directories.All(p => p.Exists == false))
{
    Console.Error.WriteLine("No source directories exist.");
    return;
}

// Ensure data directory exists before initializing schema
var databaseConfig = app.Services.GetRequiredService<DatabaseConfiguration>();
databaseConfig.EnsureDataDirectoryExists(fileSystem);

// Initialize database schema
app.Services.GetRequiredService<ISchemaInitializer>().EnsureSchema();

// Initialize conversations.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Fallback for React Router (must be after MapControllers)
app.MapFallbackToFile("index.html");

// Open browser only if NO_OPEN_BROWSER is not set
var noOpenBrowser = Environment.GetEnvironmentVariable("NO_OPEN_BROWSER");
if (string.IsNullOrEmpty(noOpenBrowser) || !noOpenBrowser.Equals("true", StringComparison.OrdinalIgnoreCase))
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        // Get the HTTP URL from configured addresses
        var addresses = app.Urls;
        var httpUrl = addresses.FirstOrDefault(url => url.StartsWith("http://", StringComparison.OrdinalIgnoreCase));
        
        if (!string.IsNullOrEmpty(httpUrl))
        {
            // Open browser after a short delay to ensure server is ready
            _ = Task.Run(async () =>
            {
                await Task.Delay(BrowserOpenDelayMs);
                try
                {
                    var url = httpUrl.Replace("0.0.0.0", "localhost").Replace("+", "localhost");
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                    Console.WriteLine($"Opening browser at: {url}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to open browser: {ex.Message}");
                }
            });
        }
    });
}

app.Run();
