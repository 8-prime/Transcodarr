export interface AppSettings {
  videoCodec: 'libx265' | 'hevc_nvenc' | 'hevc_qsv'
  audioCodec: 'libopus' | 'aac' | 'copy'
  crf: number
  preset:
    | 'ultrafast'
    | 'superfast'
    | 'veryfast'
    | 'faster'
    | 'fast'
    | 'medium'
    | 'slow'
    | 'slower'
    | 'veryslow'
  targetVmaf: number
}
