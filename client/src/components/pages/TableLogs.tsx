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

import { Button } from "@/components/ui/button";
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
import JsonView from "@uiw/react-json-view";

export type Record = {
  id: BigInt;
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
  // {
  //   accessorKey: "id",
  //   header: "Id",
  //   cell: ({ row }) => (
  //     console.log("testando", row.original.id),
  //     (<div className="capitalize">{row.original.id.toString()}</div>)
  //   ),
  // },
  {
    accessorKey: "level",
    header: "Level",
    cell: ({ row }) => <div className="capitalize">{RecordLevel[row.original.level]}</div>,
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
      const truncatedLines = lines.slice(0, 2).map((line) => line.slice(0, 120));
      return (
        <div className="text-left font-medium" style={{ whiteSpace: "pre-wrap" }}>
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
      };

      const fecharDetalhes = () => {
        setAbrirModal(false);
      }
      const location = useLocation();
      const { tabela } = location.state || {};

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
          {abrirModal && (
            <ModalObject id={dados.id} tableName={tabela} isOpen={abrirModal} onOpenChange={fecharDetalhes} />
          )}
        </>
      );
    },
  },
];

export function TableLogs() {
  const location = useLocation();
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

  const listTables = async () => {
    try {
      const response = await api.get(
        `/${tabela}?datetime1=2024-10-08T16:22:30`
      );
      const data = response.data.map((item) => ({
        ...item,
        id: BigInt(item.id), // Certifique-se de que `snowflakeId` seja o campo correto
      }));
      setData(data);
    } catch (error) {
      console.log(error);
    }
  };

  React.useEffect(() => {
    listTables();
  }, []);


  return (
    <>
      <h1 className="py-2 mb-4 font-bold">Logs</h1>
      <div className="w-full">
        <div className="flex items-center py-4">
          <Input
            placeholder="Filtrar por descrição"
            value={
              (table.getColumn("description")?.getFilterValue() as string) ?? ""
            }
            onChange={(event) =>
              table.getColumn("description")?.setFilterValue(event.target.value)
            }
            className="max-w-sm"
          />
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
        </div>
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
          <div className="flex-1 text-sm text-muted-foreground">
            Linhas Selecionadas -{" "}
            {table.getFilteredSelectedRowModel().rows.length} de{" "}
            {table.getFilteredRowModel().rows.length}
          </div>
          <div className="space-x-2">
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
