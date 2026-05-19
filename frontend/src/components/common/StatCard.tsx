import type { LucideIcon } from 'lucide-react'
import { cn } from '@/lib/utils'

interface StatCardProps {
  label: string
  value: React.ReactNode
  hint?: React.ReactNode
  icon?: LucideIcon
  tone?: 'default' | 'accent' | 'warn' | 'ok'
  className?: string
}

const TONE: Record<NonNullable<StatCardProps['tone']>, string> = {
  default: 'text-foreground',
  accent: 'text-[var(--status-processing)]',
  warn: 'text-[var(--status-pending)]',
  ok: 'text-[var(--status-completed)]',
}

export function StatCard({
  label,
  value,
  hint,
  icon: Icon,
  tone = 'default',
  className,
}: StatCardProps) {
  return (
    <div
      className={cn(
        'group relative overflow-hidden rounded-lg border border-border bg-card p-4',
        'transition-colors hover:border-[color-mix(in_oklab,var(--primary)_30%,var(--border))]',
        className,
      )}
    >
      <div className="flex items-start justify-between gap-3">
        <div className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
          {label}
        </div>
        {Icon && (
          <Icon className={cn('h-4 w-4 opacity-70', TONE[tone])} aria-hidden />
        )}
      </div>
      <div className={cn('mt-2 font-mono text-3xl font-semibold leading-none', TONE[tone])}>
        {value}
      </div>
      {hint && (
        <div className="mt-2 text-xs text-muted-foreground">{hint}</div>
      )}
      <div
        aria-hidden
        className="pointer-events-none absolute inset-x-0 bottom-0 h-px bg-gradient-to-r from-transparent via-[color-mix(in_oklab,var(--primary)_40%,transparent)] to-transparent opacity-0 transition-opacity group-hover:opacity-100"
      />
    </div>
  )
}
