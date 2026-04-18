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
  const [data, setData] = useState<unknown>("null");

  React.useEffect(() => {
    const getObject = async () => {
      try {
        const response = await api.get(`/${tableName}/${id}`);
        setData(null !== response.data.content ? response.data.content : "null");
      } catch (error) {
        console.log(error);
      }
    };

    void getObject();
  }, [id, tableName]);

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

  let jsonValue: object | undefined;

  if (typeof data === "object" && data !== null) {
    jsonValue = data as object;
  } else if (typeof data === "string") {
    try {
      const parsed = JSON.parse(data);
      if (typeof parsed === "object" && parsed !== null) {
        jsonValue = parsed as object;
      }
    } catch {
      jsonValue = undefined;
    }
  }

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
              {!jsonValue ? (
                <pre style={{ whiteSpace: "pre-wrap" }}>{String(data)}</pre>
              ) : (
                <JsonView 
                  value={jsonValue} 
                  shortenTextAfterLength={1000}
                />
                
              )}
            </div>
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={handleCopy}>
            Copy Content
          </Button>
          <DialogClose asChild onClick={() => onOpenChange(false)}>
            <Button type="button" variant="secondary">
              Close
            </Button>
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
