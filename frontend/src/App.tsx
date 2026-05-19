import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { TooltipProvider } from '@/components/ui/tooltip'
import { AppLayout } from '@/components/layout/AppLayout'
import { DashboardPage } from '@/pages/Dashboard'
import { NodesPage } from '@/pages/Nodes'
import { QueuePage } from '@/pages/Queue'
import { HistoryPage } from '@/pages/History'
import { LibrariesPage } from '@/pages/Libraries'
import { SettingsPage } from '@/pages/Settings'

function App() {
  return (
    <TooltipProvider delayDuration={150}>
      <BrowserRouter>
        <Routes>
          <Route element={<AppLayout />}>
            <Route index element={<DashboardPage />} />
            <Route path="nodes" element={<NodesPage />} />
            <Route path="queue" element={<QueuePage />} />
            <Route path="history" element={<HistoryPage />} />
            <Route path="libraries" element={<LibrariesPage />} />
            <Route path="settings" element={<SettingsPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </TooltipProvider>
  )
}

export default App
