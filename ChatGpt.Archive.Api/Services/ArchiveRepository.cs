using ChatGPTExport.Models;
using Microsoft.Data.Sqlite;
using System.IO.Abstractions;
using System.Text.Json;

namespace ChatGpt.Archive.Api.Services
{
    public class ArchiveRepository : IArchiveRepository
    {
        private readonly string _connectionString;
        private readonly IFileSystem _fileSystem;
        private readonly ArchiveSourcesOptions _options;

        public ArchiveRepository(IFileSystem fileSystem, ArchiveSourcesOptions options)
        {
            _fileSystem = fileSystem;
            _options = options;

            if (string.IsNullOrWhiteSpace(options.DataDirectory))
            {
                throw new ArgumentException("DataDirectory must be configured", nameof(options));
            }

            _connectionString = $"Data Source={Path.Combine(options.DataDirectory, "archive.db")}";
        }

        public void EnsureSchema()
        {
            var dbPath = Path.Combine(_options.DataDirectory, "archive.db");
            
            // Ensure data directory exists
            if (!_fileSystem.Directory.Exists(_options.DataDirectory))
            {
                _fileSystem.Directory.CreateDirectory(_options.DataDirectory);
            }

            // Create database and schema
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var createCommand = connection.CreateCommand();
            createCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS conversations (
                    id TEXT PRIMARY KEY,
                    title TEXT,
                    update_time INTEGER,
                    raw_json TEXT NOT NULL
                );";
            createCommand.ExecuteNonQuery();
        }

        public bool HasConversations()
        {
            var dbPath = Path.Combine(_options.DataDirectory, "archive.db");
            if (!_fileSystem.File.Exists(dbPath))
            {
                return false;
            }

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM conversations";

            var count = (long)command.ExecuteScalar()!;
            return count > 0;
        }

        public void InsertConversations(IEnumerable<Conversation> conversations)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

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

        public IEnumerable<Conversation> GetAll()
        {
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

        public Conversation? GetById(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT raw_json FROM conversations WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);

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
