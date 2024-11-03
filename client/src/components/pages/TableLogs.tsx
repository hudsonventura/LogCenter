"use client";

import * as React from "react";
import { useLocation, useNavigate } from "react-router-dom";
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
import { ChartBar } from "@phosphor-icons/react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
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
import { format } from "date-fns";
import { ModalObject } from "../ModalObject";
import { DateTimePicker } from "../DateTimePicker";

import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { TimeZoneSelect } from "../TimeZoneSelect";

export type Record = {
  id: bigint;
  level: RecordLevel;
  description: string;
  content: object;
  created_at: Date;
};

export enum RecordLevel {
  Info = 1,
  Debug = 2,
  Warning = 3,
  Error = 4,
  Critical = 5,
}

const getCorBadge = (level: RecordLevel) => {
  switch (level) {
    case RecordLevel.Info:
      return "bg-blue-200 text-blue-800";
    case RecordLevel.Debug:
      return "bg-gray-200 text-gray-800";
    case RecordLevel.Warning:
      return "bg-yellow-200 text-yellow-800";
    case RecordLevel.Error:
      return "bg-red-200 text-red-800";
    case RecordLevel.Critical:
      return "bg-red-700 text-white";
    default:
      return "bg-gray-200 text-gray-800";
  }
};

export const columns: ColumnDef<Record>[] = [
  {
    id: "select",
    header: ({ table }) => (
      <Checkbox
        checked={
          table.getIsAllPageRowsSelected() ||
          (table.getIsSomePageRowsSelected() && "indeterminate")
        }
        onCheckedChange={(value) => table.toggleAllPageRowsSelected(!!value)}
        aria-label="Select all"
      />
    ),
    cell: ({ row }) => (
      <Checkbox
        checked={row.getIsSelected()}
        onCheckedChange={(value) => row.toggleSelected(!!value)}
        aria-label="Select row"
      />
    ),
    enableSorting: false,
    enableHiding: false,
  },
  {
    accessorKey: "level",
    header: "Level",
    cell: ({ row }) => (
      <Badge
        className={`capitalize ${getCorBadge(
          row.original.level
        )} min-w-[40px] flex items-center justify-center`}
      >
        {RecordLevel[row.original.level]}
      </Badge>
    ),
  },

  {
    accessorKey: "description",
    header: ({ column }) => {
      return (
        <Button
          variant="ghost"
          onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
        >
          Descrição
          <ArrowUpDown className="ml-2 h-4 w-4" />
        </Button>
      );
    },
    cell: ({ row }) => (
      <div className="lowercase">{row.original.description}</div>
    ),
  },
  {
    accessorKey: "created_at",
    header: () => <div className="text-right">Criado em</div>,
    cell: ({ row }) => {
      // Formatar a data no formato personalizado
      const formattedDate = format(
        new Date(row.original.created_at),
        "yyyy/MM/dd HH:mm:ss"
      );
      return <div className="text-right font-medium">{formattedDate}</div>;
    },
  },
  {
    accessorKey: "content",
    header: () => <div className="text-left">Conteúdo</div>,
    cell: ({ row }) => {
      // Converter o objeto `content` para uma string JSON formatada
      const content = row.original.content;
      if (typeof content === "string") {
        return <div className="text-left font-medium">{content}</div>;
      }

      const jsonStr = JSON.stringify(content);
      const lines = jsonStr.split(/\r?\n/);
      const truncatedLines = lines
        .slice(0, 2)
        .map((line) => line.slice(0, 120));
      return (
        <div
          className="text-left font-medium"
          style={{ whiteSpace: "pre-wrap" }}
        >
          {truncatedLines.join("\n")}
        </div>
      );
    },
  },

  {
    id: "actions",
    enableHiding: false,
    cell: ({ row }) => {
      const dados = row.original;
      const [abrirModal, setAbrirModal] = React.useState(false);

      const copiarId = () => {
        navigator.clipboard.writeText(dados.id);
        toast("ID copiado com sucesso!");
      };

      const abrirDetalhes = () => {
        setAbrirModal(true);
        console.log(abrirModal);
      };

      const fecharDetalhes = () => {
        setAbrirModal(false);
        document.body.style.pointerEvents = "";
      };
      const location = useLocation();
      const { tabela } = location.state || {};

      React.useEffect(() => {
        // Cleanup ao desmontar ou quando o modal fechar
        return () => {
          document.body.style.pointerEvents = "";
        };
      }, [abrirModal]);

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
              <DropdownMenuLabel>Ações</DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={copiarId}>Copiar ID</DropdownMenuItem>
              <DropdownMenuItem onClick={() => abrirDetalhes()}>
                Detalhes
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>

          {/* ModalObject renderizado com base no estado */}
          {abrirModal && (
            <ModalObject
              id={dados.id}
              tableName={tabela}
              isOpen={abrirModal}
              onOpenChange={fecharDetalhes}
            />
          )}
        </>
      );
    },
  },
];

