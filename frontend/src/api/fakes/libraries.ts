import type { Library } from '@/types/library'

export const fakeLibraries: Library[] = [
  {
    id: 'lib-movies',
    name: 'Movies',
    path: '/mnt/media/movies',
    fileCount: 1284,
    watching: true,
    lastScanAt: '2026-05-20T06:00:00Z',
  },
  {
    id: 'lib-tv',
    name: 'TV Shows',
    path: '/mnt/media/tv',
    fileCount: 8412,
    watching: true,
    lastScanAt: '2026-05-20T06:02:00Z',
  },
  {
    id: 'lib-anime',
    name: 'Anime',
    path: '/mnt/media/anime',
    fileCount: 612,
    watching: false,
    lastScanAt: '2026-05-15T03:30:00Z',
  },
  {
    id: 'lib-docs',
    name: 'Documentaries',
    path: '/mnt/media/docs',
    fileCount: 184,
    watching: true,
    lastScanAt: '2026-05-20T05:58:00Z',
  },
]
