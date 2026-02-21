using ChatGpt.Archive.Api.Services;
using ChatGPTExport.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace ChatGpt.Archive.Api.Database
{
    public class ArchiveRepository(
        DatabaseConfiguration databaseConfiguration,
        FTS5Escaper fTS5Escaper
        ) : IArchiveRepository
    {
        public bool HasConversations()
        {
            using var connection = new SqliteConnection(databaseConfiguration.ConnectionString);
            connection.Open();
            var count = connection.ExecuteScalar<long>("SELECT COUNT(*) FROM conversations");
            return count > 0;
        }

        public void ClearAll()
        {
            using var connection = new SqliteConnection(databaseConfiguration.ConnectionString);
            connection.Open();
            connection.Execute("DELETE FROM messages; DELETE FROM conversations;");
        }

        public void InsertConversations(IEnumerable<Conversation> conversations)
        {
            using var connection = new SqliteConnection(databaseConfiguration.ConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            const string conversationSql = @"
                INSERT OR REPLACE INTO conversations (id, title, create_time, update_time, gizmo_id, raw_json)
                VALUES (@id, @title, @create_time, @update_time, @gizmo_id, @raw_json);";

            var conversationList = conversations.ToList();

            connection.Execute(conversationSql, conversationList.Select(conversation => new
            {
                id = conversation.id ?? string.Empty,
                title = conversation.title ?? string.Empty,
                create_time = (long)conversation.create_time,
                update_time = (long)conversation.update_time,
                gizmo_id = conversation.gizmo_id,
                raw_json = JsonSerializer.Serialize(conversation)
            }), transaction);

            const string messageSql = @"
                INSERT OR REPLACE INTO messages (id, conversation_id, role, content, create_time)
                VALUES (@id, @conversation_id, @role, @content, @create_time);";

            var messageParams = new List<object>();

            foreach (var conversation in conversationList)
            {
                var plaintextExtractor = new PlaintextExtractor();

                if (conversation.mapping == null)
                    continue;

                var conversationId = conversation.id ?? string.Empty;

                var mapping = conversation.GetLastestConversation().mapping;
                if (mapping == null)
                    continue;

                foreach (var messageContainerEntry in mapping)
                {
                    var messageContainer = messageContainerEntry.Value;
                    var message = messageContainer.message;

                    if (message == null)
                        continue;

                    var messageId = message.id ?? messageContainer.id ?? string.Empty;
                    var role = message.author?.role ?? string.Empty;
                    var plaintext = plaintextExtractor.ExtractPlaintext(message);
                    var messageCreateTime = message.create_time.HasValue ? (long)message.create_time.Value : 0;

                    messageParams.Add(new
                    {
                        id = messageId,
                        conversation_id = conversationId,
                        role,
                        content = plaintext,
                        create_time = messageCreateTime
                    });
                }
            }

            if (messageParams.Count > 0)
            {
                connection.Execute(messageSql, messageParams, transaction);
            }

            transaction.Commit();
        }

        public IEnumerable<Conversation> GetAll()
        {
            using var connection = new SqliteConnection(databaseConfiguration.ConnectionString);
            connection.Open();

            return connection.Query<Conversation>(@"SELECT id, title, create_time, update_time, gizmo_id
                FROM conversations 
                ORDER BY update_time DESC");
        }

        public Conversation? GetById(string id)
        {
            using var connection = new SqliteConnection(databaseConfiguration.ConnectionString);
            connection.Open();

            var json = connection.QuerySingleOrDefault<string>("SELECT raw_json FROM conversations WHERE id = @id", new { id });
            return json == null ? null : JsonSerializer.Deserialize<Conversation>(json);
        }

        public IEnumerable<SearchResult> Search(string query)
        {
            using var connection = new SqliteConnection(databaseConfiguration.ConnectionString);
            connection.Open();

            var ftsQuery = fTS5Escaper.EscapeFts5Query(query);
            try
            {
                return ExecuteSearch(connection, ftsQuery);
            }
            catch (SqliteException)
            {
                var fallbackQuery = fTS5Escaper.QuoteAsLiteral((query ?? string.Empty).Trim());
                if (!string.Equals(ftsQuery, fallbackQuery, StringComparison.Ordinal))
                {
                    return ExecuteSearch(connection, fallbackQuery);
                }
                else
                {
                    throw;
                }
            }
        }

        private static IEnumerable<SearchResult> ExecuteSearch(SqliteConnection connection, string query)
        {
            const string sql = @"
                SELECT
                    m.conversation_id AS ConversationId,
                    c.title AS ConversationTitle,
                    m.id AS MessageId,
                    snippet(messages_fts, 0, '<b>', '</b>', '...', 20) AS Snippet
                FROM messages_fts
                JOIN messages m ON m.rowid = messages_fts.rowid
                JOIN conversations c ON c.id = m.conversation_id
                WHERE messages_fts MATCH @query
                ORDER BY rank
                LIMIT 200;";

            return connection.Query<SearchResult>(sql, new { query });
        }
    }
}
