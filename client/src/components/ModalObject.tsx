import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import api from "@/services/api";
import React from "react";
import { useState } from "react";

import JsonView from "@uiw/react-json-view";
import { toast } from "sonner"

export function ModalObject({ id, tableName, isOpen, onOpenChange }) {
  const [data, setData] = useState([]);

  const getObject = async () => {
    try {
      const response = await api.get(`/${tableName}/${id}`);
      console.log(response.data.content)
      setData(response.data.content);
    } catch (error) {
      console.log(error);
    }
  };
  

  React.useEffect(() => {
    getObject();
  }, []);

  const handleCopy = async () => {
    try {
        const jsonStr = JSON.stringify(data, null, 2);
        await navigator.clipboard.writeText(jsonStr);
        
        toast("Copied!", {
            description: "The content was copied to clipoboar",
        })
    } catch (error) {
        console.log(error);
    }
}

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      {isOpen && (
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Detalhes</DialogTitle>
            <DialogDescription>
              <div
                style={{
                  maxHeight: "400px", // Definindo a altura máxima para ativar o scroll
                  overflowY: "auto", // Habilita o scroll vertical se o conteúdo exceder a altura
                  width: "32em",
                  marginTop: "1em",
                }}
              >
                <JsonView value={data} />
              </div>
            </DialogDescription>
          </DialogHeader>
          <DialogFooter className="sm:justify-start">
            <Button variant="outline"onClick={handleCopy}>Copy content</Button>
            <DialogClose asChild>
              <Button
                type="button"
                variant="secondary"
                onClick={() => onOpenChange(false)}
              >
                Fechar
              </Button>
            </DialogClose>
          </DialogFooter>
        </DialogContent>
      )}
    </Dialog>
  );
}
