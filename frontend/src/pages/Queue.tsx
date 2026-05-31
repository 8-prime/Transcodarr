import { useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { PageHeader } from '@/components/common/PageHeader'
import { StatusBadge } from '@/components/common/StatusBadge'
import { MonoText } from '@/components/common/MonoText'
import { Badge } from '@/components/ui/badge'
import { Progress } from '@/components/ui/progress'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { apiFetch } from '@/lib/api'
import type { QueueItem, QueueItemState } from '@/types/job'

const STATE_ORDER: QueueItemState[] = [
  'Processing',
  'Pending',
  'Discovered',
  'Failed',
  'LeaseExpired',
]

export function QueuePage() {
  const { data: items = [] } = useQuery({
    queryKey: ['queue'],
    queryFn: () => apiFetch<QueueItem[]>('/queue'),
    refetchInterval: 3000,
  })

  const sorted = useMemo(
    () =>
      [...items].sort(
        (a, b) => STATE_ORDER.indexOf(a.state) - STATE_ORDER.indexOf(b.state),
      ),
    [items],
  )

  return (
    <div className="space-y-6">
      <PageHeader
        title="Queue"
        description="All media files awaiting or actively undergoing transcoding."
      />
      <QueueTable items={sorted} />
    </div>
  )
}

function QueueTable({ items }: { items: QueueItem[] }) {
  if (items.length === 0) {
    return (
      <div className="rounded-lg border border-dashed border-border bg-card/40 p-10 text-center text-sm text-muted-foreground">
        Nothing here.
      </div>
    )
  }

  return (
    <div className="overflow-hidden rounded-lg border border-border bg-card">
      <Table>
        <TableHeader>
          <TableRow className="hover:bg-transparent">
            <TableHead>File</TableHead>
            <TableHead className="w-[140px]">Library</TableHead>
            <TableHead className="w-[120px]">State</TableHead>
            <TableHead className="w-[160px]">Codec</TableHead>
            <TableHead className="w-[140px]">Node</TableHead>
            <TableHead className="w-[180px]">Progress</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {items.map((item) => (
            <TableRow key={item.id}>
              <TableCell>
                <div className="truncate text-sm">{item.fileName}</div>
                {item.attemptNumber > 0 && (
                  <MonoText muted className="block truncate">
                    attempt #{item.attemptNumber}
                  </MonoText>
                )}
              </TableCell>
              <TableCell>
                <span className="text-sm text-muted-foreground">{item.libraryName}</span>
              </TableCell>
              <TableCell>
                <StatusBadge
                  status={stateToStatus(item.state)}
                  label={item.state}
                  pulse={item.state === 'Processing'}
                />
              </TableCell>
              <TableCell>
                {item.targetCodec ? (
                  <Badge variant="secondary" className="font-mono text-[11px]">
                    {item.targetCodec}
                  </Badge>
                ) : (
                  <span className="text-xs text-muted-foreground">—</span>
                )}
              </TableCell>
              <TableCell>
                {item.nodeId ? (
                  <MonoText className="text-foreground">{item.nodeId}</MonoText>
                ) : (
                  <span className="text-xs text-muted-foreground">—</span>
                )}
              </TableCell>
              <TableCell>
                {item.progressPct != null ? (
                  <div className="flex items-center gap-2">
                    <Progress value={item.progressPct} className="h-1.5" />
                    <MonoText className="w-10 text-right">{item.progressPct}%</MonoText>
                  </div>
                ) : (
                  <span className="text-xs text-muted-foreground">—</span>
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}

function stateToStatus(state: QueueItemState) {
  switch (state) {
    case 'Discovered':
    case 'Pending':
      return 'pending' as const
    case 'Processing':
      return 'processing' as const
    case 'Completed':
      return 'completed' as const
    case 'Failed':
    case 'LeaseExpired':
      return 'failed' as const
  }
}
