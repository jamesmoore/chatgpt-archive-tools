using Microsoft.Data.Sqlite;
using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Database
{
    public class SqliteSchemaInitializer : ISchemaInitializer
    {
        private const string DatabaseFileName = "archive.db";
        private readonly string _connectionString;
        private readonly IFileSystem _fileSystem;
        private readonly ArchiveSourcesOptions _options;

        public SqliteSchemaInitializer(IFileSystem fileSystem, ArchiveSourcesOptions options)
        {
            _fileSystem = fileSystem;
            _options = options;

            if (string.IsNullOrWhiteSpace(options.DataDirectory))
            {
                throw new ArgumentException("DataDirectory must be configured", nameof(options));
            }

            _connectionString = $"Data Source={GetDatabasePath()}";
        }

        private string GetDatabasePath() => Path.Combine(_options.DataDirectory, DatabaseFileName);

        public void EnsureSchema()
        {
            if (!_fileSystem.Directory.Exists(_options.DataDirectory))
            {
                _fileSystem.Directory.CreateDirectory(_options.DataDirectory);
            }

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var createCommand = connection.CreateCommand();
            createCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS conversations (
                    id TEXT PRIMARY KEY,
                    title TEXT,
                    create_time INTEGER,
                    update_time INTEGER,
                    gizmo_id TEXT,
                    raw_json TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_conversations_update_time
                    ON conversations(update_time DESC);

                CREATE TABLE IF NOT EXISTS messages (
                    id TEXT PRIMARY KEY,
                    conversation_id TEXT NOT NULL,
                    role TEXT,
                    content TEXT,
                    create_time INTEGER,
                    FOREIGN KEY(conversation_id)
                        REFERENCES conversations(id)
                        ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS idx_messages_conversation
                    ON messages(conversation_id);

                CREATE VIRTUAL TABLE IF NOT EXISTS messages_fts
                    USING fts5(content, content='messages', content_rowid='rowid');

                -- triggers to keep the FTS index up to date
                CREATE TRIGGER IF NOT EXISTS messages_ai AFTER INSERT ON messages BEGIN
                    INSERT INTO messages_fts(rowid, content)
                    VALUES (new.rowid, new.content);
                END;

                CREATE TRIGGER IF NOT EXISTS messages_ad AFTER DELETE ON messages BEGIN
                    INSERT INTO messages_fts(messages_fts, rowid, content)
                    VALUES('delete', old.rowid, old.content);
                END;

                CREATE TRIGGER IF NOT EXISTS messages_au AFTER UPDATE ON messages BEGIN
                    INSERT INTO messages_fts(messages_fts, rowid, content)
                    VALUES('delete', old.rowid, old.content);
                    INSERT INTO messages_fts(rowid, content)
                    VALUES (new.rowid, new.content);
                END;";
            createCommand.ExecuteNonQuery();
        }
    }
}
