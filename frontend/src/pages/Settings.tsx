import { useState } from 'react'
import { Save } from 'lucide-react'
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
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip'
import { fakeSettings } from '@/api/fakes/settings'
import { ENCODER_LABELS } from '@/types/node'
import type { AppSettings } from '@/types/settings'

const PRESETS: AppSettings['preset'][] = [
  'ultrafast',
  'superfast',
  'veryfast',
  'faster',
  'fast',
  'medium',
  'slow',
  'slower',
  'veryslow',
]

export function SettingsPage() {
  const [draft, setDraft] = useState<AppSettings>(fakeSettings)

  return (
    <div className="space-y-6">
      <PageHeader
        kicker="Placeholder · backend not wired yet"
        title="Settings"
        description="Encoder defaults applied to every queued job. Changes live in memory only for now."
        actions={
          <Tooltip>
            <TooltipTrigger asChild>
              <span>
                <Button size="sm" disabled>
                  <Save className="h-3.5 w-3.5" />
                  Save
                </Button>
              </span>
            </TooltipTrigger>
            <TooltipContent>Persistence endpoint not wired yet</TooltipContent>
          </Tooltip>
        }
      />

      <div className="rounded-lg border border-border bg-card">
        <SettingsRow
          label="Video codec"
          description="Encoder family applied to every job."
        >
          <Select
            value={draft.videoCodec}
            onValueChange={(v) =>
              setDraft({ ...draft, videoCodec: v as AppSettings['videoCodec'] })
            }
          >
            <SelectTrigger className="w-[260px] font-mono">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {(['libx265', 'hevc_nvenc', 'hevc_qsv'] as const).map((c) => (
                <SelectItem key={c} value={c} className="font-mono text-sm">
                  {ENCODER_LABELS[c]}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </SettingsRow>

        <Separator />

        <SettingsRow
          label="Audio codec"
          description="Audio track encoder. 'copy' passes through losslessly."
        >
          <Select
            value={draft.audioCodec}
            onValueChange={(v) =>
              setDraft({ ...draft, audioCodec: v as AppSettings['audioCodec'] })
            }
          >
            <SelectTrigger className="w-[260px] font-mono">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="libopus" className="font-mono text-sm">
                libopus
              </SelectItem>
              <SelectItem value="aac" className="font-mono text-sm">
                aac
              </SelectItem>
              <SelectItem value="copy" className="font-mono text-sm">
                copy (passthrough)
              </SelectItem>
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
              value={[draft.crf]}
              onValueChange={(v) => setDraft({ ...draft, crf: v[0] })}
            />
            <MonoText className="w-8 text-right text-foreground">{draft.crf}</MonoText>
          </div>
        </SettingsRow>

        <Separator />

        <SettingsRow
          label="Preset"
          description="Encoder speed vs. compression efficiency trade-off."
        >
          <Select
            value={draft.preset}
            onValueChange={(v) =>
              setDraft({ ...draft, preset: v as AppSettings['preset'] })
            }
          >
            <SelectTrigger className="w-[260px] font-mono">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {PRESETS.map((p) => (
                <SelectItem key={p} value={p} className="font-mono text-sm">
                  {p}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </SettingsRow>

        <Separator />

        <SettingsRow
          label="Target VMAF"
          description="Quality floor. Jobs below this score are flagged in history."
        >
          <div className="flex w-[260px] items-center gap-4">
            <Slider
              min={80}
              max={99}
              step={1}
              value={[draft.targetVmaf]}
              onValueChange={(v) => setDraft({ ...draft, targetVmaf: v[0] })}
            />
            <MonoText className="w-8 text-right text-foreground">
              {draft.targetVmaf}
            </MonoText>
          </div>
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
