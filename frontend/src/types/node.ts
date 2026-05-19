export const ENCODER_LABELS: Record<string, string> = {
  libx265: 'CPU · x265',
  hevc_nvenc: 'NVIDIA · NVENC',
  hevc_qsv: 'Intel · QSV',
}

export interface EncoderCapability {
  slots: number
  encoderName: string
}

export interface NodeInfo {
  name: string
  encoderCapabilities: EncoderCapability[]
}

export interface NodeConnectionInfo {
  connectionId: string
  nodeInfo: NodeInfo | null
  freeSlots: number
  connectionIsReady: boolean
}
