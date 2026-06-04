import { useState } from 'react'
import { useQuery, keepPreviousData } from '@tanstack/react-query'
import { PageHeader } from '@/components/common/PageHeader'
import { MonoText } from '@/components/common/MonoText'
import { Badge } from '@/components/ui/badge'
import { Pagination } from '@/components/ui/pagination'
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
import { cn } from '@/lib/utils'
import type { CompletedJob } from '@/types/job'
import type { PagedResponse } from '@/types/api'

const GB = 1024 ** 3

function vmafTone(v: number) {
  if (v >= 95) return 'text-[var(--status-completed)]'
  if (v >= 92) return 'text-[var(--status-processing)]'
  if (v >= 88) return 'text-[var(--status-pending)]'
  return 'text-[var(--status-failed)]'
}

function formatDuration(sec: number) {
  const m = Math.floor(sec / 60)
  const s = Math.floor(sec % 60)
  return `${m}m ${s.toString().padStart(2, '0')}s`
}

export function HistoryPage() {
  const [page, setPage] = useState(1)

  const { data } = useQuery({
    queryKey: ['history', page],
    queryFn: () => apiFetch<PagedResponse<CompletedJob>>(`/history?page=${page}&pageSize=25`),
    refetchInterval: 10000,
    placeholderData: keepPreviousData,
  })

  const history = data?.items ?? []
  const totalPages = data?.totalPages ?? 1

  return (
    <div className="space-y-6">
      <PageHeader
        title="History"
        description="Completed transcode jobs with quality and size deltas."
      />

      <div className="overflow-hidden rounded-lg border border-border bg-card">
        <Table>
          <TableHeader>
            <TableRow className="hover:bg-transparent">
              <TableHead>File</TableHead>
              <TableHead className="w-[160px]">Encoder</TableHead>
              <TableHead className="w-[90px] text-right">CRF</TableHead>
              <TableHead className="w-[90px] text-right">VMAF</TableHead>
              <TableHead className="w-[120px] text-right">Size Δ</TableHead>
              <TableHead className="w-[110px] text-right">Duration</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {history.map((job) => {
              const inputGB = job.inputSizeBytes / GB
              const outputGB = job.outputSizeBytes / GB
              const deltaPct = ((outputGB - inputGB) / inputGB) * 100
              return (
                <TableRow key={job.id}>
                  <TableCell>
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <div className="max-w-[280px] cursor-default truncate text-sm">
                          {job.fileName}
                        </div>
                      </TooltipTrigger>
                      <TooltipContent className="max-w-xs break-all">
                        {job.fileName}
                      </TooltipContent>
                    </Tooltip>
                    <MonoText muted className="block">
                      {job.libraryName}
                    </MonoText>
                  </TableCell>
                  <TableCell>
                    <Badge variant="secondary" className="font-mono text-[11px]">
                      {job.encoderUsed}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right font-mono text-sm">
                    {job.crf}
                  </TableCell>
                  <TableCell className="text-right">
                    <span className={cn('font-mono text-sm font-medium', vmafTone(job.vmafScore))}>
                      {job.vmafScore.toFixed(1)}
                    </span>
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="font-mono text-sm text-[var(--status-completed)]">
                      {deltaPct.toFixed(0)}%
                    </div>
                    <MonoText muted className="block">
                      {inputGB.toFixed(1)} → {outputGB.toFixed(1)} GB
                    </MonoText>
                  </TableCell>
                  <TableCell className="text-right">
                    <MonoText>{formatDuration(job.durationSec)}</MonoText>
                  </TableCell>
                </TableRow>
              )
            })}
          </TableBody>
        </Table>
      </div>

      <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
    </div>
  )
}
