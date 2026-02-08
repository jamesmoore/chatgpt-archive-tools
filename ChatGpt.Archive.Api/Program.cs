using ChatGpt.Archive.Api;
using ChatGpt.Archive.Api.Services;
using ChatGPTExport;
using Microsoft.AspNetCore.HttpOverrides;
using System.CommandLine;
using System.IO.Abstractions;

var builder = WebApplication.CreateBuilder(args);

var sourceOption = new Option<string[]>("--source", "-s")
{
    Arity = ArgumentArity.ZeroOrMore,
    Description = "One or more source directories",
    DefaultValueFactory = (argumentResult) => []
};

var rootCommand = new RootCommand
{
    sourceOption
};

var parseResult = rootCommand.Parse(args);
var cliSources = parseResult.GetValue(sourceOption);

var envSources = Environment.GetEnvironmentVariable("SOURCE")
    ?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

var selectedSources = (cliSources is { Length: > 0 })
    ? cliSources
    : envSources;

// Create ArchiveSourcesOptions directly from command line/env vars
var archiveSourcesOptions = new ArchiveSourcesOptions
{
    SourceDirectories = selectedSources?.ToList() ?? []
};

// Add services to the container.
builder.Services.AddSingleton(archiveSourcesOptions);

builder.Services.AddControllers();
builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddSingleton<IConversationsService, ConversationsService>();
builder.Services.AddSingleton<IConversationAssetsCache, ConversationAssetsCache>();
builder.Services.AddSingleton<ApiAssetLocator>();
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

// Initialize conversations.
app.Services.GetRequiredService<IConversationsService>().GetLatestConversations();

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
