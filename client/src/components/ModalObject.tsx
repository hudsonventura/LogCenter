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
import api from "@/services/api";
import React from "react";
import { useState } from "react";

import JsonView from "@uiw/react-json-view";
import { toast } from "sonner";

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
  const [data, setData] = useState([]);

  const getObject = async () => {
    try {
      const response = await api.get(`/${tableName}/${id}`);
      setData(response.data.content);
    } catch (error) {
      console.log(error);
    }
  };

  React.useEffect(() => {
    getObject();
  }, [id]);

  const handleCopy = async () => {
    try {
      const jsonStr = JSON.stringify(data, null, 2);
      await navigator.clipboard.writeText(jsonStr);

      toast("Copied!", {
        description: "The content was copied to clipoboar",
      });
    } catch (error) {
      console.log(error);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[1080px] min-w-[300px]">
        <DialogHeader>
          <DialogTitle>Detalhes</DialogTitle>
          <DialogDescription className="w-full">
            <div
              style={{
                maxHeight: "400px", // Definindo a altura máxima para ativar o scroll
                overflow: "auto", // Habilita o scroll vertical se o conteúdo exceder a altura
                marginTop: "1em",
              }}
              className="min-w-full"
            >
              {typeof data === "string" && !/^\{.*\}$/.test(data) ? (
                <pre style={{ whiteSpace: "pre-wrap" }}>{data}</pre>
              ) : (
                <JsonView 
                  value={data} 
                  shortenTextAfterLength={1000}
                />
                
              )}
            </div>
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={handleCopy}>
            Copiar Conteudo
          </Button>
          <DialogClose asChild onClick={() => onOpenChange(false)}>
            <Button type="button" variant="secondary">
              Fechar
            </Button>
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
