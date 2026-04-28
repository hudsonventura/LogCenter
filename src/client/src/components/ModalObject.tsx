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
import { getLogLevelBadgeClass, getLogLevelLabel } from "@/lib/log-levels";

type LogDetails = {
  message?: string;
  timestamp?: string;
  level?: number;
  content?: unknown;
  traceId?: string;
};

type RequestPayload = {
  method?: string;
  Method?: string;
  headers?: Record<string, unknown>;
  Headers?: Record<string, unknown>;
  host?: string;
  Host?: string;
  path?: string;
  Path?: string;
  completeURL?: string;
  CompleteURL?: string;
  query?: Record<string, unknown>;
  Query?: Record<string, unknown>;
  body?: unknown;
  Body?: unknown;
};



const formatTimestamp = (value?: string) => {
  if (!value) {
    return "-";
  }
  return String(value).replace("T", " ").replace(/Z$/, "");
};

const shellQuote = (value: string) => `'${value.replace(/'/g, "'\\''")}'`;

const ignoredCurlHeaders = new Set([
  "connection",
  "content-length",
  "host",
  "transfer-encoding",
]);

const ignoredGeneratedHttpHeaders = new Set([
  "connection",
  "content-length",
  "host",
  "transfer-encoding",
]);

const normalizeObject = (value: unknown): Record<string, unknown> | null => {
  if (typeof value === "object" && value !== null && !Array.isArray(value)) {
    return value as Record<string, unknown>;
  }

  if (typeof value === "string") {
    try {
      const parsed = JSON.parse(value);
      if (typeof parsed === "object" && parsed !== null && !Array.isArray(parsed)) {
        return parsed as Record<string, unknown>;
      }
    } catch {
      return null;
    }
  }

  return null;
};

const getRequestPayload = (content: unknown): RequestPayload | null => {
  const payload = normalizeObject(content) as RequestPayload | null;

  if (!payload) {
    return null;
  }

  const method = payload.method ?? payload.Method;
  const completeURL = payload.completeURL ?? payload.CompleteURL;

  if (typeof method !== "string" || typeof completeURL !== "string") {
    return null;
  }

  return payload;
};

const stringifyBody = (body: unknown): string | null => {
  if (body === undefined || body === null || body === "null") {
    return null;
  }

  if (typeof body === "string") {
    return body.length > 0 ? body : null;
  }

  return JSON.stringify(body);
};

const getHeaderValue = (headers: Record<string, unknown>, headerName: string) => {
  const entry = Object.entries(headers).find(
    ([key]) => key.toLowerCase() === headerName.toLowerCase()
  );

  return entry?.[1];
};

const resolveRequestTarget = (request: RequestPayload) => {
  const path = request.path ?? request.Path;
  const query = request.query ?? request.Query;

  if (typeof path === "string" && path.length > 0) {
    const params = new URLSearchParams();

    if (query && typeof query === "object") {
      Object.entries(query).forEach(([key, value]) => {
        if (value !== undefined && value !== null && String(value).length > 0) {
          params.set(key, String(value));
        }
      });
    }

    const queryString = params.toString();
    return queryString ? `${path}?${queryString}` : path;
  }

  const completeURL = String(request.completeURL ?? request.CompleteURL);

  try {
    const url = new URL(completeURL);
    return `${url.pathname}${url.search}`;
  } catch {
    return completeURL;
  }
};

const resolveHost = (request: RequestPayload) => {
  const host = request.host ?? request.Host;

  if (typeof host === "string" && host.length > 0) {
    return host;
  }

  const completeURL = String(request.completeURL ?? request.CompleteURL);

  try {
    return new URL(completeURL).host;
  } catch {
    return "";
  }
};

const generateCurlCommand = (request: RequestPayload) => {
  const method = String(request.method ?? request.Method).toUpperCase();
  const completeURL = String(request.completeURL ?? request.CompleteURL);
  const headers = request.headers ?? request.Headers ?? {};
  const body = stringifyBody(request.body ?? request.Body);
  const hasContentType = Object.keys(headers).some(
    (key) => key.toLowerCase() === "content-type"
  );

  const lines = [`curl --request ${shellQuote(method)} ${shellQuote(completeURL)}`];

  Object.entries(headers).forEach(([key, value]) => {
    if (ignoredCurlHeaders.has(key.toLowerCase()) || value === undefined || value === null) {
      return;
    }

    lines.push(`  --header ${shellQuote(`${key}: ${String(value)}`)}`);
  });

  if (body !== null && !hasContentType) {
    lines.push(`  --header ${shellQuote("Content-Type: application/json")}`);
  }

  if (body !== null) {
    lines.push(`  --data-raw ${shellQuote(body)}`);
  }

  return lines.join(" \\\n");
};

