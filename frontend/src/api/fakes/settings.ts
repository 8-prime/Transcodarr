import type { AppSettings } from '@/types/settings'

export const fakeSettings: AppSettings = {
  videoCodec: 'libx265',
  audioCodec: 'libopus',
  crf: 23,
  preset: 'medium',
  targetVmaf: 94,
}
