import { NavLink } from 'react-router-dom'
import {
  LayoutDashboard,
  Server,
  ListOrdered,
  History,
  FolderTree,
  Settings,
  type LucideIcon,
} from 'lucide-react'
import { cn } from '@/lib/utils'
import { useConnections } from '@/hooks/useConnections'

interface NavItem {
  to: string
  label: string
  icon: LucideIcon
  end?: boolean
}

const NAV: NavItem[] = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/nodes', label: 'Nodes', icon: Server },
  { to: '/queue', label: 'Queue', icon: ListOrdered },
  { to: '/history', label: 'History', icon: History },
  { to: '/libraries', label: 'Libraries', icon: FolderTree },
  { to: '/settings', label: 'Settings', icon: Settings },
]

export function Sidebar() {
  const { data: connections, isError, isLoading } = useConnections()
  const readyCount = connections?.filter((c) => c.connectionIsReady).length ?? 0
  const total = connections?.length ?? 0

  return (
    <aside className="flex h-full w-60 shrink-0 flex-col border-r border-sidebar-border bg-sidebar text-sidebar-foreground">
      <div className="flex h-14 items-center gap-2.5 px-4">
        <div className="flex h-8 w-8 items-center justify-center rounded-md border border-[color-mix(in_oklab,var(--primary)_40%,var(--sidebar-border))] bg-[color-mix(in_oklab,var(--primary)_10%,var(--sidebar))] font-mono text-sm font-bold text-[var(--primary)]">
          T.
        </div>
        <div className="leading-tight">
          <div className="text-sm font-semibold tracking-tight">Transcodarr</div>
          <div className="font-mono text-[10px] uppercase tracking-[0.2em] text-muted-foreground">
            v0.1 · dev
          </div>
        </div>
      </div>

      <div className="px-3 pt-2 pb-1 font-mono text-[10px] uppercase tracking-[0.2em] text-muted-foreground">
        Workspace
      </div>
      <nav className="flex-1 space-y-0.5 px-2">
        {NAV.map(({ to, label, icon: Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            className={({ isActive }) =>
              cn(
                'group relative flex items-center gap-2.5 rounded-md px-2.5 py-1.5 text-sm transition-colors',
                'hover:bg-sidebar-accent hover:text-sidebar-accent-foreground',
                isActive
                  ? 'bg-sidebar-accent text-sidebar-accent-foreground'
                  : 'text-muted-foreground',
              )
            }
          >
            {({ isActive }) => (
              <>
                <span
                  className={cn(
                    'absolute left-0 top-1/2 h-5 w-0.5 -translate-y-1/2 rounded-r bg-[var(--primary)] transition-opacity',
                    isActive ? 'opacity-100' : 'opacity-0',
                  )}
                  aria-hidden
                />
                <Icon className="h-4 w-4 shrink-0" aria-hidden />
                <span>{label}</span>
              </>
            )}
          </NavLink>
        ))}
      </nav>

      <div className="m-3 rounded-md border border-sidebar-border bg-[color-mix(in_oklab,var(--sidebar)_50%,var(--background))] p-3">
        <div className="font-mono text-[10px] uppercase tracking-[0.18em] text-muted-foreground">
          Cluster
        </div>
        <div className="mt-1.5 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <span
              className={cn(
                'relative flex h-2 w-2',
                isError && 'text-[var(--status-failed)]',
              )}
              aria-hidden
            >
              <span
                className={cn(
                  'absolute inline-flex h-full w-full animate-ping rounded-full opacity-60',
                  isError
                    ? 'bg-[var(--status-failed)]'
                    : readyCount > 0
                      ? 'bg-[var(--status-completed)]'
                      : 'bg-[var(--status-idle)]',
                )}
              />
              <span
                className={cn(
                  'relative inline-flex h-2 w-2 rounded-full',
                  isError
                    ? 'bg-[var(--status-failed)]'
                    : readyCount > 0
                      ? 'bg-[var(--status-completed)]'
                      : 'bg-[var(--status-idle)]',
                )}
              />
            </span>
            <span className="text-xs text-foreground">
              {isError
                ? 'Backend offline'
                : isLoading
                  ? 'Connecting…'
                  : `${readyCount} of ${total || readyCount} online`}
            </span>
          </div>
          <span className="font-mono text-[11px] text-muted-foreground">nodes</span>
        </div>
      </div>
    </aside>
  )
}
