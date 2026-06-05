import { useState, useEffect } from 'react'
import { useQuery, keepPreviousData } from '@tanstack/react-query'
import { PageHeader } from '@/components/common/PageHeader'
import { StatusBadge } from '@/components/common/StatusBadge'
import { MonoText } from '@/components/common/MonoText'
import { Badge } from '@/components/ui/badge'
import { Pagination } from '@/components/ui/pagination'
import { Progress } from '@/components/ui/progress'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip'
import { apiFetch } from '@/lib/api'
import type { QueueItem, QueueItemState } from '@/types/job'
import type { PagedResponse } from '@/types/api'

export function QueuePage() {
  const [page, setPage] = useState(1)

  const { data } = useQuery({
    queryKey: ['queue', page],
    queryFn: () => apiFetch<PagedResponse<QueueItem>>(`/queue?page=${page}&pageSize=25`),
    refetchInterval: 3000,
    placeholderData: keepPreviousData,
  })

  const items = data?.items ?? []
  const totalPages = data?.totalPages ?? 1

  useEffect(() => {
    if (data?.totalPages !== undefined && page > data.totalPages && data.totalPages > 0) {
      setPage(data.totalPages)
    }
  }, [data?.totalPages, page])

  return (
    <div className="space-y-6">
      <PageHeader
        title="Queue"
        description="All media files awaiting or actively undergoing transcoding."
      />
      <QueueTable items={items} />
      <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
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
    <div className="overflow-x-auto rounded-lg border border-border bg-card">
      <Table className="min-w-[560px]">
        <TableHeader>
          <TableRow className="hover:bg-transparent">
            <TableHead>File</TableHead>
            <TableHead className="hidden w-[140px] sm:table-cell">Library</TableHead>
            <TableHead className="w-[120px]">State</TableHead>
            <TableHead className="w-[160px]">Codec</TableHead>
            <TableHead className="hidden w-[140px] sm:table-cell">Node</TableHead>
            <TableHead className="w-[180px]">Progress</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {items.map((item) => (
            <TableRow key={item.id}>
              <TableCell>
                <Tooltip>
                  <TooltipTrigger asChild>
                    <div className="max-w-[260px] cursor-default truncate text-sm">
                      {item.fileName}
                    </div>
                  </TooltipTrigger>
                  <TooltipContent className="max-w-xs break-all">
                    {item.fileName}
                  </TooltipContent>
                </Tooltip>
                {item.attemptNumber > 0 && (
                  <MonoText muted className="block truncate">
                    attempt #{item.attemptNumber}
                  </MonoText>
                )}
              </TableCell>
              <TableCell className="hidden sm:table-cell">
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
              <TableCell className="hidden sm:table-cell">
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
