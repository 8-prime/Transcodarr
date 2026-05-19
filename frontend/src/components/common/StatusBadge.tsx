import { cn } from '@/lib/utils'

type Status = 'pending' | 'processing' | 'completed' | 'failed' | 'idle' | 'ready'

const STYLES: Record<Status, string> = {
  pending:
    'bg-[color-mix(in_oklab,var(--status-pending)_15%,transparent)] text-[var(--status-pending)] border-[color-mix(in_oklab,var(--status-pending)_30%,transparent)]',
  processing:
    'bg-[color-mix(in_oklab,var(--status-processing)_15%,transparent)] text-[var(--status-processing)] border-[color-mix(in_oklab,var(--status-processing)_35%,transparent)]',
  ready:
    'bg-[color-mix(in_oklab,var(--status-processing)_15%,transparent)] text-[var(--status-processing)] border-[color-mix(in_oklab,var(--status-processing)_35%,transparent)]',
  completed:
    'bg-[color-mix(in_oklab,var(--status-completed)_15%,transparent)] text-[var(--status-completed)] border-[color-mix(in_oklab,var(--status-completed)_30%,transparent)]',
  failed:
    'bg-[color-mix(in_oklab,var(--status-failed)_15%,transparent)] text-[var(--status-failed)] border-[color-mix(in_oklab,var(--status-failed)_35%,transparent)]',
  idle: 'bg-muted/40 text-muted-foreground border-border',
}

interface StatusBadgeProps {
  status: Status
  label: string
  pulse?: boolean
}

export function StatusBadge({ status, label, pulse }: StatusBadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 rounded-full border px-2 py-0.5 text-xs font-medium',
        STYLES[status],
      )}
    >
      <span
        className={cn(
          'h-1.5 w-1.5 rounded-full bg-current',
          pulse && 'animate-pulse',
        )}
      />
      {label}
    </span>
  )
}
