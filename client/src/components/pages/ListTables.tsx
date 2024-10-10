import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { useNavigate } from "react-router-dom";

import api from "@/services/api";

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
  }

  return (
    <>
      <h1 className="text-3xl font-bold mb-5">Tabelas</h1>
      <div className="flex flex-wrap gap-4">
        {data.map((item, index) => (
          <Card key={index} className="w-[350px]">
            <CardHeader>
              <CardTitle>{item || "Tabela sem nome"}</CardTitle>
            </CardHeader>
            <CardFooter className="flex justify-between">
              <Button className="w-full" onClick={() => consultarTabela(item)}>Consultar Logs</Button>
            </CardFooter>
          </Card>
        ))}
      </div>
    </>
  );
}
