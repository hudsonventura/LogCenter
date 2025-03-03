"use client"

import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTrigger } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useEffect, useState } from "react"
import { Calendar } from "@/components/ui/calendar"
import { format } from "date-fns"
import { CalendarIcon } from "lucide-react"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { cn } from "@/lib/utils"
import api from "@/services/api"
import { toast } from "sonner"

export function ModalTokenGeneration() {
  const [isOpen, setIsOpen] = useState(false)
  const [tables, setTables] = useState("")
  const [date, setDate] = useState<Date | null>(null)
  const [isCalendarOpen, setIsCalendarOpen] = useState(false)
  const [generatedToken, setGeneratedToken] = useState("")
  const [tokenGerated, setTokenGerated] = useState(false)

  useEffect(() => {
    console.log(date)
  }, [date])

  const handleGenerate = async () => {
    if (!tables || !date) {
      alert("Por favor, preencha todos os campos.")
      return
    }

    try {
      const response = await api.post("/generateToken", {
        owner: "sistem X",
        tables: tables.split(","),
        expires: date!.toISOString(),
      })

      if (response.status === 200) {
        setGeneratedToken(response.data.token)
        toast.success("Token gerado com sucesso!")
        setTokenGerated(response.data);
      } else {
        alert("Erro ao gerar token. Por favor, tente novamente.")
      }
    } catch (error) {
      console.error("Erro ao gerar token:", error)
      alert("Erro ao gerar token. Por favor, tente novamente.")
    }
  }

  const handleDateSelect = (selectedDate: Date | null) => {
    setDate(selectedDate)
    setIsCalendarOpen(false)
  }

  const handleCopy = () => {
    navigator.clipboard.writeText(tokenGerated)
    toast.success("Token copiado com sucesso!")
  }
  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        <span style={{ cursor: "pointer" }}>Generate token</span>
      </DialogTrigger>
      <DialogContent onPointerDownOutside={(e) => e.preventDefault()}>
        <DialogHeader>
          <h2 className="text-lg font-semibold">Generate integration token</h2>
        </DialogHeader>
        <DialogDescription>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="tables" className="text-right">
                Tables
              </Label>
              <Input
                id="tables"
                value={tables}
                onChange={(e) => setTables(e.target.value)}
                className="col-span-3"
                placeholder="Enter table names (comma separated)"
              />
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="date" className="text-right">
                Expires in
              </Label>
              <Calendar
                mode="single"
                selected={date}
                onSelect={setDate}
              />
            </div>
          </div>
        </DialogDescription>
        {generatedToken && (
          <div className="flex items-center gap-4 py-4">
            <Input
              type="text"
              value={generatedToken}
              readOnly
              className="flex-1"
            />
            <Button variant="default" onClick={handleCopy}>
              Copiar
            </Button>
          </div>
        )}
        <div className="flex justify-end gap-4">
          <Button onClick={() => setIsOpen(false)}>Cancelar</Button>
          <Button variant="default" onClick={handleGenerate}>
            Gerar
          </Button>
        </div>
        <div className="flex items-center gap-2" style={{ wordBreak: "break-word" }}>
          {tokenGerated}
          <Button variant="default" onClick={handleCopy}>
            Copiar
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
}


