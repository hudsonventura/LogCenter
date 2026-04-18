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
import { format } from "date-fns";
import React from "react";
import { useState } from "react";

import JsonView from "@uiw/react-json-view";
import { toast } from "sonner";

type LogDetails = {
  message?: string;
  timestamp?: string;
  level?: number;
  content?: unknown;
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

  const parsedDate = new Date(value);

  if (Number.isNaN(parsedDate.getTime())) {
    return String(value).replace("T", " ");
  }

  return format(parsedDate, "yyyy/MM/dd HH:mm:ss.SSS");
};

export function ModalObject({
  id,
  tableName,
  isOpen,
  onOpenChange,
}: {
  id: string;
  tableName: string;
  isOpen: boolean;
  onOpenChange: (isOpen: boolean) => void;
}) {
  const [data, setData] = useState<LogDetails>({});

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
  }, [id, tableName]);

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
      <DialogContent className="max-w-[1080px] min-w-[300px]">
        <DialogHeader>
          <DialogTitle>{data.message ?? "Log details"}</DialogTitle>
          <div className="flex items-center gap-3 text-sm text-muted-foreground">
            <span>{formatTimestamp(data.timestamp)}</span>
            {levelLabel && levelClass ? (
              <Badge className={`min-w-[76px] justify-center ${levelClass}`}>
                {levelLabel}
              </Badge>
            ) : null}
          </div>

          <DialogDescription className="w-full">
            <div
              style={{
                maxHeight: "400px", // Definindo a altura máxima para ativar o scroll
                overflow: "auto", // Habilita o scroll vertical se o conteúdo exceder a altura
                marginTop: "1em",
              }}
              className="min-w-full"
            >
              {!jsonValue ? (
                <pre style={{ whiteSpace: "pre-wrap" }}>{String(data.content)}</pre>
              ) : (
                <JsonView 
                  value={jsonValue} 
                  shortenTextAfterLength={1000}
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
