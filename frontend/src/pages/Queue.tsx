import { useMemo } from 'react'
import { PageHeader } from '@/components/common/PageHeader'
import { StatusBadge } from '@/components/common/StatusBadge'
import { MonoText } from '@/components/common/MonoText'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
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
import { fakeQueue } from '@/api/fakes/queue'
import { ENCODER_LABELS } from '@/types/node'
import type { JobState, TranscodeJob } from '@/types/job'

const TABS: { key: JobState; label: string }[] = [
  { key: 'Pending', label: 'Pending' },
  { key: 'Assigned', label: 'Assigned' },
  { key: 'Processing', label: 'Processing' },
]

export function QueuePage() {
  const grouped = useMemo(() => {
    return {
      Pending: fakeQueue.filter((j) => j.state === 'Pending'),
      Assigned: fakeQueue.filter((j) => j.state === 'Assigned'),
      Processing: fakeQueue.filter((j) => j.state === 'Processing'),
    } as Record<'Pending' | 'Assigned' | 'Processing', TranscodeJob[]>
  }, [])

  return (
    <div className="space-y-6">
      <PageHeader
        kicker="Placeholder · backend not wired yet"
        title="Queue"
        description="Jobs waiting to run, recently assigned, and currently encoding. Connected to faked data for now."
      />

      <Tabs defaultValue="Processing" className="space-y-4">
        <TabsList>
          {TABS.map((t) => (
            <TabsTrigger key={t.key} value={t.key} className="gap-2">
              {t.label}
              <span className="font-mono text-[10px] text-muted-foreground">
                {grouped[t.key as 'Pending' | 'Assigned' | 'Processing'].length}
              </span>
            </TabsTrigger>
          ))}
        </TabsList>

        {TABS.map((t) => (
          <TabsContent key={t.key} value={t.key}>
            <QueueTable
              jobs={grouped[t.key as 'Pending' | 'Assigned' | 'Processing']}
              showProgress={t.key === 'Processing'}
              showNode={t.key !== 'Pending'}
            />
          </TabsContent>
        ))}
      </Tabs>
    </div>
  )
}

function QueueTable({
  jobs,
  showProgress,
  showNode,
}: {
  jobs: TranscodeJob[]
  showProgress: boolean
  showNode: boolean
}) {
  if (jobs.length === 0) {
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
            <TableHead className="w-[160px]">Target</TableHead>
            {showNode && <TableHead className="w-[140px]">Node</TableHead>}
            {showProgress && (
              <TableHead className="w-[180px]">Progress</TableHead>
            )}
            <TableHead className="w-[120px]">State</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {jobs.map((job) => (
            <TableRow key={job.id}>
              <TableCell>
                <div className="truncate text-sm">{job.fileName}</div>
                <MonoText muted className="block truncate">
                  attempt #{job.attemptNumber + 1}
                </MonoText>
              </TableCell>
              <TableCell>
                <span className="text-sm text-muted-foreground">{job.libraryName}</span>
              </TableCell>
              <TableCell>
                <Badge variant="secondary" className="font-mono text-[11px]">
                  {ENCODER_LABELS[job.targetEncoder] ?? job.targetEncoder}
                </Badge>
              </TableCell>
              {showNode && (
                <TableCell>
                  {job.nodeId ? (
                    <MonoText className="text-foreground">{job.nodeId}</MonoText>
                  ) : (
                    <span className="text-xs text-muted-foreground">—</span>
                  )}
                </TableCell>
              )}
              {showProgress && (
                <TableCell>
                  {job.progressPct != null ? (
                    <div className="flex items-center gap-2">
                      <Progress value={job.progressPct} className="h-1.5" />
                      <MonoText className="w-10 text-right">{job.progressPct}%</MonoText>
                    </div>
                  ) : (
                    <span className="text-xs text-muted-foreground">—</span>
                  )}
                </TableCell>
              )}
              <TableCell>
                <StatusBadge
                  status={stateToStatus(job.state)}
                  label={job.state}
                  pulse={job.state === 'Processing'}
                />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}

function stateToStatus(state: JobState) {
  switch (state) {
    case 'Pending':
      return 'pending' as const
    case 'Assigned':
      return 'pending' as const
    case 'Processing':
      return 'processing' as const
    case 'Completed':
      return 'completed' as const
    case 'Failed':
    case 'Cancelled':
    case 'LeaseExpired':
      return 'failed' as const
  }
}