export function TableLogs() {
  const location = useLocation();
  const navigate = useNavigate();
  const { tabela } = location.state || {};
  const [data, setData] = React.useState([]);
  const [sorting, setSorting] = React.useState<SortingState>([]);
  const [columnFilters, setColumnFilters] = React.useState<ColumnFiltersState>(
    []
  );
  const [columnVisibility, setColumnVisibility] =
    React.useState<VisibilityState>({});
  const [rowSelection, setRowSelection] = React.useState({});

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
    onRowSelectionChange: setRowSelection,
    state: {
      sorting,
      columnFilters,
      columnVisibility,
      rowSelection,
    },
  });

  const search = async () => {
    try {
      const from = dateFrom.toISOString();
      const to = dateTo.toISOString();
      const response = await api.get(
        `/${tabela}?datetime1=${from}&datetime2=${to}&timezone=${timezone}${
          searchTerm ? `&search=${searchTerm}` : ''
        }`
      );
      const data = response.data
        ? response.data.map((item) => ({
            ...item,
            id: BigInt(item.id), // Certifique-se de que `snowflakeId` seja o campo correto
          }))
        : [];
      setData(data);
    } catch (error) {
      console.log(error);
      setData([]);
    }
  };

  React.useEffect(() => {
    search();
  }, []);

  const goToChart = (data) => {
    navigate("/charts", { state: { data } });
  };

  const [timezone, setTimezone] = React.useState(Intl.DateTimeFormat().resolvedOptions().timeZone);
  const [dateFrom, setDateFrom] = React.useState<Date>(() => {
    const now = new Date();
    now.setHours(now.getHours() - 1);
    return now;
  });
  const [dateTo, setDateTo] = React.useState<Date>(new Date());
  const [searchTerm, setSearchTerm] = React.useState<string>("");

  return (
    <>
      <h1 className="py-5 mb-4 font-bold text-center">Logs from {tabela}</h1>
      <div className="w-full">
        <div className="flex items-center py-4">
          <div className="max-w-xs"style={{padding: "0 0.5em"}}>
            <Input
              placeholder="Search"
              value={searchTerm}
              className="max-w-sm"
              onKeyDown={(event) => {
                if (event.key === 'Enter') {
                  search();
                }
              }}
              onChange={(event) => setSearchTerm(event.target.value)}
            />
          </div>
          <div className="max-w-xs"style={{padding: "0 0.5em"}}>
            <Input
              placeholder="Filter table"
              value={
                (table.getColumn("description")?.getFilterValue() as string) ?? ""
              }
              onChange={(event) =>
                table.getColumn("description")?.setFilterValue(event.target.value)
              }
              className="max-w-sm"
            />
          </div>
          <div className="max-w-xs" style={{padding: "0 0.5em"}}>
          <DateTimePicker date={dateFrom} setDate={setDateFrom}/>
          </div>
          <div className="max-w-xs">
            <DateTimePicker date={dateTo} setDate={setDateTo}/>
          </div>
          <div className="max-w-xs" style={{padding: "0 0.5em"}}>
            <TimeZoneSelect value={timezone} setValue={setTimezone} />
          </div><div className="max-w-xs" style={{padding: "0 0.5em"}}>
            <Button onClick={search}>Search</Button>
          </div>
          <div className=" flex ml-auto gap-2 ">
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline" className="ml-auto">
                  Colunas <ChevronDown className="ml-2 h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>

              <DropdownMenuContent align="end">
                {table
                  .getAllColumns()
                  .filter((column) => column.getCanHide())
                  .map((column) => {
                    return (
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
                    );
                  })}
              </DropdownMenuContent>
            </DropdownMenu>
            <TooltipProvider>
              <Tooltip>
                <TooltipTrigger>
                  <Button onClick={() => goToChart(data)}>
                    <ChartBar size={32} />
                  </Button>
                </TooltipTrigger>
                <TooltipContent>
                  <p>Visualizar gráficos</p>
                </TooltipContent>
              </Tooltip>
            </TooltipProvider>
          </div>
        </div>
        <div className="mt-4" />
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              {table.getHeaderGroups().map((headerGroup) => (
                <TableRow key={headerGroup.id}>
                  {headerGroup.headers.map((header) => {
                    return (
                      <TableHead key={header.id}>
                        {header.isPlaceholder
                          ? null
                          : flexRender(
                              header.column.columnDef.header,
                              header.getContext()
                            )}
                      </TableHead>
                    );
                  })}
                </TableRow>
              ))}
            </TableHeader>
            <TableBody>
              {table.getRowModel().rows?.length ? (
                table.getRowModel().rows.map((row) => (
                  <TableRow
                    key={row.id}
                    data-state={row.getIsSelected() && "selected"}
                  >
                    {row.getVisibleCells().map((cell) => (
                      <TableCell key={cell.id}>
                        {flexRender(
                          cell.column.columnDef.cell,
                          cell.getContext()
                        )}
                      </TableCell>
                    ))}
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell
                    colSpan={columns.length}
                    className="h-24 text-center"
                  >
                    Sem Resultados.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </div>
        <div className="flex items-center justify-end space-x-2 py-4">
          <div className="flex-1 text-sm text-muted-foreground p-2">
            Linhas Selecionadas -{" "}
            {table.getFilteredSelectedRowModel().rows.length} de{" "}
            {table.getFilteredRowModel().rows.length}
          </div>
          <div className="space-x-2 p-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => table.previousPage()}
              disabled={!table.getCanPreviousPage()}
            >
              Anterior
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => table.nextPage()}
              disabled={!table.getCanNextPage()}
            >
              Próximo
            </Button>
          </div>
        </div>
      </div>
    </>
  );
}
