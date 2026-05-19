import { useQuery } from '@tanstack/react-query'
import { getConnections } from '@/api/connections'

export function useConnections() {
  return useQuery({
    queryKey: ['connections'],
    queryFn: getConnections,
    refetchInterval: 3000,
    refetchOnWindowFocus: true,
    staleTime: 1000,
  })
}
