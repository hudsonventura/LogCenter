"use client"

import * as React from "react"
import { Check, ChevronsUpDown } from "lucide-react"

import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"

const values = [
  {
    value: 30,
    label: "30 days",
  },
  {
    value: 180,
    label: "6 months",
  },
  {
    value: 365,
    label: "1 year",
  },
  {
    value: 730,
    label: "2 years",
  },
  {
    value: 195,
    label: "3 years",
  },
  {
    value: 1865,
    label: "5 years",
  },
  {
    value: 3650,
    label: "10 years",
  },
]

export function ComboBoxTimeExpiration({ setDate }) {
  const [open, setOpen] = React.useState(false)
  const [value, setValue] = React.useState(0)

  React.useEffect(() => {
    if (value) {
      const newDate = new Date();
      newDate.setDate(newDate.getDate() + value); // Soma os dias corretamente
      setDate(newDate);
    }
  }, [value])
  

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className="col-span-3"
        >
          {value
            ? values.find((val) => val.value === value)?.label
            : "Select framework..."}
          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="col-span-3">
        <Command>
          <CommandInput placeholder="Select the expiration time..." />
          <CommandList>
            <CommandGroup>
              {values.map((val) => (
                <CommandItem
                  key={val.value}
                  value={val.value}
                  onSelect={(currentValue) => {
                    setValue(val.value)
                    setOpen(false)
                  }}
                >
                
                  {val.label}
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  )
}
