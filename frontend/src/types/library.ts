export interface Library {
  id: string
  name: string
  path: string
  fileCount: number
  watching: boolean
  lastScanAt: string | null
}
