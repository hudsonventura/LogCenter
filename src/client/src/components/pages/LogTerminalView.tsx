"use client";

import * as React from "react";
import { useLocation, useNavigate } from "react-router-dom";

import EnsureLogin from "@/components/EnsureLogin";
import HeaderBar from "@/components/HeaderBar";
import { Button } from "@/components/ui/button";
import api from "@/services/api";
import { ArrowLeft } from "lucide-react";
import { toast } from "sonner";
import type { LogRecord } from "./TableLogs";

const formatTimestamp = (value: string) => {
  return String(value)
    .replace("T", " ")
    .replace(/(\.\d{3})\d+/, "$1")
    .replace(/Z$/, "");
};

const formatContent = (content: unknown) => {
  if (content === undefined || content === null || content === "") {
    return null;
  }

  if (typeof content === "string") {
    const trimmed = content.trim();

    if (!trimmed || trimmed === "null") {
      return null;
    }

    try {
      return JSON.stringify(JSON.parse(trimmed), null, 2);
    } catch {
      return content;
    }
  }

  return JSON.stringify(content, null, 2);
};

export function LogTerminalView() {
  const navigate = useNavigate();
  const location = useLocation();
  const params = React.useMemo(() => new URLSearchParams(location.search), [location.search]);
  const tableName = params.get("tabela");
  const [records, setRecords] = React.useState<LogRecord[]>([]);
  const [expandedRecords, setExpandedRecords] = React.useState<Set<string>>(() => new Set());
  const [isLoading, setIsLoading] = React.useState(false);

  React.useEffect(() => {
    const loadRecords = async () => {
      if (!tableName) {
        setRecords([]);
        setExpandedRecords(new Set());
        return;
      }

      setIsLoading(true);

      try {
        const requestParams = new URLSearchParams(params);
        requestParams.set("bring_content", "true");

        const response = await api.get<LogRecord[]>(
          `/${tableName}?${requestParams.toString()}`
        );
        const nextRecords = Array.isArray(response.data) ? response.data : [];

        setRecords(
          nextRecords.sort(
            (left, right) =>
              new Date(left.timestamp).getTime() - new Date(right.timestamp).getTime()
          )
        );
        setExpandedRecords(new Set());
      } catch (error) {
        console.log(error);
        setRecords([]);
        setExpandedRecords(new Set());
        toast.error("Error loading terminal log view");
      } finally {
        setIsLoading(false);
      }
    };

    void loadRecords();
  }, [params, tableName]);

  return (
    <>
      <HeaderBar />
      <EnsureLogin />
      <main className="mx-auto flex w-full max-w-none flex-col gap-4 px-4 py-5 lg:px-6">
        <div className="flex items-center gap-3">
          <Button
            type="button"
            variant="outline"
            size="icon"
            aria-label="Back to table logs"
            onClick={() => navigate(`/table-logs?${params.toString()}`)}
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-semibold tracking-tight">
              Terminal view {tableName ? `from ${tableName}` : ""}
            </h1>
            <p className="text-sm text-muted-foreground">
              Older records at the top, newer records at the bottom.
            </p>
          </div>
        </div>

        <section className="min-h-[70vh] rounded-xl border border-slate-800 bg-[#080d12] p-4 shadow-2xl">
          <div
            className="max-h-[72vh] overflow-auto pr-2 text-[12px] leading-5 text-slate-100"
            style={{ fontFamily: "Consolas, 'Cascadia Code', 'Courier New', monospace" }}
          >
            {isLoading ? (
              <div className="text-slate-400">Loading logs...</div>
            ) : records.length === 0 ? (
              <div className="text-slate-400">No logs found.</div>
            ) : (
              records.map((record) => {
                const content = record.hideContentWhenMessageIsRendered
                  ? null
                  : formatContent(record.content);
                const isExpanded = expandedRecords.has(record.id);

                return (
                  <article key={record.id} className="border-b border-slate-800/80 py-1 last:border-b-0">
                    <div className="whitespace-pre-wrap break-words">
                      <span className="text-emerald-300">{formatTimestamp(record.timestamp)}</span>
                      <span className="text-slate-500"> | </span>
                      <span>{record.message}</span>
                      {content ? (
                        <button
                          type="button"
                          className="ml-2 rounded border border-slate-700 px-1.5 text-[11px] leading-4 text-slate-300 hover:border-slate-500 hover:text-slate-100"
                          onClick={() => {
                            setExpandedRecords((current) => {
                              const next = new Set(current);

                              if (next.has(record.id)) {
                                next.delete(record.id);
                              } else {
                                next.add(record.id);
                              }

                              return next;
                            });
                          }}
                        >
                          {isExpanded ? "Hide" : "Show content"}
                        </button>
                      ) : null}
                    </div>
                    {content && isExpanded ? (
                      <pre className="whitespace-pre-wrap break-words pl-6 text-slate-300">
                        {content}
                      </pre>
                    ) : null}
                  </article>
                );
              })
            )}
          </div>
        </section>
      </main>
    </>
  );
}
