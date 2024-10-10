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

export function ModalObject({ id, isOpen, onOpenChange }) {
  const [data, setData] = useState([]);
  const getObject = async () => {
    try {
      const response = await api.get(`/teste/${id}`);
      setData(response.data);
    } catch (error) {
      console.log(error);
    }
  };

  React.useEffect(() => {
    getObject();
  }, []);

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
