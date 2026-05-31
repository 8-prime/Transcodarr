import { useState } from 'react'
import { Save } from 'lucide-react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { PageHeader } from '@/components/common/PageHeader'
import { MonoText } from '@/components/common/MonoText'
import { Button } from '@/components/ui/button'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Slider } from '@/components/ui/slider'
import { Separator } from '@/components/ui/separator'
import { Input } from '@/components/ui/input'
import { apiFetch } from '@/lib/api'
import type { AppSettings } from '@/types/settings'

const PRESETS: AppSettings['preset'][] = [
  'Ultrafast',
  'Superfast',
  'Veryfast',
  'Faster',
  'Fast',
  'Medium',
  'Slow',
  'Slower',
  'Veryslow',
]

const DEFAULT_SETTINGS: AppSettings = {
  videoCodec: "H265",
  audioCodec: "Copy",
  preset: "Slower",
  crf: 20,
  autoApplyTranscode: true,
  jobExpirationInMinutes: 30,
  transcodeTempDirectory: '/tmp/transcodarr/'
}

export function SettingsPage() {
  const queryClient = useQueryClient()
  const { data: settings } = useQuery({
    queryKey: ['settings'],
    queryFn: async () => {
      try{
        return apiFetch<AppSettings>('/settings')
      }
      catch (e) {
        if((e as {status?: number}).status === 404) return null
        throw e
      }
    },
    retry: (count, e) =>
      (e as {status?: number}).status === 404 ? false : count < 3
  })

  const [draft, setDraft] = useState<AppSettings | null>(null)
  const current = draft ?? settings ?? DEFAULT_SETTINGS
  const needsInit = settings === undefined

  const { mutate: save, isPending } = useMutation({
    mutationFn: (s: AppSettings) =>
      apiFetch<void>('/settings', { method: 'PUT', body: JSON.stringify(s) }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['settings'] })
      setDraft(null)
    },
  })

  if (!current) return null

  return (
    <div className="space-y-6">
      <PageHeader
        title="Settings"
        description="Encoder defaults applied to every queued job."
        actions={
          <Button size="sm" disabled={(!draft && !needsInit) || isPending} onClick={() => save(current)}>
            <Save className="h-3.5 w-3.5" />
            Save
          </Button>
        }
      />

      <div className="rounded-lg border border-border bg-card">
        <SettingsRow
          label="Video codec"
          description="Codec family applied to every job. The node selects the best available encoder."
        >
          <Select
            value={current.videoCodec}
            onValueChange={(v) =>
              setDraft({ ...current, videoCodec: v as AppSettings['videoCodec'] })
            }
          >
            <SelectTrigger className="w-[260px] font-mono">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="H265" className="font-mono text-sm">H.265 / HEVC</SelectItem>
              <SelectItem value="H264" className="font-mono text-sm">H.264 / AVC</SelectItem>
              <SelectItem value="Av1" className="font-mono text-sm">AV1</SelectItem>
            </SelectContent>
          </Select>
        </SettingsRow>

        <Separator />

        <SettingsRow
          label="Audio codec"
          description="Audio track encoder. 'Copy' passes through losslessly."
        >
          <Select
            value={current.audioCodec}
            onValueChange={(v) =>
              setDraft({ ...current, audioCodec: v as AppSettings['audioCodec'] })
            }
          >
            <SelectTrigger className="w-[260px] font-mono">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="Aac" className="font-mono text-sm">AAC</SelectItem>
              <SelectItem value="Ac3" className="font-mono text-sm">AC3 / Dolby Digital</SelectItem>
              <SelectItem value="Copy" className="font-mono text-sm">Copy (passthrough)</SelectItem>
            </SelectContent>
          </Select>
        </SettingsRow>

        <Separator />

        <SettingsRow
          label="CRF"
          description="Constant-rate factor. Lower = higher quality, larger files."
        >
          <div className="flex w-[260px] items-center gap-4">
            <Slider
              min={18}
              max={32}
              step={1}
              value={[current.crf]}
              onValueChange={(v) => setDraft({ ...current, crf: v[0] })}
            />
            <MonoText className="w-8 text-right text-foreground">{current.crf}</MonoText>
          </div>
        </SettingsRow>

        <Separator />

        <SettingsRow
          label="Preset"
          description="Encoder speed vs. compression efficiency trade-off."
        >
          <Select
            value={current.preset}
            onValueChange={(v) =>
              setDraft({ ...current, preset: v as AppSettings['preset'] })
            }
          >
            <SelectTrigger className="w-[260px] font-mono">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {PRESETS.map((p) => (
                <SelectItem key={p} value={p} className="font-mono text-sm">
                  {p.toLowerCase()}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </SettingsRow>

        <Separator />

        <SettingsRow
          label="Temp directory"
          description="Directory where in-progress transcode files are written before being moved to their final location."
        >
          <Input
            className="w-[260px] font-mono text-sm"
            value={current.transcodeTempDirectory}
            onChange={(e) =>
              setDraft({ ...current, transcodeTempDirectory: e.target.value })
            }
            placeholder="/tmp/transcodarr/"
          />
        </SettingsRow>
      </div>
    </div>
  )
}

function SettingsRow({
  label,
  description,
  children,
}: {
  label: string
  description?: string
  children: React.ReactNode
}) {
  return (
    <div className="flex flex-col gap-3 px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <div className="text-sm font-medium text-foreground">{label}</div>
        {description && (
          <p className="mt-0.5 max-w-md text-xs text-muted-foreground">
            {description}
          </p>
        )}
      </div>
      <div className="shrink-0">{children}</div>
    </div>
  )
}
