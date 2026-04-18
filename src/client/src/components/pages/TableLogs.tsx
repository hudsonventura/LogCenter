"use client";

import * as React from "react";
import { useLocation } from "react-router-dom";
import {
  ColumnDef,
  ColumnFiltersState,
  SortingState,
  VisibilityState,
  flexRender,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { ArrowUpDown, ChevronDown, MoreHorizontal } from "lucide-react";
import { format } from "date-fns";

import HeaderBar from "@/components/HeaderBar";
import EnsureLogin from "@/components/EnsureLogin";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { toast } from "sonner";

import api from "@/services/api";
import { ModalObject } from "../ModalObject";
import { DateTimePicker } from "../DateTimePicker";
import { TimeZoneSelect } from "../TimeZoneSelect";
import LogTimelineChart from "../charts/LogTimelineChart";

export type LogRecord = {
  id: string;
  level: RecordLevel;
  traceId: string | null;
  message: string;
  content: unknown;
  timestamp: string;
};

export enum RecordLevel {
  Trace = 0,
  Info = 1,
  Debug = 2,
  Warning = 3,
  Error = 4,
  Critical = 5,
  Success = 6,
  Fatal = 7,
}

const levelLabels: Record<number, string> = {
  [RecordLevel.Trace]: "Trace",
  [RecordLevel.Info]: "Info",
  [RecordLevel.Debug]: "Debug",
  [RecordLevel.Warning]: "Warning",
  [RecordLevel.Error]: "Error",
  [RecordLevel.Critical]: "Critical",
  [RecordLevel.Success]: "Success",
  [RecordLevel.Fatal]: "Fatal",
};

const levelBadgeClass: Record<number, string> = {
  [RecordLevel.Trace]: "bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-100",
  [RecordLevel.Info]: "bg-sky-100 text-sky-800 dark:bg-sky-500/15 dark:text-sky-300",
  [RecordLevel.Debug]: "bg-zinc-100 text-zinc-800 dark:bg-zinc-800 dark:text-zinc-100",
  [RecordLevel.Warning]: "bg-amber-100 text-amber-800 dark:bg-amber-500/15 dark:text-amber-300",
  [RecordLevel.Error]: "bg-rose-100 text-rose-800 dark:bg-rose-500/15 dark:text-rose-300",
  [RecordLevel.Critical]: "bg-red-600 text-white dark:bg-red-700 dark:text-red-50",
  [RecordLevel.Success]: "bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300",
  [RecordLevel.Fatal]: "bg-red-700 text-white dark:bg-red-800 dark:text-red-50",
};

const formatTimestamp = (value: string) => {
  const [datePart, microPart = "000000"] = String(value).split(".");
  const formattedDate = format(new Date(datePart), "yyyy/MM/dd HH:mm:ss");
  const fixedMicro = microPart.padEnd(6, "0");

  return `${formattedDate}.${fixedMicro}`;
};

export const columns: ColumnDef<LogRecord>[] = [
  {
    accessorKey: "level",
    header: "Level",
    cell: ({ row }) => (
      <Badge
        className={`min-w-[76px] justify-center ${levelBadgeClass[row.original.level] ?? levelBadgeClass[RecordLevel.Trace]}`}
      >
        {levelLabels[row.original.level] ?? "Unknown"}
      </Badge>
    ),
  },
  {
    accessorKey: "traceId",
    header: ({ column }) => (
      <Button
        variant="ghost"
        onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
      >
        Trace ID
        <ArrowUpDown className="ml-2 h-4 w-4" />
      </Button>
    ),
    cell: ({ row }) => (
      <div className="max-w-[220px] truncate lowercase">
        {row.original.traceId || "-"}
      </div>
    ),
  },
  {
    accessorKey: "timestamp",
    header: () => <div className="text-left">Timestamp</div>,
    cell: ({ row }) => (
      <div className="text-left font-mono text-xs sm:text-sm">
        {formatTimestamp(row.original.timestamp)}
      </div>
    ),
  },
  {
    accessorKey: "message",
    header: () => <div className="text-left">Message</div>,
    cell: ({ row }) => (
      <div className="max-w-[280px] whitespace-pre-wrap break-words text-left">
        {row.original.message}
      </div>
    ),
  },
  {
    accessorKey: "content",
    header: () => <div className="text-left">Content</div>,
    cell: ({ row }) => {
      const { content } = row.original;

      if (typeof content === "string") {
        return (
          <div className="max-w-[320px] whitespace-pre-wrap break-words text-left">
            {content.substring(0, 230)}
            {content.length > 230 ? "..." : ""}
          </div>
        );
      }

      const json = JSON.stringify(content);

      return (
        <div className="max-w-[320px] whitespace-pre-wrap break-words text-left">
          {json?.substring(0, 230)}
          {json && json.length > 230 ? "..." : ""}
        </div>
      );
    },
  },
  {
    id: "actions",
    enableHiding: false,
    cell: ({ row }) => <LogActionsCell record={row.original} />,
  },
];

function LogActionsCell({ record }: { record: LogRecord }) {
  const [isOpen, setIsOpen] = React.useState(false);
  const location = useLocation();
  const { tabela } = location.state || {
    tabela: new URLSearchParams(location.search).get("tabela"),
  };

  React.useEffect(() => {
    return () => {
      document.body.style.pointerEvents = "";
    };
  }, [isOpen]);

  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" className="h-8 w-8 p-0">
            <span className="sr-only">Open menu</span>
            <MoreHorizontal className="h-4 w-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuLabel>Actions</DropdownMenuLabel>
          <DropdownMenuSeparator />
          <DropdownMenuItem
            onClick={() => {
              navigator.clipboard.writeText(record.id);
              toast("ID copied successfully!");
            }}
          >
            Copy ID
          </DropdownMenuItem>
          <DropdownMenuItem onClick={() => setIsOpen(true)}>
            Details
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>

      {isOpen && tabela && (
        <ModalObject
          id={record.id}
          tableName={tabela}
          isOpen={isOpen}
          onOpenChange={setIsOpen}
        />
      )}
    </>
  );
}

