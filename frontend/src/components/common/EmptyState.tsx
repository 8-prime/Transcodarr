import type { LucideIcon } from 'lucide-react'
import { cn } from '@/lib/utils'

interface EmptyStateProps {
  icon?: LucideIcon
  title: string
  description?: string
  action?: React.ReactNode
  className?: string
}

export function EmptyState({
  icon: Icon,
  title,
  description,
  action,
  className,
}: EmptyStateProps) {
  return (
    <div
      className={cn(
        'flex flex-col items-center justify-center gap-3 rounded-lg border border-dashed border-border bg-card/40 px-6 py-12 text-center',
        className,
      )}
    >
      {Icon && (
        <div className="rounded-full border border-border bg-background/60 p-3 text-muted-foreground">
          <Icon className="h-5 w-5" aria-hidden />
        </div>
      )}
      <div className="space-y-1">
        <div className="text-sm font-medium text-foreground">{title}</div>
        {description && (
          <div className="max-w-md text-sm text-muted-foreground">{description}</div>
        )}
      </div>
      {action}
    </div>
  )
}
