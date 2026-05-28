export interface AppSettings {
  videoCodec: 'H264' | 'H265' | 'Av1'
  audioCodec: 'Aac' | 'Ac3' | 'Copy'
  preset:
    | 'Ultrafast'
    | 'Superfast'
    | 'Veryfast'
    | 'Faster'
    | 'Fast'
    | 'Medium'
    | 'Slow'
    | 'Slower'
    | 'Veryslow'
  crf: number
  autoApplyTranscode: boolean
  jobExpirationInMinutes: number
  transcodeTempDirectory: string
}
