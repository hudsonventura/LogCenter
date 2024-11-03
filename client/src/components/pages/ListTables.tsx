import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { useNavigate } from "react-router-dom";

import api from "@/services/api";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuPortal,
  DropdownMenuSeparator,
  DropdownMenuShortcut,
  DropdownMenuSub,
  DropdownMenuSubContent,
  DropdownMenuSubTrigger,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { ChevronDown, MoreHorizontal } from "lucide-react";

export function ListTables() {
  const [data, setData] = useState([]);
  const navigate = useNavigate();

  useEffect(() => {
    listTables();
  }, []);

  const listTables = async () => {
    try {
      const response = await api.get("/ListTables");
      setData(response.data);
    } catch (error) {
      console.log(error);
    }
  };

  const consultarTabela = (tabela) => {
    navigate("/table-logs", { state: { tabela } });
  };

  const goToConfigsTable = (tabela) => {
    navigate(`/table-configs/${tabela}`, { state: { tabela } });
  };

  return (
    <>
      <h1 className="font-bold mb-5 text-center mt-5">Tabelas</h1>
      <div className="flex flex-col items-center justify-center h-full">
        <div className="flex flex-wrap gap-4 text-center ">
          {data.map((item, index) => (
            <Card key={index} className="w-[350px]">
              <div className="flex justify-end">
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" className="h-8 w-8 p-0">
                      <span className="sr-only">Open menu</span>
                      <MoreHorizontal className="h-4 w-4" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent className="w-56">
                    <DropdownMenuGroup>
                      <DropdownMenuItem onClick={() => goToConfigsTable(item)}>
                        Configurations
                      </DropdownMenuItem>
                    </DropdownMenuGroup>
                  </DropdownMenuContent>
                </DropdownMenu>
              </div>
              <CardHeader>
                <CardTitle>{item || "Tabela sem nome"}</CardTitle>
              </CardHeader>
              <CardFooter className="flex justify-between gap-2">
                <Button
                  className="w-full"
                  onClick={() => consultarTabela(item)}
                >
                  Consultar Logs
                </Button>
              </CardFooter>
            </Card>
          ))}
        </div>
      </div>
    </>
  );
}
