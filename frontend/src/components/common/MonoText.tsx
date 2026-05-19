import { cn } from '@/lib/utils'

interface MonoTextProps extends React.HTMLAttributes<HTMLSpanElement> {
  muted?: boolean
}

export function MonoText({ className, muted, ...props }: MonoTextProps) {
  return (
    <span
      className={cn(
        'font-mono text-[0.85em] tracking-tight',
        muted && 'text-muted-foreground',
        className,
      )}
      {...props}
    />
  )
}
