using ChatGpt.Archive.Api;
using ChatGpt.Archive.Api.Database;
using ChatGpt.Archive.Api.Services;
using ChatGPTExport;
using ChatGPTExport.Assets;
using ChatGPTExport.Formatters.Markdown;
using Microsoft.AspNetCore.HttpOverrides;
using System.CommandLine;
using System.IO.Abstractions;

const string DefaultDataDirectory = "chatgpt-archive";

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

builder.Services.AddControllers();
builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddSingleton<ConversationFinder>();
builder.Services.AddSingleton<ISchemaInitializer, SqliteSchemaInitializer>();
builder.Services.AddSingleton<IArchiveRepository, ArchiveRepository>();
builder.Services.AddSingleton<IConversationsService, ConversationsService>();
builder.Services.AddSingleton<IConversationAssetsCache, ConversationAssetsCache>();
builder.Services.AddSingleton<IAssetLocator, ApiAssetLocator>();
builder.Services.AddSingleton<IMarkdownAssetRenderer, MarkdownAssetRenderer>();
builder.Services.AddSingleton<ConversationFormatterFactory>();
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

app.Run();
