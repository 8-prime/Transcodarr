import { ChevronLeft, ChevronRight, MoreHorizontal } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

interface PaginationProps {
  page: number
  totalPages: number
  onPageChange: (page: number) => void
  className?: string
}

function getPageWindow(page: number, totalPages: number): (number | null)[] {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, i) => i + 1)
  }

  const pages: (number | null)[] = [1]

  const windowStart = Math.max(2, page - 2)
  const windowEnd = Math.min(totalPages - 1, page + 2)

  if (windowStart > 2) pages.push(null)

  for (let i = windowStart; i <= windowEnd; i++) {
    pages.push(i)
  }

  if (windowEnd < totalPages - 1) pages.push(null)

  pages.push(totalPages)
  return pages
}

export function Pagination({ page, totalPages, onPageChange, className }: PaginationProps) {
  if (totalPages <= 1) return null

  const pageWindow = getPageWindow(page, totalPages)

  return (
    <div className={cn('flex items-center justify-center gap-1', className)}>
      <Button
        variant="ghost"
        size="sm"
        onClick={() => onPageChange(page - 1)}
        disabled={page <= 1}
        aria-label="Previous page"
      >
        <ChevronLeft className="size-4" />
        Prev
      </Button>

      {pageWindow.map((p, i) =>
        p === null ? (
          <span key={`ellipsis-${i}`} className="flex size-8 items-center justify-center text-muted-foreground">
            <MoreHorizontal className="size-4" />
          </span>
        ) : (
          <Button
            key={p}
            variant="ghost"
            size="sm"
            onClick={() => onPageChange(p)}
            className={cn(
              'size-8 px-0',
              p === page && 'bg-accent text-accent-foreground',
            )}
            aria-label={`Page ${p}`}
            aria-current={p === page ? 'page' : undefined}
          >
            {p}
          </Button>
        )
      )}

      <Button
        variant="ghost"
        size="sm"
        onClick={() => onPageChange(page + 1)}
        disabled={page >= totalPages}
        aria-label="Next page"
      >
        Next
        <ChevronRight className="size-4" />
      </Button>
    </div>
  )
}
