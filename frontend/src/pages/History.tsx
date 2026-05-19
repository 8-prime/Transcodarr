import { PageHeader } from '@/components/common/PageHeader'
import { MonoText } from '@/components/common/MonoText'
import { Badge } from '@/components/ui/badge'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { fakeHistory } from '@/api/fakes/history'
import { ENCODER_LABELS } from '@/types/node'
import { cn } from '@/lib/utils'

const GB = 1024 ** 3

function vmafTone(v: number) {
  if (v >= 95) return 'text-[var(--status-completed)]'
  if (v >= 92) return 'text-[var(--status-processing)]'
  if (v >= 88) return 'text-[var(--status-pending)]'
  return 'text-[var(--status-failed)]'
}

function formatDuration(sec: number) {
  const m = Math.floor(sec / 60)
  const s = sec % 60
  return `${m}m ${s.toString().padStart(2, '0')}s`
}

export function HistoryPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        kicker="Placeholder · backend not wired yet"
        title="History"
        description="Completed transcode jobs with quality and size deltas. Connected to faked data for now."
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
            {fakeHistory.map((job) => {
              const inputGB = job.inputSizeBytes / GB
              const outputGB = job.outputSizeBytes / GB
              const deltaPct = ((outputGB - inputGB) / inputGB) * 100
              return (
                <TableRow key={job.id}>
                  <TableCell>
                    <div className="truncate text-sm">{job.fileName}</div>
                    <MonoText muted className="block">
                      {job.libraryName}
                    </MonoText>
                  </TableCell>
                  <TableCell>
                    <Badge variant="secondary" className="font-mono text-[11px]">
                      {ENCODER_LABELS[job.encoderUsed] ?? job.encoderUsed}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right font-mono text-sm">
                    {job.crf}
                  </TableCell>
                  <TableCell className="text-right">
                    <span className={cn('font-mono text-sm font-medium', vmafTone(job.vmaf))}>
                      {job.vmaf.toFixed(1)}
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
    </div>
  )
}
