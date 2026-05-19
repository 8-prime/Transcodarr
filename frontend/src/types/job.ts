export type JobState =
  | 'Pending'
  | 'Assigned'
  | 'Processing'
  | 'Completed'
  | 'Failed'
  | 'Cancelled'
  | 'LeaseExpired'

export interface TranscodeJob {
  id: string
  fileName: string
  libraryName: string
  targetEncoder: string
  state: JobState
  attemptNumber: number
  createdAt: string
  nodeId: string | null
  progressPct: number | null
}

export interface CompletedJob {
  id: string
  fileName: string
  libraryName: string
  encoderUsed: string
  crf: number
  vmaf: number
  inputSizeBytes: number
  outputSizeBytes: number
  durationSec: number
  completedAt: string
}
