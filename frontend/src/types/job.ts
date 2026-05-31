export type QueueItemState =
  | 'Discovered'
  | 'Pending'
  | 'Processing'
  | 'Completed'
  | 'Failed'
  | 'LeaseExpired'

export interface QueueItem {
  id: string
  fileName: string
  libraryName: string
  targetCodec: string
  state: QueueItemState
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
  vmafScore: number
  inputSizeBytes: number
  outputSizeBytes: number
  durationSec: number
  completedAt: string
}
