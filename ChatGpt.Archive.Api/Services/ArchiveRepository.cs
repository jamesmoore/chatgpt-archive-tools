using ChatGPTExport.Models;
using Microsoft.Data.Sqlite;
using System.IO.Abstractions;
using System.Text.Json;

namespace ChatGpt.Archive.Api.Services
{
    public class ArchiveRepository : IArchiveRepository
    {
        private const string DatabaseFileName = "archive.db";
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

            _connectionString = $"Data Source={Path.Combine(options.DataDirectory, DatabaseFileName)}";
        }

        public void EnsureSchema()
        {
            var dbPath = Path.Combine(_options.DataDirectory, DatabaseFileName);
            
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
            var dbPath = Path.Combine(_options.DataDirectory, DatabaseFileName);
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

            using var transaction = connection.BeginTransaction();
            using var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT OR REPLACE INTO conversations (id, title, update_time, raw_json)
                VALUES (@id, @title, @update_time, @raw_json);";

            var idParam = insertCommand.CreateParameter();
            idParam.ParameterName = "@id";
            insertCommand.Parameters.Add(idParam);

            var titleParam = insertCommand.CreateParameter();
            titleParam.ParameterName = "@title";
            insertCommand.Parameters.Add(titleParam);

            var updateTimeParam = insertCommand.CreateParameter();
            updateTimeParam.ParameterName = "@update_time";
            insertCommand.Parameters.Add(updateTimeParam);

            var rawJsonParam = insertCommand.CreateParameter();
            rawJsonParam.ParameterName = "@raw_json";
            insertCommand.Parameters.Add(rawJsonParam);

            foreach (var conversation in conversations)
            {
                var json = JsonSerializer.Serialize(conversation);
                var updateTime = (long)conversation.update_time;

                idParam.Value = conversation.id ?? string.Empty;
                titleParam.Value = conversation.title ?? string.Empty;
                updateTimeParam.Value = updateTime;
                rawJsonParam.Value = json;

                insertCommand.ExecuteNonQuery();
            }

            transaction.Commit();
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
