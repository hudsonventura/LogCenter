"use client"

import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useState } from "react"

import api from "@/services/api"
import { toast } from "sonner"

import { ComboBoxTimeExpiration } from "./ComboBoxTimeExpiration"

export function ModalTokenGeneration() {
	const [isOpen, setIsOpen] = useState(false)
	const [tables, setTables] = useState("")
	const [name, setName] = useState("")
	const [date, setDate] = useState<Date | undefined>(new Date());



	const [generatedToken, setGeneratedToken] = useState("")
	const [tokenGerated, setTokenGerated] = useState(false)



	const handleGenerate = async () => {
		if (!tables || !date) {
			alert("Por favor, preencha todos os campos.")
			return
		}

		try {
			const response = await api.post("/generateToken", {
				name: name,
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
		<Popover>
			<PopoverTrigger asChild>
				<span style={{ cursor: "pointer" }}>Generate token</span>
			</PopoverTrigger>
			<PopoverContent className="p-4">
				<div className="grid gap-4 py-4">
					<div className="grid grid-cols-4 items-center gap-4">
						<Label htmlFor="tables" className="text-right">
							Name
						</Label>
						<Input
							id="name"
							value={name}
							onChange={(e) => setName(e.target.value)}
							className="col-span-3"
							placeholder="Set system's name"
						/>
						
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
						<Label htmlFor="date" className="text-right col-span-1">
							Expires in
						</Label>
						<div className="col-span-3">
							<ComboBoxTimeExpiration setDate={setDate} />
						</div>
					</div>
				</div>
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
					{tokenGerated && (
						<>
							<Button variant="default" onClick={handleCopy}>
								Copy
							</Button>
							{tokenGerated.slice(0, 80)}...
						</>
					)}
				</div>
			</PopoverContent>
		</Popover>
	)
}

