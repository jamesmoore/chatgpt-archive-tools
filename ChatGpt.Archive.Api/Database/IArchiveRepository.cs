using ChatGPTExport.Models;

namespace ChatGpt.Archive.Api.Database
{
    public interface IArchiveRepository
    {
        void EnsureSchema();
        bool HasConversations();
        void InsertConversations(IEnumerable<Conversation> conversations);
        IEnumerable<Conversation> GetAll();
        Conversation? GetById(string id);
        IEnumerable<SearchResult> Search(string query);
    }
}
