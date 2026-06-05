import { Server, RefreshCw, Cpu } from 'lucide-react'
import { useConnections } from '@/hooks/useConnections'
import { PageHeader } from '@/components/common/PageHeader'
import { EmptyState } from '@/components/common/EmptyState'
import { StatusBadge } from '@/components/common/StatusBadge'
import { MonoText } from '@/components/common/MonoText'
import { Skeleton } from '@/components/ui/skeleton'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { ENCODER_LABELS } from '@/types/node'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

export function NodesPage() {
  const { data, isLoading, isError, isFetching, refetch, dataUpdatedAt } =
    useConnections()

  const updatedLabel = dataUpdatedAt
    ? new Date(dataUpdatedAt).toLocaleTimeString()
    : '—'

  return (
    <div className="space-y-6">
      <PageHeader
        kicker="Live · polling every 3s"
        title="Nodes"
        description="Worker nodes currently connected to the orchestrator via WebSocket."
        actions={
          <Button
            variant="outline"
            size="sm"
            onClick={() => refetch()}
            disabled={isFetching}
          >
            <RefreshCw
              className={`h-3.5 w-3.5 ${isFetching ? 'animate-spin' : ''}`}
            />
            Refresh
          </Button>
        }
      />

      <div className="flex items-center justify-between text-xs text-muted-foreground">
        <div>
          {isError
            ? 'Backend unreachable — is the Core service running on :5037?'
            : `${data?.length ?? 0} connection${data?.length === 1 ? '' : 's'}`}
        </div>
        <MonoText muted>last update {updatedLabel}</MonoText>
      </div>

      {isLoading ? (
        <NodesSkeleton />
      ) : !data || data.length === 0 ? (
        <EmptyState
          icon={Server}
          title="No nodes connected"
          description="Start a Transcodarr.Node process and it will appear here automatically once the WebSocket handshake completes."
        />
      ) : (
        <div className="overflow-x-auto rounded-lg border border-border bg-card">
          <Table className="min-w-[480px]">
            <TableHeader>
              <TableRow className="hover:bg-transparent">
                <TableHead className="w-[280px]">Node</TableHead>
                <TableHead className="hidden sm:table-cell">Encoders</TableHead>
                <TableHead className="w-[140px] text-right">Free slots</TableHead>
                <TableHead className="w-[140px]">Status</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.map((node) => {
                const total =
                  node.nodeInfo?.encoderCapabilities.reduce(
                    (sum, e) => sum + e.slots,
                    0,
                  ) ?? 0
                const used = Math.max(0, total - node.freeSlots)
                return (
                  <TableRow key={node.connectionId}>
                    <TableCell>
                      <div className="flex items-center gap-2.5">
                        <div className="flex h-8 w-8 items-center justify-center rounded-md border border-border bg-background">
                          <Cpu className="h-4 w-4 text-muted-foreground" aria-hidden />
                        </div>
                        <div className="min-w-0">
                          <div className="truncate text-sm font-medium text-foreground">
                            {node.nodeInfo?.name ?? 'Unknown node'}
                          </div>
                          <MonoText muted className="block truncate">
                            {node.connectionId}
                          </MonoText>
                        </div>
                      </div>
                    </TableCell>
                    <TableCell className="hidden sm:table-cell">
                      {node.nodeInfo?.encoderCapabilities.length ? (
                        <div className="flex flex-wrap gap-1.5">
                          {node.nodeInfo.encoderCapabilities.map((e) => (
                            <Badge
                              key={e.encoderName}
                              variant="secondary"
                              className="gap-1 font-mono text-[11px]"
                            >
                              {ENCODER_LABELS[e.encoderName] ?? e.encoderName}
                              <span className="text-muted-foreground">·</span>
                              <span>{e.slots}</span>
                            </Badge>
                          ))}
                        </div>
                      ) : (
                        <span className="text-xs text-muted-foreground">
                          awaiting capabilities…
                        </span>
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="font-mono text-sm">
                        <span className="text-foreground">{node.freeSlots}</span>
                        {total > 0 && (
                          <span className="text-muted-foreground"> / {total}</span>
                        )}
                      </div>
                      {total > 0 && (
                        <div className="mt-1 ml-auto h-1 w-24 overflow-hidden rounded-full bg-muted">
                          <div
                            className="h-full bg-[var(--status-processing)]"
                            style={{ width: `${(used / total) * 100}%` }}
                          />
                        </div>
                      )}
                    </TableCell>
                    <TableCell>
                      {node.connectionIsReady ? (
                        <StatusBadge status="ready" label="Ready" pulse />
                      ) : (
                        <StatusBadge status="pending" label="Handshake" />
                      )}
                    </TableCell>
                  </TableRow>
                )
              })}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  )
}

function NodesSkeleton() {
  return (
    <div className="overflow-hidden rounded-lg border border-border bg-card">
      <div className="space-y-px">
        {Array.from({ length: 3 }).map((_, i) => (
          <div
            key={i}
            className="flex items-center gap-4 border-b border-border/60 p-4 last:border-b-0"
          >
            <Skeleton className="h-8 w-8 rounded-md" />
            <div className="flex-1 space-y-2">
              <Skeleton className="h-3 w-32" />
              <Skeleton className="h-2.5 w-48" />
            </div>
            <Skeleton className="h-6 w-24 rounded-full" />
          </div>
        ))}
      </div>
    </div>
  )
}
