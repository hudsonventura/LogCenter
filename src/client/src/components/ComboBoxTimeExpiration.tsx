"use client"

import { useEffect, useState } from "react"

import { Input } from "@/components/ui/input"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"

type ExpirationUnit = "days" | "months" | "years"

type ComboBoxTimeExpirationProps = {
  setDate: (date: Date) => void
}

const expirationUnits: Array<{ label: string; value: ExpirationUnit }> = [
  { label: "Days", value: "days" },
  { label: "Months", value: "months" },
  { label: "Years", value: "years" },
]

function getExpirationDate(amount: number, unit: ExpirationUnit) {
  const date = new Date()

  if (unit === "days") {
    date.setDate(date.getDate() + amount)
  }

  if (unit === "months") {
    date.setMonth(date.getMonth() + amount)
  }

  if (unit === "years") {
    date.setFullYear(date.getFullYear() + amount)
  }

  return date
}

export function ComboBoxTimeExpiration({ setDate }: ComboBoxTimeExpirationProps) {
  const [amount, setAmount] = useState(1)
  const [unit, setUnit] = useState<ExpirationUnit>("days")

  useEffect(() => {
    setDate(getExpirationDate(amount, unit))
  }, [amount, setDate, unit])

  return (
    <div className="grid grid-cols-[minmax(0,1fr)_9rem] gap-2">
      <Input
        min={1}
        type="number"
        value={amount}
        onChange={(event) => {
          const nextAmount = Number(event.target.value)
          setAmount(Number.isFinite(nextAmount) && nextAmount > 0 ? nextAmount : 1)
        }}
      />
      <Select value={unit} onValueChange={(value: ExpirationUnit) => setUnit(value)}>
        <SelectTrigger>
          <SelectValue placeholder="Unit" />
        </SelectTrigger>
        <SelectContent>
          {expirationUnits.map((expirationUnit) => (
            <SelectItem key={expirationUnit.value} value={expirationUnit.value}>
              {expirationUnit.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  )
}
