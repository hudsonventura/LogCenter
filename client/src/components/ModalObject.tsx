import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import api from "@/services/api";
import React from "react";
import { useState } from "react";

import { JsonView, allExpanded, darkStyles, defaultStyles } from 'react-json-view-lite';
import 'react-json-view-lite/dist/index.css';


const json = {
    a: 1,
    b: 'example'
  };


export function ModalObject({id}) {

    const [data, setData] = useState([])
    const getObject = async () => {
        try {
          const response = await api.get(`/teste/${id}`);
          console.log(123);
          setData(response.data)
        } catch (error) {
          console.log(error);
        }
      }
    
      React.useEffect(() => {
        getObject();
      }, []);


    return (
    <Dialog>
        <DialogTrigger asChild>
            <Button variant="outline">Icone de lupinha</Button>
        </DialogTrigger>
        <DialogContent>
        <DialogHeader>
        <DialogTitle>Details</DialogTitle>
        <DialogDescription>
            <div style={{width: "32em", marginTop: "1em"}}><JsonView data={data} shouldExpandNode={allExpanded} style={defaultStyles} /></div>
            
        </DialogDescription>
        </DialogHeader>
        <DialogFooter className="sm:justify-start">
          <DialogClose asChild>
            <Button type="button" variant="secondary">
              Close
            </Button>
          </DialogClose>
        </DialogFooter>
    </DialogContent>
    </Dialog>
  )
}
