export interface ConversationSummary {
  id: string;
  title: string;
  gizmoId?: string;
  created: string;
  updated: string;
}

export interface ConversationSearchResult {
  conversationId: string;
  conversationTitle: string;
  messages: MessageSearchResult[];
}

export interface MessageSearchResult {
  messageId: string;
  snippet: string;
}