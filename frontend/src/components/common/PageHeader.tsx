import { cn } from '@/lib/utils'

interface PageHeaderProps {
  title: string
  description?: string
  kicker?: string
  actions?: React.ReactNode
  className?: string
}

export function PageHeader({
  title,
  description,
  kicker,
  actions,
  className,
}: PageHeaderProps) {
  return (
    <div
      className={cn(
        'flex flex-col gap-3 border-b border-border/70 pb-5 sm:flex-row sm:items-end sm:justify-between',
        className,
      )}
    >
      <div className="space-y-1">
        {kicker && (
          <div className="font-mono text-[11px] uppercase tracking-[0.18em] text-[var(--status-processing)]">
            {kicker}
          </div>
        )}
        <h1 className="text-2xl font-semibold tracking-tight text-foreground">
          {title}
        </h1>
        {description && (
          <p className="max-w-2xl text-sm text-muted-foreground">{description}</p>
        )}
      </div>
      {actions && <div className="flex items-center gap-2">{actions}</div>}
    </div>
  )
}
