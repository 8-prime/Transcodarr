import { useState } from "react";
import { ArrowLeft, ChevronRight, Folder, FolderOpen } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import { apiFetch } from "@/lib/api";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import type { FilesystemBrowseResponse } from "@/types/filesystem";

interface DirectoryPickerProps {
  value: string;
  onChange: (path: string) => void;
}

// Extract the display label from a path without relying on a specific separator.
function getPathLabel(path: string): string {
  if (!path) return "/";
  const trimmed = path.replace(/[/\\]+$/, "");
  const lastSep = Math.max(trimmed.lastIndexOf("/"), trimmed.lastIndexOf("\\"));
  return lastSep >= 0 ? trimmed.slice(lastSep + 1) : trimmed;
}

// Build an initial nav stack from a pre-existing value so breadcrumbs and .. work
// when the picker is opened with a path already selected. Works for Unix paths;
// Windows paths fall back to a single root entry.
function buildInitialStack(path: string): string[] {
  if (!path) return [];
  const normalized = path.replace(/\\/g, "/");
  const trimmed = normalized.endsWith("/") ? normalized.slice(0, -1) : normalized;
  if (!trimmed.startsWith("/")) return [""];
  const parts = trimmed.slice(1).split("/").filter(Boolean);
  if (parts.length === 0) return [];
  const stack: string[] = [""];
  for (let i = 0; i < parts.length - 1; i++) {
    stack.push("/" + parts.slice(0, i + 1).join("/") + "/");
  }
  return stack;
}

export function DirectoryPicker({ value, onChange }: DirectoryPickerProps) {
  const [open, setOpen] = useState(false);
  const [browsePath, setBrowsePath] = useState("");
  const [navStack, setNavStack] = useState<string[]>([]);

  function handleOpen() {
    const initial = value || "";
    setBrowsePath(initial);
    setNavStack(buildInitialStack(initial));
    setOpen(true);
  }

  function navigateInto(path: string) {
    setNavStack((prev) => [...prev, browsePath]);
    setBrowsePath(path);
  }

  function navigateUp() {
    const parent = navStack[navStack.length - 1] ?? "";
    setNavStack((prev) => prev.slice(0, -1));
    setBrowsePath(parent);
  }

  function navigateToCrumb(stackIndex: number, path: string) {
    setNavStack((prev) => prev.slice(0, stackIndex));
    setBrowsePath(path);
  }

  const { data, isLoading, isError } = useQuery({
    queryKey: ["filesystem", "browse", browsePath],
    queryFn: () =>
      apiFetch<FilesystemBrowseResponse>(
        `/filesystem/browse${browsePath ? `?path=${encodeURIComponent(browsePath)}` : ""}`,
      ),
    enabled: open,
  });

  const allPaths = [...navStack, browsePath];
  const breadcrumbs = allPaths.map((p, i) => ({
    label: getPathLabel(p),
    path: p,
    stackIndex: i,
  }));
  const canGoUp = navStack.length > 0;

  function handleSelect() {
    onChange(browsePath);
    setOpen(false);
  }

  return (
    <>
      <div className="flex gap-2">
        <Input
          placeholder="/mnt/media/movies"
          value={value}
          onChange={(e) => onChange(e.target.value)}
          className="font-mono text-sm"
        />
        <Button
          type="button"
          variant="outline"
          size="icon"
          onClick={handleOpen}
          title="Browse filesystem"
        >
          <FolderOpen className="h-4 w-4" />
        </Button>
      </div>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="max-w-2xl gap-0 p-0">
          <DialogHeader className="border-b px-4 py-3">
            <DialogTitle>Browse directories</DialogTitle>
          </DialogHeader>

          {/* Breadcrumb */}
          <div className="flex items-center gap-0.5 overflow-x-auto border-b bg-muted/30 px-3 py-2">
            {breadcrumbs.map((crumb, i) => (
              <div key={i} className="flex items-center gap-0.5 shrink-0">
                {i > 0 && (
                  <ChevronRight className="h-3 w-3 text-muted-foreground/50" />
                )}
                <button
                  type="button"
                  onClick={() => navigateToCrumb(crumb.stackIndex, crumb.path)}
                  className={cn(
                    "rounded px-1.5 py-0.5 font-mono text-xs transition-colors hover:bg-accent hover:text-accent-foreground",
                    i === breadcrumbs.length - 1
                      ? "text-foreground"
                      : "text-muted-foreground",
                  )}
                >
                  {crumb.label}
                </button>
              </div>
            ))}
          </div>

          {/* Directory listing */}
          <ScrollArea className="h-80">
            {isLoading && (
              <div className="flex h-full items-center justify-center py-12 text-sm text-muted-foreground">
                Loading…
              </div>
            )}
            {isError && (
              <div className="flex h-full items-center justify-center py-12 text-sm text-destructive">
                Failed to load directory contents
              </div>
            )}
            {data && (
              <div className="divide-y divide-border/50">
                {canGoUp && (
                  <button
                    type="button"
                    onClick={navigateUp}
                    className="flex w-full items-center gap-2.5 px-4 py-2.5 text-sm text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
                  >
                    <ArrowLeft className="h-4 w-4 shrink-0" />
                    <span className="font-mono text-xs">..</span>
                  </button>
                )}
                {data.directories.map((dir) => (
                  <button
                    key={dir.path}
                    type="button"
                    onClick={() => navigateInto(dir.path)}
                    className="flex w-full items-center gap-2.5 px-4 py-2.5 text-sm transition-colors hover:bg-accent hover:text-accent-foreground"
                  >
                    <Folder className="h-4 w-4 shrink-0 text-muted-foreground" />
                    <span className="font-mono text-xs truncate">
                      {dir.name}
                    </span>
                  </button>
                ))}
                {!canGoUp && data.directories.length === 0 && (
                  <div className="py-12 text-center text-sm text-muted-foreground">
                    Empty directory
                  </div>
                )}
              </div>
            )}
          </ScrollArea>

          <DialogFooter className="border-t">
            <Button
              type="button"
              variant="outline"
              onClick={() => setOpen(false)}
            >
              Cancel
            </Button>
            <Button type="button" onClick={handleSelect} disabled={!browsePath}>
              Select this folder
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
