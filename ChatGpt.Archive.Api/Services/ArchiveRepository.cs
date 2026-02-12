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

            _connectionString = $"Data Source={GetDatabasePath()}";
        }

        private string GetDatabasePath() => Path.Combine(_options.DataDirectory, DatabaseFileName);

        public void EnsureSchema()
        {
            var dbPath = GetDatabasePath();
            
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

        public bool HasConversations()
        {
            var dbPath = GetDatabasePath();
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
            // Create PlaintextExtractor for this call (not thread-safe)
            var plaintextExtractor = new PlaintextExtractor();
            
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            
            // Insert conversations
            using var insertConversationCommand = connection.CreateCommand();
            insertConversationCommand.CommandText = @"
                INSERT OR REPLACE INTO conversations (id, title, create_time, update_time, gizmo_id, raw_json)
                VALUES (@id, @title, @create_time, @update_time, @gizmo_id, @raw_json);";

            var idParam = insertConversationCommand.CreateParameter();
            idParam.ParameterName = "@id";
            insertConversationCommand.Parameters.Add(idParam);

            var titleParam = insertConversationCommand.CreateParameter();
            titleParam.ParameterName = "@title";
            insertConversationCommand.Parameters.Add(titleParam);

            var createTimeParam = insertConversationCommand.CreateParameter();
            createTimeParam.ParameterName = "@create_time";
            insertConversationCommand.Parameters.Add(createTimeParam);

            var updateTimeParam = insertConversationCommand.CreateParameter();
            updateTimeParam.ParameterName = "@update_time";
            insertConversationCommand.Parameters.Add(updateTimeParam);

            var gizmoIdParam = insertConversationCommand.CreateParameter();
            gizmoIdParam.ParameterName = "@gizmo_id";
            insertConversationCommand.Parameters.Add(gizmoIdParam);

            var rawJsonParam = insertConversationCommand.CreateParameter();
            rawJsonParam.ParameterName = "@raw_json";
            insertConversationCommand.Parameters.Add(rawJsonParam);

            foreach (var conversation in conversations)
            {
                var json = JsonSerializer.Serialize(conversation);
                var createTime = (long)conversation.create_time;
                var updateTime = (long)conversation.update_time;

                idParam.Value = conversation.id ?? string.Empty;
                titleParam.Value = conversation.title ?? string.Empty;
                createTimeParam.Value = createTime;
                updateTimeParam.Value = updateTime;
                gizmoIdParam.Value = conversation.gizmo_id ?? (object)DBNull.Value;
                rawJsonParam.Value = json;

                insertConversationCommand.ExecuteNonQuery();
            }

            // Insert messages
            using var insertMessageCommand = connection.CreateCommand();
            insertMessageCommand.CommandText = @"
                INSERT OR REPLACE INTO messages (id, conversation_id, role, content, create_time)
                VALUES (@id, @conversation_id, @role, @content, @create_time);";

            var msgIdParam = insertMessageCommand.CreateParameter();
            msgIdParam.ParameterName = "@id";
            insertMessageCommand.Parameters.Add(msgIdParam);

            var msgConvIdParam = insertMessageCommand.CreateParameter();
            msgConvIdParam.ParameterName = "@conversation_id";
            insertMessageCommand.Parameters.Add(msgConvIdParam);

            var msgRoleParam = insertMessageCommand.CreateParameter();
            msgRoleParam.ParameterName = "@role";
            insertMessageCommand.Parameters.Add(msgRoleParam);

            var msgContentParam = insertMessageCommand.CreateParameter();
            msgContentParam.ParameterName = "@content";
            insertMessageCommand.Parameters.Add(msgContentParam);

            var msgCreateTimeParam = insertMessageCommand.CreateParameter();
            msgCreateTimeParam.ParameterName = "@create_time";
            insertMessageCommand.Parameters.Add(msgCreateTimeParam);

            foreach (var conversation in conversations)
            {
                if (conversation.mapping == null)
                    continue;

                var conversationId = conversation.id ?? string.Empty;

                foreach (var messageContainerEntry in conversation.mapping)
                {
                    var messageContainer = messageContainerEntry.Value;
                    var message = messageContainer.message;

                    if (message == null)
                        continue;

                    var messageId = message.id ?? messageContainer.id ?? string.Empty;
                    var role = message.author?.role ?? string.Empty;
                    var plaintext = plaintextExtractor.ExtractPlaintext(message);
                    var messageCreateTime = message.create_time.HasValue ? (long)message.create_time.Value : 0;

                    msgIdParam.Value = messageId;
                    msgConvIdParam.Value = conversationId;
                    msgRoleParam.Value = role;
                    msgContentParam.Value = plaintext;
                    msgCreateTimeParam.Value = messageCreateTime;

                    insertMessageCommand.ExecuteNonQuery();
                }
            }

            transaction.Commit();
        }

        public IEnumerable<Conversation> GetAll()
        {
            var conversations = new List<Conversation>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT id, title, create_time, update_time, gizmo_id
                FROM conversations 
                ORDER BY update_time DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var conversation = new Conversation
                {
                    id = reader.GetString(0),
                    title = reader.GetString(1),
                    create_time = reader.GetInt64(2),
                    update_time = reader.GetInt64(3),
                    gizmo_id = reader.IsDBNull(4) ? null : reader.GetString(4)
                };
                conversations.Add(conversation);
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

        public IEnumerable<SearchResult> Search(string query)
        {
            var results = new List<SearchResult>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT
                    m.conversation_id,
                    c.title,
                    m.id AS message_id,
                    snippet(messages_fts, 0, '<b>', '</b>', '...', 20) AS snippet
                FROM messages_fts
                JOIN messages m ON m.rowid = messages_fts.rowid
                JOIN conversations c ON c.id = m.conversation_id
                WHERE messages_fts MATCH @query
                ORDER BY rank
                LIMIT 50;";
            command.Parameters.AddWithValue("@query", query);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var result = new SearchResult
                {
                    ConversationId = reader.GetString(0),
                    ConversationTitle = reader.GetString(1),
                    MessageId = reader.GetString(2),
                    Snippet = reader.GetString(3)
                };
                results.Add(result);
            }

            return results;
        }
    }
}