const generateHttpRequest = (request: RequestPayload) => {
  const method = String(request.method ?? request.Method).toUpperCase();
  const headers = request.headers ?? request.Headers ?? {};
  const body = stringifyBody(request.body ?? request.Body);
  const requestTarget = resolveRequestTarget(request);
  const host = resolveHost(request);
  const contentType = getHeaderValue(headers, "content-type");
  const lines = [`${method} ${requestTarget} HTTP/1.1`];

  if (host) {
    lines.push(`Host: ${host}`);
  }

  Object.entries(headers).forEach(([key, value]) => {
    if (
      ignoredGeneratedHttpHeaders.has(key.toLowerCase()) ||
      value === undefined ||
      value === null
    ) {
      return;
    }

    lines.push(`${key}: ${String(value)}`);
  });

  if (body !== null && !contentType) {
    lines.push("Content-Type: application/json");
  }

  if (body !== null) {
    lines.push(`Content-Length: ${new TextEncoder().encode(body).length}`);
    lines.push("");
    lines.push(body);
  }

  return lines.join("\r\n");
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
  const [isCurlModalOpen, setIsCurlModalOpen] = useState(false);
  const [isHttpRequestModalOpen, setIsHttpRequestModalOpen] = useState(false);
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

  const requestPayload =
    data.level === 99 ? getRequestPayload(data.content) : null;
  const curlCommand = requestPayload ? generateCurlCommand(requestPayload) : "";
  const httpRequest = requestPayload ? generateHttpRequest(requestPayload) : "";

  const handleCopyCurl = async () => {
    try {
      await navigator.clipboard.writeText(curlCommand);

      toast("Copied!", {
        description: "The curl command was copied to clipboard",
      });
    } catch (error) {
      console.log(error);
    }
  };

  const handleCopyHttpRequest = async () => {
    try {
      await navigator.clipboard.writeText(httpRequest);

      toast("Copied!", {
        description: "The HTTP request was copied to clipboard",
      });
    } catch (error) {
      console.log(error);
    }
  };

  let jsonValue: object | undefined;
  const levelLabel =
    typeof data.level === "number" ? getLogLevelLabel(data.level) : null;
  const levelClass =
    typeof data.level === "number"
      ? getLogLevelBadgeClass(data.level)
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
          {requestPayload ? (
            <>
              <Button variant="outline" onClick={() => setIsCurlModalOpen(true)}>
                Generate curl
              </Button>
              <Button variant="outline" onClick={() => setIsHttpRequestModalOpen(true)}>
                Generate HTTP request
              </Button>
            </>
          ) : null}
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
      <Dialog open={isCurlModalOpen} onOpenChange={setIsCurlModalOpen}>
        <DialogContent className="w-[calc(100vw-2rem)] max-w-[900px]">
          <DialogHeader>
            <DialogTitle>Generated curl</DialogTitle>
            <DialogDescription>
              Copy this command to replay the captured HTTP request.
            </DialogDescription>
          </DialogHeader>
          <pre className="max-h-[420px] overflow-auto rounded-md border border-border/70 bg-slate-950 p-4 text-sm leading-6 text-slate-50">
            {curlCommand}
          </pre>
          <DialogFooter>
            <Button variant="outline" onClick={handleCopyCurl}>
              Copy curl
            </Button>
            <DialogClose asChild>
              <Button type="button" variant="secondary">
                Close
              </Button>
            </DialogClose>
          </DialogFooter>
        </DialogContent>
      </Dialog>
      <Dialog open={isHttpRequestModalOpen} onOpenChange={setIsHttpRequestModalOpen}>
        <DialogContent className="w-[calc(100vw-2rem)] max-w-[900px]">
          <DialogHeader>
            <DialogTitle>Generated HTTP request</DialogTitle>
            <DialogDescription>
              Copy this raw HTTP/1.1 request to replay the captured request over a socket.
            </DialogDescription>
          </DialogHeader>
          <pre className="max-h-[420px] overflow-auto rounded-md border border-border/70 bg-slate-950 p-4 text-sm leading-6 text-slate-50">
            {httpRequest}
          </pre>
          <DialogFooter>
            <Button variant="outline" onClick={handleCopyHttpRequest}>
              Copy HTTP request
            </Button>
            <DialogClose asChild>
              <Button type="button" variant="secondary">
                Close
              </Button>
            </DialogClose>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </Dialog>
  );
}
