import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import api from "@/services/api";
import { ChevronLeft, ChevronRight } from "lucide-react";
import React from "react";
import { useState } from "react";
import { useTimezone } from "./timezone-provider";

import JsonView from "@uiw/react-json-view";
import { darkTheme } from "@uiw/react-json-view/dark";
import { lightTheme } from "@uiw/react-json-view/light";
import { useTheme } from "next-themes";
import { toast } from "sonner";

type LogDetails = {
  message?: string;
  timestamp?: string;
  level?: number;
  content?: unknown;
  traceId?: string;
};

const levelLabels: Record<number, string> = {
  0: "Trace",
  1: "Info",
  2: "Debug",
  3: "Warning",
  4: "Error",
  5: "Critical",
  6: "Success",
  7: "Fatal",
};

const levelBadgeClass: Record<number, string> = {
  0: "bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-100",
  1: "bg-sky-100 text-sky-800 dark:bg-sky-500/15 dark:text-sky-300",
  2: "bg-zinc-100 text-zinc-800 dark:bg-zinc-800 dark:text-zinc-100",
  3: "bg-amber-100 text-amber-800 dark:bg-amber-500/15 dark:text-amber-300",
  4: "bg-rose-100 text-rose-800 dark:bg-rose-500/15 dark:text-rose-300",
  5: "bg-red-600 text-white dark:bg-red-700 dark:text-red-50",
  6: "bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300",
  7: "bg-red-700 text-white dark:bg-red-800 dark:text-red-50",
};

const formatTimestamp = (value?: string) => {
  if (!value) {
    return "-";
  }
  return String(value).replace("T", " ").replace(/Z$/, "");
};

export function ModalObject({
  id,
  tableName,
  isOpen,
  onOpenChange,
  onPrevious,
  onNext,
  hasPrevious = false,
  hasNext = false,
}: {
  id: string;
  tableName: string;
  isOpen: boolean;
  onOpenChange: (isOpen: boolean) => void;
  onPrevious?: () => void;
  onNext?: () => void;
  hasPrevious?: boolean;
  hasNext?: boolean;
}) {
  const [data, setData] = useState<LogDetails>({});
  const { timezone } = useTimezone();
  const { resolvedTheme = "light" } = useTheme();
  const isDark = resolvedTheme === "dark";

  React.useEffect(() => {
    const getObject = async () => {
      try {
        const response = await api.get(`/${tableName}/${id}`);
        setData(response.data ?? {});
      } catch (error) {
        console.log(error);
      }
    };

    void getObject();
  }, [id, tableName, timezone]);

  const handleCopy = async () => {
    try {
      const jsonStr = JSON.stringify(data.content, null, 2);
      await navigator.clipboard.writeText(jsonStr);

      toast("Copied!", {
        description: "The content was copied to clipboar",
      });
    } catch (error) {
      console.log(error);
    }
  };

  let jsonValue: object | undefined;
  const levelLabel =
    typeof data.level === "number" ? levelLabels[data.level] ?? "Unknown" : null;
  const levelClass =
    typeof data.level === "number"
      ? levelBadgeClass[data.level] ?? levelBadgeClass[0]
      : null;
  const jsonTheme = isDark
    ? {
        ...darkTheme,
        "--w-rjv-background-color": "transparent",
        "--w-rjv-color": "#e5edf7",
        "--w-rjv-line-color": "rgba(148, 163, 184, 0.18)",
        "--w-rjv-arrow-color": "#cbd5e1",
        "--w-rjv-info-color": "#cbd5e1",
        "--w-rjv-curlybraces-color": "#f8fafc",
        "--w-rjv-colon-color": "#f8fafc",
        "--w-rjv-brackets-color": "#f8fafc",
        "--w-rjv-key-string": "#7dd3fc",
        "--w-rjv-key-number": "#f0abfc",
        "--w-rjv-type-string-color": "#fdba74",
        "--w-rjv-type-int-color": "#86efac",
        "--w-rjv-type-float-color": "#86efac",
        "--w-rjv-type-bigint-color": "#86efac",
        "--w-rjv-type-boolean-color": "#93c5fd",
        "--w-rjv-type-date-color": "#86efac",
        "--w-rjv-type-url-color": "#93c5fd",
        "--w-rjv-type-null-color": "#93c5fd",
        "--w-rjv-type-nan-color": "#fcd34d",
        "--w-rjv-type-undefined-color": "#93c5fd",
      }
    : {
        ...lightTheme,
        "--w-rjv-background-color": "transparent",
      };

  if (typeof data.content === "object" && data.content !== null) {
    jsonValue = data.content as object;
  } else if (typeof data.content === "string") {
    try {
      const parsed = JSON.parse(data.content);
      if (typeof parsed === "object" && parsed !== null) {
        jsonValue = parsed as object;
      }
    } catch {
      jsonValue = undefined;
    }
  }

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="w-[calc(100vw-2rem)] max-w-[1080px] min-w-[300px] overflow-hidden px-14">
        <Button
          type="button"
          variant="outline"
          size="icon"
          className="absolute left-2 top-1/2 z-10 h-10 w-10 -translate-y-1/2 rounded-full"
          onClick={onPrevious}
          disabled={!hasPrevious}
          aria-label="Previous log"
        >
          <ChevronLeft className="h-5 w-5" />
        </Button>
        <Button
          type="button"
          variant="outline"
          size="icon"
          className="absolute right-2 top-1/2 z-10 h-10 w-10 -translate-y-1/2 rounded-full"
          onClick={onNext}
          disabled={!hasNext}
          aria-label="Next log"
        >
          <ChevronRight className="h-5 w-5" />
        </Button>
        <DialogHeader className="min-w-0">
          
          <DialogTitle>
            {levelLabel && levelClass ? (
              <Badge className={`min-w-[66px] justify-center ${levelClass}`}>
                {levelLabel}
              </Badge>
              
            ) : null} 
              <span className="ml-2">
             {data.message ?? "Log details"}
              </span>
            </DialogTitle>
          <span>TraceId: 
            {data.traceId !== null ?(
              <span> {data.traceId}</span>
            ): " -"}
          </span>
          <div className="flex items-center gap-3 text-sm text-muted-foreground">
            <span>{formatTimestamp(data.timestamp)}</span>
            
            
          </div>
          

          <DialogDescription className="w-full min-w-0">
            <div
              style={{
                maxHeight: "400px", // Definindo a altura máxima para ativar o scroll
                marginTop: "1em",
              }}
              className="w-full min-w-0 max-w-full overflow-x-auto overflow-y-auto rounded-md border border-border/70 bg-slate-50/70 p-4 text-slate-900 dark:border-slate-700 dark:bg-slate-900/70 dark:text-slate-50"
            >
              {!jsonValue ? (
                <pre
                  style={{ whiteSpace: "pre-wrap" }}
                  className="w-full min-w-0 max-w-full text-sm leading-6 text-slate-900 dark:text-slate-50"
                >
                  {String(data.content)}
                </pre>
              ) : (
                <JsonView
                  className="max-w-full min-w-0"
                  value={jsonValue}
                  style={jsonTheme}
                  shortenTextAfterLength={1000}
                  displayDataTypes={false}
                />
              )}
            </div>
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={handleCopy}>
            Copy Content
          </Button>
          <DialogClose asChild onClick={() => onOpenChange(false)}>
            <Button type="button" variant="secondary">
              Close
            </Button>
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
