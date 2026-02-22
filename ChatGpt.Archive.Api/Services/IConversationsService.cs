using ChatGPTExport;
using ChatGPTExport.Models;

namespace ChatGpt.Archive.Api.Services
{
    public interface IConversationsService
    {
        IEnumerable<Conversation> GetLatestConversations();
        Conversation? GetConversation(string conversationId);
        IEnumerable<ConsolidatedSearchResult> Search(string query);
        string? GetContent(string conversationId, ExportType exportType);
        void ClearAll();
        void LoadConversations();
    }
}