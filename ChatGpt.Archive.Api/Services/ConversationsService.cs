using ChatGPTExport;
using ChatGPTExport.Assets;
using ChatGPTExport.Models;
using System.IO.Abstractions;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace ChatGpt.Archive.Api.Services
{
    public class ConversationsService : IConversationsService
    {
        private readonly ArchiveSourcesOptions _options;
        private readonly IFileSystem _fileSystem;
        private readonly IConversationAssetsCache _directoryCache;
        private readonly string _connectionString;
        private bool _databaseInitialized = false;
        private readonly object _initLock = new object();

        public ConversationsService(
            IFileSystem fileSystem,
            IConversationAssetsCache directoryCache,
            ArchiveSourcesOptions options)
        {
            _fileSystem = fileSystem;
            _directoryCache = directoryCache;
            _options = options;
            _connectionString = $"Data Source={Path.Combine(options.DataDirectory, "archive.db")}";
        }

        private void EnsureDatabaseInitialized()
        {
            if (_databaseInitialized)
                return;

            lock (_initLock)
            {
                if (_databaseInitialized)
                    return;

                var dbPath = Path.Combine(_options.DataDirectory, "archive.db");
                var dbExists = _fileSystem.File.Exists(dbPath);

                if (!dbExists)
                {
                    // Ensure data directory exists
                    if (!_fileSystem.Directory.Exists(_options.DataDirectory))
                    {
                        _fileSystem.Directory.CreateDirectory(_options.DataDirectory);
                    }

                    // Create database and populate it
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    // Create table
                    using (var createCommand = connection.CreateCommand())
                    {
                        createCommand.CommandText = @"
                            CREATE TABLE IF NOT EXISTS conversations (
                                id TEXT PRIMARY KEY,
                                title TEXT,
                                update_time INTEGER,
                                raw_json TEXT NOT NULL
                            );";
                        createCommand.ExecuteNonQuery();
                    }

                    // Import conversations
                    var conversations = GetConversationsFromSource();
                    foreach (var conversation in conversations)
                    {
                        var json = JsonSerializer.Serialize(conversation);
                        var updateTime = (long)conversation.update_time;

                        using var insertCommand = connection.CreateCommand();
                        insertCommand.CommandText = @"
                            INSERT OR REPLACE INTO conversations (id, title, update_time, raw_json)
                            VALUES (@id, @title, @update_time, @raw_json);";
                        insertCommand.Parameters.AddWithValue("@id", conversation.id ?? string.Empty);
                        insertCommand.Parameters.AddWithValue("@title", conversation.title ?? string.Empty);
                        insertCommand.Parameters.AddWithValue("@update_time", updateTime);
                        insertCommand.Parameters.AddWithValue("@raw_json", json);
                        insertCommand.ExecuteNonQuery();
                    }
                }

                _databaseInitialized = true;
            }
        }

        public IEnumerable<Conversation> GetLatestConversations()
        {
            EnsureDatabaseInitialized();

            var conversations = new List<Conversation>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT raw_json FROM conversations ORDER BY update_time DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var json = reader.GetString(0);
                var conversation = JsonSerializer.Deserialize<Conversation>(json);
                if (conversation != null)
                {
                    conversations.Add(conversation);
                }
            }

            return conversations;
        }

        private IEnumerable<Conversation> GetConversationsFromSource()
        {
            var directories = _options.SourceDirectories.Select(p => _fileSystem.DirectoryInfo.New(p));
            var conversationFinder = new ConversationFinder();
            var conversationFiles = conversationFinder.GetConversationFiles(directories);
            var conversationsParser = new ConversationsParser([]);
            var conversations = conversationFiles.Select(p => new { 
                ParsedConversations = conversationsParser.GetConversations(p), 
                ParentDirectory = p.Directory
            }).ToList();
            var successfulConversations = conversations.Where(p => p.ParsedConversations.Status == ConversationParseResult.Success && p.ParsedConversations.Conversations != null).ToList();

            var parentDirectories = successfulConversations.OrderByDescending(p => p.ParsedConversations.Conversations!.GetUpdateTime()).Select(p => p.ParentDirectory!).ToList();
            _directoryCache.SetConversationAssets(parentDirectories.Select(ConversationAssets.FromDirectory).ToList());
            var latestConversations = successfulConversations.Select(p => p.ParsedConversations.Conversations!).GetLatestConversations();
            return latestConversations;
        }

        public Conversation? GetConversation(string conversationId)
        {
            EnsureDatabaseInitialized();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT raw_json FROM conversations WHERE id = @id";
            command.Parameters.AddWithValue("@id", conversationId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var json = reader.GetString(0);
                return JsonSerializer.Deserialize<Conversation>(json);
            }

            return null;
        }
    }
}
