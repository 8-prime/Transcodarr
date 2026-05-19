import { apiFetch } from '@/lib/api'
import type { NodeConnectionInfo } from '@/types/node'

interface RawConnection {
  connectionId: string
  nodeInfo: { name: string; encoderCapabilities: { slots: number; encoderName: string }[] } | null
  freeSlots: number
  connectionIsReady: boolean
}

export async function getConnections(): Promise<NodeConnectionInfo[]> {
  const raw = await apiFetch<RawConnection[]>('/connections')
  return raw
}
