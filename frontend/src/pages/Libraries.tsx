import { FolderTree, Plus } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { PageHeader } from '@/components/common/PageHeader'
import { MonoText } from '@/components/common/MonoText'
import { StatusBadge } from '@/components/common/StatusBadge'
import { Switch } from '@/components/ui/switch'
import { Button } from '@/components/ui/button'
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip'
import { apiFetch } from '@/lib/api'
import type { Library } from '@/types/library'

export function LibrariesPage() {
  const { data: libraries = [] } = useQuery({
    queryKey: ['libraries'],
    queryFn: () => apiFetch<Library[]>('/libraries'),
    refetchInterval: 10000,
  })

  return (
    <div className="space-y-6">
      <PageHeader
        title="Libraries"
        description="Configured media libraries. Files added here get scanned, probed, and queued for transcoding."
        actions={
          <Tooltip>
            <TooltipTrigger asChild>
              <span>
                <Button size="sm" disabled>
                  <Plus className="h-3.5 w-3.5" />
                  Add library
                </Button>
              </span>
            </TooltipTrigger>
            <TooltipContent>Coming soon</TooltipContent>
          </Tooltip>
        }
      />

      <div className="grid gap-3 md:grid-cols-2">
        {libraries.map((lib) => (
          <div
            key={lib.id}
            className="group relative overflow-hidden rounded-lg border border-border bg-card p-4 transition-colors hover:border-[color-mix(in_oklab,var(--primary)_30%,var(--border))]"
          >
            <div className="flex items-start justify-between gap-3">
              <div className="flex items-start gap-3">
                <div className="flex h-9 w-9 items-center justify-center rounded-md border border-border bg-background text-muted-foreground">
                  <FolderTree className="h-4 w-4" aria-hidden />
                </div>
                <div>
                  <div className="text-sm font-semibold text-foreground">
                    {lib.name}
                  </div>
                  <MonoText muted className="block break-all">
                    {lib.path}
                  </MonoText>
                </div>
              </div>
              {lib.watching ? (
                <StatusBadge status="ready" label="Watching" pulse />
              ) : (
                <StatusBadge status="idle" label="Paused" />
              )}
            </div>

            <div className="mt-4 grid grid-cols-2 gap-4 border-t border-border/70 pt-3 text-xs">
              <div>
                <div className="font-mono text-[10px] uppercase tracking-wider text-muted-foreground">
                  Files
                </div>
                <div className="mt-0.5 font-mono text-base text-foreground">
                  {lib.fileCount.toLocaleString()}
                </div>
              </div>
              <div>
                <div className="font-mono text-[10px] uppercase tracking-wider text-muted-foreground">
                  Last scan
                </div>
                <div className="mt-0.5 font-mono text-xs text-foreground">
                  {lib.lastScanAt
                    ? new Date(lib.lastScanAt).toLocaleString()
                    : 'never'}
                </div>
              </div>
            </div>

            <div className="mt-3 flex items-center justify-between border-t border-border/70 pt-3">
              <span className="text-xs text-muted-foreground">Auto-watch</span>
              <Switch checked={lib.watching} disabled />
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
