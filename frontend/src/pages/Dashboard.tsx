import { Link } from 'react-router-dom'
import {
  Server,
  ListOrdered,
  CheckCircle2,
  Activity,
  ArrowRight,
} from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { useConnections } from '@/hooks/useConnections'
import { PageHeader } from '@/components/common/PageHeader'
import { StatCard } from '@/components/common/StatCard'
import { StatusBadge } from '@/components/common/StatusBadge'
import { MonoText } from '@/components/common/MonoText'
import { apiFetch } from '@/lib/api'
import type { TranscodeJob, CompletedJob } from '@/types/job'

export function DashboardPage() {
  const { data: connections, isError } = useConnections()
  const readyNodes = connections?.filter((c) => c.connectionIsReady).length ?? 0
  const totalFreeSlots =
    connections?.reduce((sum, c) => sum + c.freeSlots, 0) ?? 0

  const { data: queue = [] } = useQuery({
    queryKey: ['queue'],
    queryFn: () => apiFetch<TranscodeJob[]>('/queue'),
    refetchInterval: 3000,
  })

  const { data: history = [] } = useQuery({
    queryKey: ['history'],
    queryFn: () => apiFetch<CompletedJob[]>('/history'),
    refetchInterval: 10000,
  })

  const processing = queue.filter((j) => j.state === 'Processing')
  const pending = queue.filter(
    (j) => j.state === 'Pending' || j.state === 'Assigned',
  )

  return (
    <div className="space-y-8">
      <PageHeader
        kicker="Overview"
        title="Dashboard"
        description="A snapshot of the transcoding cluster."
      />

      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        <StatCard
          label="Nodes online"
          value={isError ? '—' : readyNodes}
          hint={
            isError
              ? 'backend unreachable'
              : `${totalFreeSlots} free slots cluster-wide`
          }
          icon={Server}
          tone={readyNodes > 0 ? 'accent' : 'default'}
        />
        <StatCard
          label="Processing"
          value={processing.length}
          hint="active transcodes"
          icon={Activity}
          tone="accent"
        />
        <StatCard
          label="Queued"
          value={pending.length}
          hint="pending + assigned"
          icon={ListOrdered}
          tone="warn"
        />
        <StatCard
          label="Completed"
          value={history.length}
          hint="all time"
          icon={CheckCircle2}
          tone="ok"
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <section className="rounded-lg border border-border bg-card lg:col-span-2">
          <header className="flex items-center justify-between border-b border-border/70 px-5 py-3">
            <div>
              <h2 className="text-sm font-semibold">Recent activity</h2>
              <p className="text-xs text-muted-foreground">
                Last completed jobs across the cluster
              </p>
            </div>
            <Link
              to="/history"
              className="inline-flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground"
            >
              All history <ArrowRight className="h-3 w-3" aria-hidden />
            </Link>
          </header>
          <ul className="divide-y divide-border/60">
            {history.slice(0, 5).map((job) => (
              <li
                key={job.id}
                className="flex items-center justify-between gap-4 px-5 py-3"
              >
                <div className="min-w-0">
                  <div className="truncate text-sm text-foreground">
                    {job.fileName}
                  </div>
                  <MonoText muted className="block">
                    {job.libraryName} · {job.encoderUsed} · CRF {job.crf}
                  </MonoText>
                </div>
                <div className="flex items-center gap-4">
                  <div className="text-right">
                    <div className="font-mono text-sm text-foreground">
                      {job.vmafScore.toFixed(1)}
                    </div>
                    <MonoText muted>VMAF</MonoText>
                  </div>
                  <StatusBadge status="completed" label="Done" />
                </div>
              </li>
            ))}
          </ul>
        </section>

        <section className="rounded-lg border border-border bg-card">
          <header className="flex items-center justify-between border-b border-border/70 px-5 py-3">
            <h2 className="text-sm font-semibold">Cluster</h2>
            <Link
              to="/nodes"
              className="inline-flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground"
            >
              All nodes <ArrowRight className="h-3 w-3" aria-hidden />
            </Link>
          </header>
          <div className="space-y-3 p-5">
            {connections && connections.length > 0 ? (
              connections.slice(0, 4).map((node) => (
                <div
                  key={node.connectionId}
                  className="flex items-center justify-between gap-3"
                >
                  <div className="min-w-0">
                    <div className="truncate text-sm font-medium text-foreground">
                      {node.nodeInfo?.name ?? 'Unknown'}
                    </div>
                    <MonoText muted className="block truncate">
                      {node.connectionId.slice(0, 16)}…
                    </MonoText>
                  </div>
                  <div className="text-right">
                    <div className="font-mono text-sm">{node.freeSlots}</div>
                    <MonoText muted>free</MonoText>
                  </div>
                </div>
              ))
            ) : (
              <div className="py-3 text-center text-xs text-muted-foreground">
                No nodes connected yet.
              </div>
            )}
          </div>
        </section>
      </div>
    </div>
  )
}
