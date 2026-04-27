"use client"

import * as React from "react"

import { Input } from "@/components/ui/input"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"

type ExpirationUnit = "days" | "months" | "years"

const addExpiration = (amount: number, unit: ExpirationUnit) => {
  const date = new Date()

  if (unit === "days") {
    date.setDate(date.getDate() + amount)
    return date
  }

  if (unit === "months") {
    date.setMonth(date.getMonth() + amount)
    return date
  }

  date.setFullYear(date.getFullYear() + amount)
  return date
}

export function ComboBoxTimeExpiration({ setDate }: { setDate: (date: Date) => void }) {
  const [amount, setAmount] = React.useState(30)
  const [unit, setUnit] = React.useState<ExpirationUnit>("days")

  React.useEffect(() => {
    setDate(addExpiration(amount, unit))
  }, [amount, setDate, unit])

  return (
    <div className="grid grid-cols-[minmax(96px,1fr)_minmax(120px,1fr)] gap-2">
      <Input
        type="number"
        min={1}
        step={1}
        value={amount}
        onChange={(event) => {
          const nextAmount = Number(event.target.value)
          setAmount(Number.isFinite(nextAmount) && nextAmount > 0 ? nextAmount : 1)
        }}
      />
      <Select value={unit} onValueChange={(value) => setUnit(value as ExpirationUnit)}>
        <SelectTrigger>
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="days">Days</SelectItem>
          <SelectItem value="months">Months</SelectItem>
          <SelectItem value="years">Years</SelectItem>
        </SelectContent>
      </Select>
    </div>
  )
}