export function TableLogs() {
  const location = useLocation();
  const { tabela } = location.state || {
    tabela: new URLSearchParams(location.search).get("tabela"),
  };
  const params = new URLSearchParams(location.search);

  const [data, setData] = React.useState<LogRecord[]>([]);
  const [sorting, setSorting] = React.useState<SortingState>([]);
  const [columnFilters, setColumnFilters] = React.useState<ColumnFiltersState>([]);
  const [columnVisibility, setColumnVisibility] = React.useState<VisibilityState>({});
  const [lastId, setLastId] = React.useState<string | null>(null);

  const [timezone, setTimezone] = React.useState(
    params.get("timezone") || Intl.DateTimeFormat().resolvedOptions().timeZone
  );
  const [dateFrom, setDateFrom] = React.useState<Date>(() => {
    const datetime = params.get("datetime1");
    const now = new Date();
    now.setHours(now.getHours() - 1);
    return datetime ? new Date(datetime) : now;
  });
  const [dateTo, setDateTo] = React.useState<Date>(() => {
    const datetime = params.get("datetime2");
    const now = new Date();
    now.setMinutes(now.getMinutes() + 2);
    return datetime ? new Date(datetime) : now;
  });
  const [searchTerm, setSearchTerm] = React.useState(params.get("search") || "");

  const filtersRef = React.useRef({
    dateFrom,
    dateTo,
    timezone,
    searchTerm,
  });
  filtersRef.current = { dateFrom, dateTo, timezone, searchTerm };

  const table = useReactTable({
    data,
    columns,
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    onColumnVisibilityChange: setColumnVisibility,
    state: {
      sorting,
      columnFilters,
      columnVisibility,
    },
  });

  const search = async () => {
    if (!tabela) {
      setData([]);
      return;
    }

    const searchParams = new URLSearchParams({
      take: "5000",
      datetime1: format(dateFrom, "yyyy-MM-dd HH:mm:ss"),
      datetime2: format(dateTo, "yyyy-MM-dd HH:mm:ss"),
      timezone,
    });

    if (searchTerm) {
      searchParams.set("search", searchTerm);
    }

    try {
      const response = await api.get<LogRecord[]>(`/${tabela}?${searchParams.toString()}`, {
        headers: {
          Timezone: timezone,
        },
      });

      const nextData = Array.isArray(response.data) ? response.data : [];
      setData(nextData);

      if (nextData.length > 0) {
        setLastId(nextData[0].id);
      }

      const url = new URL(window.location.href);
      url.search = searchParams.toString();
      window.history.replaceState({}, "", url.toString());
    } catch (error) {
      console.log(error);
      setData([]);
      toast.error("Error on load data from table");
    }
  };

  const searchRef = React.useRef(search);
  searchRef.current = search;

  React.useEffect(() => {
    void searchRef.current();
  }, [tabela]);

  React.useEffect(() => {
    if (!tabela) {
      return;
    }

    const interval = setInterval(async () => {
      try {
        const response = await api.get<string>(`/${tabela}/Last`, {
          headers: {
            Timezone: filtersRef.current.timezone,
          },
        });

        if (filtersRef.current.dateTo >= new Date()) {
          const nextDateTo = new Date();
          nextDateTo.setMinutes(nextDateTo.getMinutes() + 2);
          setDateTo(nextDateTo);
        }

        if (response.data && response.data !== lastId) {
          setLastId(response.data);
          await searchRef.current();
        }
      } catch {
        console.log("Erro ao carregar LastID");
      }
    }, 3000);

    return () => clearInterval(interval);
  }, [lastId, tabela]);

  const handleTimeZone = (nextTimezone: string) => {
    const now = new Date();
    const dateInCurrentTZ = new Date(
      now.toLocaleString("en-US", { timeZone: timezone })
    );
    const dateInNextTZ = new Date(
      now.toLocaleString("en-US", { timeZone: nextTimezone })
    );
    const offset = dateInNextTZ.getTime() - dateInCurrentTZ.getTime();

    setTimezone(nextTimezone);
    setDateFrom(new Date(dateFrom.getTime() + offset));
    setDateTo(new Date(dateTo.getTime() + offset));
  };

  const handleDateFrom = (date: Date | undefined) => {
    if (!date) {
      return;
    }

    if (date > dateTo) {
      const nextDateTo = new Date(date.getTime() + 3600000);
      setDateTo(nextDateTo);
    }

    setDateFrom(date);
  };

  const handleDateTo = (date: Date | undefined) => {
    if (!date) {
      return;
    }

    if (date < dateFrom) {
      const nextDateFrom = new Date(date.getTime() + 3600000);
      setDateFrom(nextDateFrom);
    }

    setDateTo(date);
  };

  return (
    <>
      <HeaderBar />
      <EnsureLogin />
      <main className="mx-auto flex w-full max-w-7xl flex-col gap-4 px-4 py-5">
        <div className="space-y-1">
          <h1 className="text-3xl font-semibold tracking-tight">Logs from {tabela}</h1>
          <p className="text-sm text-muted-foreground">
            Explore table data, filter by time window and inspect log details.
          </p>
        </div>

        <LogTimelineChart rawData={data} dateFrom={dateFrom} dateTo={dateTo} />

        <div className="w-full rounded-xl border bg-card p-4 shadow-sm">
          <div className="flex flex-wrap items-center gap-3 py-1">
            <div className="w-full max-w-xs">
              <Input
                placeholder="Search"
                value={searchTerm}
                onKeyDown={(event) => {
                  if (event.key === "Enter") {
                    void search();
                  }
                }}
                onChange={(event) => setSearchTerm(event.target.value)}
              />
            </div>
            <div className="w-full max-w-xs">
              <Input
                placeholder="Filter trace ID"
                value={(table.getColumn("traceId")?.getFilterValue() as string) ?? ""}
                onChange={(event) =>
                  table.getColumn("traceId")?.setFilterValue(event.target.value)
                }
              />
            </div>
            <div className="w-full max-w-xs">
              <DateTimePicker
                func="from"
                date={dateFrom}
                setDate={handleDateFrom}
              />
            </div>
            <div className="w-full max-w-xs">
              <DateTimePicker
                func="to"
                date={dateTo}
                setDate={handleDateTo}
              />
            </div>
            <div className="w-full max-w-xs">
              <TimeZoneSelect value={timezone} setValue={handleTimeZone} />
            </div>
            <Button onClick={() => void search()}>Search</Button>
            <div className="ml-auto flex gap-2">
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline" className="ml-auto">
                    Visible Columns <ChevronDown className="ml-2 h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  {table
                    .getAllColumns()
                    .filter((column) => column.getCanHide())
                    .map((column) => (
                      <DropdownMenuCheckboxItem
                        key={column.id}
                        className="capitalize"
                        checked={column.getIsVisible()}
                        onCheckedChange={(value) =>
                          column.toggleVisibility(!!value)
                        }
                      >
                        {column.id}
                      </DropdownMenuCheckboxItem>
                    ))}
                </DropdownMenuContent>
              </DropdownMenu>
            </div>
          </div>

          <div className="mt-4 rounded-md border">
            <Table>
              <TableHeader>
                {table.getHeaderGroups().map((headerGroup) => (
                  <TableRow key={headerGroup.id}>
                    {headerGroup.headers.map((header) => (
                      <TableHead key={header.id}>
                        {header.isPlaceholder
                          ? null
                          : flexRender(header.column.columnDef.header, header.getContext())}
                      </TableHead>
                    ))}
                  </TableRow>
                ))}
              </TableHeader>
              <TableBody>
                {table.getRowModel().rows.length > 0 ? (
                  table.getRowModel().rows.map((row) => (
                    <TableRow key={row.id}>
                      {row.getVisibleCells().map((cell) => (
                        <TableCell key={cell.id}>
                          {flexRender(cell.column.columnDef.cell, cell.getContext())}
                        </TableCell>
                      ))}
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell colSpan={columns.length} className="h-24 text-center">
                      No results found.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>

          <div className="flex items-center justify-end gap-2 py-4">
            <div className="mr-auto text-sm text-muted-foreground">
              Showing {table.getRowModel().rows.length} of {data.length} logs
            </div>
            <Button
              variant="outline"
              size="sm"
              onClick={() => table.previousPage()}
              disabled={!table.getCanPreviousPage()}
            >
              Previous
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => table.nextPage()}
              disabled={!table.getCanNextPage()}
            >
              Next
            </Button>
          </div>
        </div>
      </main>
    </>
  );
}
