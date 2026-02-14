using ChatGPTExport.Models;

namespace ChatGpt.Archive.Api.Database
{
    public interface IArchiveRepository
    {
        bool HasConversations();
        void ClearAll();
        void InsertConversations(IEnumerable<Conversation> conversations);
        IEnumerable<Conversation> GetAll();
        Conversation? GetById(string id);
        IEnumerable<SearchResult> Search(string query);
    }
}
