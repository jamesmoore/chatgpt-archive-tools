import { useQuery } from '@tanstack/react-query'
import { getStatus, type Status } from '../api-client'

export function useStatus() {
  return useQuery<Status, Error>({
    queryKey: ['status'],
    queryFn: getStatus,
  })
}
