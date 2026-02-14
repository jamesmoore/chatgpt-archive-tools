using ChatGpt.Archive.Api.Database;
using ChatGPTExport.Models;

namespace ChatGpt.Archive.Api.Services
{
    public interface IConversationsService
    {
        IEnumerable<Conversation> GetLatestConversations();
        Conversation? GetConversation(string conversationId);
        IEnumerable<ConsolidatedSearchResult> Search(string query);
        void ClearAll();
        void LoadConversations();
    }
}