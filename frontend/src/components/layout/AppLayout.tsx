import { Outlet } from 'react-router-dom'
import { Sidebar } from '@/components/layout/Sidebar'

export function AppLayout() {
  return (
    <div className="flex h-full min-h-screen">
      <Sidebar />
      <main className="flex-1 overflow-y-auto">
        <div className="mx-auto max-w-6xl space-y-6 px-8 py-6">
          <Outlet />
        </div>
      </main>
    </div>
  )
}
