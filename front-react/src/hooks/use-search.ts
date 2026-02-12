import { useQuery } from '@tanstack/react-query'
import { search as searchConversations } from '../api-client'
import type { ConversationSearchResult } from '../models'

export function useSearch(query: string) {
  const normalizedQuery = query.trim()

  return useQuery<ConversationSearchResult[], Error>({
    queryKey: ['search', normalizedQuery],
    queryFn: () => searchConversations(normalizedQuery),
    enabled: normalizedQuery.length > 2,
  })
}