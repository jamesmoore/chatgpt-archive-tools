export interface ConversationSummary {
  id: string;
  title: string;
  gizmoId?: string;
  created: string;
  updated: string;
}

export interface ConversationSearchResult {
  conversationId: string;
  messageId: string;
  snippet: string;
  conversationTitle: string;
}