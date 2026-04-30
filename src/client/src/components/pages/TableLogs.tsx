"use client";

import * as React from "react";
import { useLocation, useNavigate } from "react-router-dom";
import {
  ColumnDef,
  ColumnFiltersState,
  PaginationState,
  ColumnSizingState,
  SortingState,
  VisibilityState,
  flexRender,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { ArrowLeft, ChevronDown } from "lucide-react";
import { format } from "date-fns";

import HeaderBar from "@/components/HeaderBar";
import EnsureLogin from "@/components/EnsureLogin";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
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
import { Checkbox } from "@/components/ui/checkbox";
import { toast } from "sonner";

import api from "@/services/api";
import { ModalObject } from "../ModalObject";
import { DatePickerValue, DateTimePicker, resolveDatePickerValue } from "../DateTimePicker";
import { useTimezone } from "../timezone-provider";
import LogTimelineChart from "../charts/LogTimelineChart";
import {
  allKnownRecordLevels,
  getLogLevelBadgeClass,
  getLogLevelLabel,
  levelFilterOptions,
} from "@/lib/log-levels";
import { Eye } from "lucide-react"

export type LogRecord = {
  id: string;
  level: number;
  traceId: string | null;
  message: string;
  content: unknown;
  timestamp: string;
  hideContentWhenMessageIsRendered?: boolean;
};



const allLevels = allKnownRecordLevels;

const formatTimestamp = (value: string) => {
  return String(value).replace("T", " ").replace(/Z$/, "");
};

const truncateText = (value: string, maxLength = 230) => {
  return value.length > maxLength ? `${value.substring(0, maxLength)}...` : value;
};

const summarizeValue = (value: unknown): unknown => {
  if (typeof value === "string") {
    return truncateText(value, 80);
  }

  if (Array.isArray(value)) {
    return `[Array(${value.length})]`;
  }

  if (typeof value === "object" && value !== null) {
    return "{...}";
  }

  return value;
};

const createContentPreview = (content: unknown) => {
  if (typeof content === "string") {
    return truncateText(content);
  }

  if (Array.isArray(content)) {
    const preview = content.slice(0, 3).map(summarizeValue);
    return truncateText(JSON.stringify(preview));
  }

  if (typeof content === "object" && content !== null) {
    const entries = Object.entries(content as Record<string, unknown>).slice(0, 8);
    const preview = Object.fromEntries(
      entries.map(([key, value]) => [key, summarizeValue(value)])
    );

    return truncateText(JSON.stringify(preview));
  }

  return String(content ?? "");
};

const columnWidths = {
  level: 60,
  timestamp: 130,
  traceId: 210,
  message: 320,
  content: 360,
  actions: 72,
} as const;

export const columns: ColumnDef<LogRecord>[] = [
  {
    accessorKey: "level",
    header: "Level",
    size: 80,
    minSize: 80,
    cell: ({ row }) => (
      <Badge
        className={`min-w-[76px] justify-center ${getLogLevelBadgeClass(row.original.level)}`}
      >
        {getLogLevelLabel(row.original.level)}
      </Badge>
    ),
  },
  {
    accessorKey: "timestamp",
    header: () => <div className="text-left">Timestamp</div>,
    size: 80,
    minSize: 80,
    cell: ({ row }) => (
      <div className="text-left font-mono text-xs sm:text-sm">
        {formatTimestamp(row.original.timestamp)}
      </div>
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
      </Button>
    ),
    size: 100,
    minSize: 100,
    cell: ({ row }) => (
      <div className="truncate lowercase">
        {row.original.traceId || "-"}
      </div>
    ),
  },
  {
    accessorKey: "message",
    header: () => <div className="text-left">Message</div>,
    size: columnWidths.message,
    minSize: 150,
    cell: ({ row }) => (
      <div className="whitespace-pre-wrap break-words text-left">
        {row.original.message}
      </div>
    ),
  },
  {
    accessorKey: "content",
    header: () => <div className="text-left">Content</div>,
    size: columnWidths.content,
    minSize: 250,
    cell: ({ row }) => {
      return (
        <div className="whitespace-pre-wrap break-words text-left">
          {createContentPreview(row.original.content)}
        </div>
      );
    },
  },
];

function LogActionsCell({
  record,
  onOpenDetails,
}: {
  record: LogRecord;
  onOpenDetails: (id: string) => void;
}) {
  return (
    <>
      <Button variant="outline" size="icon" aria-label="Submit" onClick={() => onOpenDetails(record.id)}>
        <Eye />
      </Button>
      {/* <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" className="h-8 w-8 p-0">
            <span className="sr-only">Open menu</span>
            <MoreHorizontal className="h-4 w-4"  />
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
          
        </DropdownMenuContent>
      </DropdownMenu> */}
    </>
  );
}

const pageSizeOptions = [50, 100, 200, 500];

export function TableLogs() {
  const navigate = useNavigate();
  const location = useLocation();
  const params = new URLSearchParams(location.search);
  const tabela =
    (location.state as { tabela?: string } | null)?.tabela ?? params.get("tabela");
  const { timezone } = useTimezone();

  const [data, setData] = React.useState<LogRecord[]>([]);
  const [sorting, setSorting] = React.useState<SortingState>([]);
  const [columnFilters, setColumnFilters] = React.useState<ColumnFiltersState>([]);
  const [pagination, setPagination] = React.useState<PaginationState>({
    pageIndex: 0,
    pageSize: 100,
  });
  const [columnVisibility, setColumnVisibility] = React.useState<VisibilityState>({
    content: false,
  });
  const [columnSizing, setColumnSizing] = React.useState<ColumnSizingState>({});
  const [dateFrom, setDateFrom] = React.useState<DatePickerValue>(() => {
    const datetime = params.get("datetime1");
    if (datetime) {
      return { mode: "absolute", date: new Date(datetime) };
    }

    return { mode: "relative", amount: 1, unit: "hours" };
  });
  const [dateTo, setDateTo] = React.useState<DatePickerValue>(() => {
    const datetime = params.get("datetime2");
    if (datetime) {
      return { mode: "absolute", date: new Date(datetime) };
    }

    return { mode: "now" };
  });
  const [searchTerm, setSearchTerm] = React.useState(params.get("search") || "");
  const [includeContent, setIncludeContent] = React.useState(
    params.get("bring_content") === "true"
  );
  const [lastSearchParams, setLastSearchParams] = React.useState<URLSearchParams | null>(null);
  const [selectedRecordId, setSelectedRecordId] = React.useState<string | null>(null);
  const [selectedLevels, setSelectedLevels] = React.useState<number[]>(allLevels);
  const [liveNowTick, setLiveNowTick] = React.useState(new Date());
  const resolvedDateFrom = React.useMemo(
    () => resolveDatePickerValue(dateFrom, liveNowTick),
    [dateFrom, liveNowTick]
  );
  const resolvedDateTo = React.useMemo(
    () => resolveDatePickerValue(dateTo, liveNowTick),
    [dateTo, liveNowTick]
  );
  const selectedLevelSet = React.useMemo(
    () => new Set(selectedLevels),
    [selectedLevels]
  );
  const visibleData = React.useMemo(
    () => data.filter((record) => selectedLevelSet.has(record.level)),
    [data, selectedLevelSet]
  );

  const table = useReactTable({
    data: visibleData,
    columns: React.useMemo<ColumnDef<LogRecord>[]>(
      () => [
        ...columns,
        {
          id: "actions",
          enableHiding: false,
          size: columnWidths.actions,
          minSize: 72,
          cell: ({ row }) => (
            <LogActionsCell
              record={row.original}
              onOpenDetails={setSelectedRecordId}
            />
          ),
        },
      ],
      []
    ),
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    onColumnSizingChange: setColumnSizing,
    onPaginationChange: setPagination,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    onColumnVisibilityChange: setColumnVisibility,
    enableColumnResizing: true,
    columnResizeMode: "onChange",
    state: {
      sorting,
      columnFilters,
      columnVisibility,
      columnSizing,
      pagination,
    },
  });
  const tableRows = table.getRowModel().rows;
  const navigationRows = table.getPrePaginationRowModel().rows;
  const modalRecordIds = React.useMemo(
    () => navigationRows.map((row) => row.original.id),
    [navigationRows]
  );
  const selectedIndex =
    selectedRecordId !== null ? modalRecordIds.indexOf(selectedRecordId) : -1;
  const previousRecordId = selectedIndex > 0 ? modalRecordIds[selectedIndex - 1] : null;
  const nextRecordId =
    selectedIndex >= 0 && selectedIndex < modalRecordIds.length - 1
      ? modalRecordIds[selectedIndex + 1]
      : null;

  const search = async () => {
    if (!tabela) {
      setData([]);
      setLastSearchParams(null);
      return;
    }

    try {
      const fromDate = resolveDatePickerValue(dateFrom);
      const toDate = resolveDatePickerValue(dateTo);

      if (Number.isNaN(fromDate.getTime()) || Number.isNaN(toDate.getTime())) {
        toast.error("Invalid date range");
        return;
      }

      const searchParams = new URLSearchParams({
        tabela,
        take: "5000",
        datetime1: format(fromDate, "yyyy-MM-dd HH:mm:ss"),
        datetime2: format(toDate, "yyyy-MM-dd HH:mm:ss"),
        timezone,
        bring_content: String(includeContent),
      });

      if (searchTerm) {
        searchParams.set("search", searchTerm);
      }

      const response = await api.get<LogRecord[]>(`/${tabela}?${searchParams.toString()}`);

      const nextData = Array.isArray(response.data) ? response.data : [];
      setData(nextData);
      setLastSearchParams(new URLSearchParams(searchParams));
      setPagination((current) => ({ ...current, pageIndex: 0 }));

      const url = new URL(window.location.href);
      url.search = searchParams.toString();
      window.history.replaceState({}, "", url.toString());
    } catch (error) {
      console.log(error);
      setData([]);
      setLastSearchParams(null);
      toast.error("Error on load data from table");
    }
  };

  const openTerminalView = () => {
    if (!lastSearchParams) {
      return;
    }

    const terminalParams = new URLSearchParams(lastSearchParams);
    terminalParams.set("bring_content", "true");

    navigate(`/log-terminal?${terminalParams.toString()}`);
  };

  const searchRef = React.useRef(search);
  searchRef.current = search;

  React.useEffect(() => {
    void searchRef.current();
  }, [tabela, timezone]);

  React.useEffect(() => {
    setColumnVisibility((current) => ({
      ...current,
      content: includeContent,
    }));
  }, [includeContent]);

  React.useEffect(() => {
    if (dateTo.mode !== "now") {
      return;
    }

    const interval = setInterval(() => {
      setLiveNowTick(new Date());
    }, 3000);

    return () => clearInterval(interval);
  }, [dateTo.mode]);

  React.useEffect(() => {
    if (!selectedRecordId) {
      return;
    }

    if (!modalRecordIds.includes(selectedRecordId)) {
      setSelectedRecordId(null);
    }
  }, [modalRecordIds, selectedRecordId]);

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
            aria-label="Back to tables"
            onClick={() => navigate("/tables")}
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div className="space-y-1">
            <h1 className="text-3xl font-semibold tracking-tight">Logs from {tabela}</h1>
          </div>
        </div>

        <LogTimelineChart rawData={visibleData} dateFrom={resolvedDateFrom} dateTo={resolvedDateTo} />

    
        <div className="w-full rounded-xl border bg-card p-4 shadow-sm">
          <div className="mb-3 flex justify-end gap-2">
              <label className="flex h-10 items-center gap-2 rounded-md border px-3 text-sm text-muted-foreground">
              <Checkbox
                checked={includeContent}
                onCheckedChange={(checked) => setIncludeContent(checked === true)}
              />
              <span>Include payloads</span>
            </label>
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline">
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
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline">
                    Levels <ChevronDown className="ml-2 h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  {levelFilterOptions.map((option) => {
                    const optionLevels = option.levels as readonly number[];

                    return (
                    <DropdownMenuCheckboxItem
                      key={option.id}
                      className="capitalize"
                      checked={optionLevels.every((level) => selectedLevels.includes(level))}
                      onSelect={(event) => event.preventDefault()}
                      onCheckedChange={(checked) => {
                        setSelectedLevels((current) => {
                          if (checked) {
                            return Array.from(new Set([...current, ...optionLevels])).sort((a, b) => a - b);
                          }

                          const next = current.filter((item) => !optionLevels.includes(item));
                          return next.length > 0 ? next : current;
                        });
                      }}
                    >
                      {option.label}
                    </DropdownMenuCheckboxItem>
                    );
                  })}
                </DropdownMenuContent>
              </DropdownMenu>
          </div>
          <div className="flex flex-wrap items-start gap-3 py-1">
            <div className="min-w-0 flex-1 basis-[220px]">
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
            <div className="min-w-0 flex-1 basis-[320px]">
              <Input
                placeholder="Filter trace ID"
                value={(table.getColumn("traceId")?.getFilterValue() as string) ?? ""}
                onChange={(event) =>
                  table.getColumn("traceId")?.setFilterValue(event.target.value)
                }
              />
            </div>
            
            <div className="flex min-w-0 flex-[1.5] basis-[430px] gap-2">
              <div className="min-w-0 flex-1">
                <DateTimePicker
                  value={dateFrom}
                  onChange={setDateFrom}
                />
              </div>
              <div className="min-w-0 flex-1">
                <DateTimePicker
                  value={dateTo}
                  onChange={setDateTo}
                  allowNow
                />
              </div>
            </div>
            
            <Button type="button" onClick={() => void search()}>
              Search
            </Button>
            <Button
              type="button"
              variant="outline"
              onClick={openTerminalView}
              disabled={!lastSearchParams}
            >
              Terminal view
            </Button>
            
          </div>
          
          <div className="flex items-center justify-end gap-2 py-4">
            <div className="mr-auto text-sm text-muted-foreground">
              Showing {tableRows.length} of {table.getPrePaginationRowModel().rows.length} logs
            </div>
            <select
              className="h-9 rounded-md border bg-background px-2 text-sm"
              value={pagination.pageSize}
              onChange={(event) => {
                table.setPageSize(Number(event.target.value));
              }}
            >
              {pageSizeOptions.map((pageSize) => (
                <option key={pageSize} value={pageSize}>
                  {pageSize} per page
                </option>
              ))}
            </select>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => table.previousPage()}
              disabled={!table.getCanPreviousPage()}
            >
              Previous
            </Button>
            <span className="text-sm text-muted-foreground">
              Page {table.getState().pagination.pageIndex + 1} of {table.getPageCount()}
            </span>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => table.nextPage()}
              disabled={!table.getCanNextPage()}
            >
              Next
            </Button>
          </div>

          <div className="mt-4 rounded-md border">
            <Table className="table-fixed">
              <TableHeader>
                {table.getHeaderGroups().map((headerGroup) => (
                  <TableRow key={headerGroup.id}>
                    {headerGroup.headers.map((header) => (
                      <TableHead
                        key={header.id}
                        className="relative select-none"
                        style={{ width: header.getSize() }}
                      >
                        {header.isPlaceholder ? null : (
                          <>
                            {flexRender(header.column.columnDef.header, header.getContext())}
                            <div
                              onDoubleClick={() => header.column.resetSize()}
                              onMouseDown={header.getResizeHandler()}
                              onTouchStart={header.getResizeHandler()}
                              className={`absolute right-0 top-0 h-full w-2 cursor-col-resize select-none touch-none ${
                                header.column.getIsResizing() ? "bg-primary/40" : "bg-transparent hover:bg-border"
                              }`}
                            />
                          </>
                        )}
                      </TableHead>
                    ))}
                  </TableRow>
                ))}
              </TableHeader>
              <TableBody>
                {table.getRowModel().rows.length > 0 ? (
                  tableRows.map((row) => (
                    <TableRow key={row.id}>
                      {row.getVisibleCells().map((cell) => (
                        <TableCell
                          key={cell.id}
                          style={{ width: cell.column.getSize() }}
                        >
                          {flexRender(cell.column.columnDef.cell, cell.getContext())}
                        </TableCell>
                      ))}
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell colSpan={columns.length + 1} className="h-24 text-center">
                      No results found.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>

          
        </div>
      </main>
      {selectedRecordId && tabela ? (
        <ModalObject
          id={selectedRecordId}
          tableName={tabela}
          isOpen={selectedRecordId !== null}
          onOpenChange={(isOpen) => {
            if (!isOpen) {
              setSelectedRecordId(null);
            }
          }}
          onPrevious={previousRecordId ? () => setSelectedRecordId(previousRecordId) : undefined}
          onNext={nextRecordId ? () => setSelectedRecordId(nextRecordId) : undefined}
          hasPrevious={previousRecordId !== null}
          hasNext={nextRecordId !== null}
        />
      ) : null}
    </>
  );
}
