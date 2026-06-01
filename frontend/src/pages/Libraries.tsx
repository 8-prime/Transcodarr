import { FolderTree, Plus } from 'lucide-react'
import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { PageHeader } from '@/components/common/PageHeader'
import { MonoText } from '@/components/common/MonoText'
import { StatusBadge } from '@/components/common/StatusBadge'
import { DirectoryPicker } from '@/components/DirectoryPicker'
import { Switch } from '@/components/ui/switch'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetFooter,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/sheet'
import { apiFetch } from '@/lib/api'
import type { Library } from '@/types/library'

export function LibrariesPage() {
  const queryClient = useQueryClient()
  const [open, setOpen] = useState(false)
  const [path, setPath] = useState('')
  const [displayName, setDisplayName] = useState('')

  const { data: libraries = [] } = useQuery({
    queryKey: ['libraries'],
    queryFn: () => apiFetch<Library[]>('/libraries'),
    refetchInterval: 10000,
  })

  const { mutate: addLibrary, isPending } = useMutation({
    mutationFn: () =>
      apiFetch<void>('/libraries', {
        method: 'POST',
        body: JSON.stringify({ path, displayName: displayName || undefined }),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['libraries'] })
      setOpen(false)
      setPath('')
      setDisplayName('')
    },
  })

  return (
    <div className="space-y-6">
      <PageHeader
        title="Libraries"
        description="Configured media libraries. Files added here get scanned, probed, and queued for transcoding."
        actions={
          <Sheet open={open} onOpenChange={setOpen}>
            <SheetTrigger asChild>
              <Button size="sm">
                <Plus className="h-3.5 w-3.5" />
                Add library
              </Button>
            </SheetTrigger>
            <SheetContent>
              <SheetHeader>
                <SheetTitle>Add library</SheetTitle>
                <SheetDescription>
                  Point to a directory on disk. Files will be scanned and queued for transcoding.
                </SheetDescription>
              </SheetHeader>
              <div className="flex flex-col gap-4 px-4 py-6">
                <div className="flex flex-col gap-1.5">
                  <span className="text-sm font-medium">Path</span>
                  <DirectoryPicker value={path} onChange={setPath} />
                </div>
                <div className="flex flex-col gap-1.5">
                  <span className="text-sm font-medium">Display name <span className="font-normal text-muted-foreground">(optional)</span></span>
                  <Input
                    placeholder="Movies"
                    value={displayName}
                    onChange={(e) => setDisplayName(e.target.value)}
                  />
                </div>
              </div>
              <SheetFooter>
                <Button variant="outline" onClick={() => setOpen(false)}>
                  Cancel
                </Button>
                <Button
                  onClick={() => addLibrary()}
                  disabled={!path.trim() || isPending}
                >
                  Add library
                </Button>
              </SheetFooter>
            </SheetContent>
          </Sheet>
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
