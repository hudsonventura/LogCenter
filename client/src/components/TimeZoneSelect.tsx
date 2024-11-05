"use client"

import * as React from "react"
import { CaretSortIcon, CheckIcon } from "@radix-ui/react-icons"

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
import api from "@/services/api"



export function TimeZoneSelect({value, setValue}: {value: string | undefined, setValue: (value: string) => void}) {
  const [open, setOpen] = React.useState(false)
  

  const [timezones, setTimezones] = React.useState([]);

  const listTimeZones = async () => {
    try {
      const response = await api.get(
        "/Timezones"
      );
      setTimezones(response.data);
      console.log(timezones); 
    } catch (error) {
      console.log(error);
    }
  };

  React.useEffect(() => {
    listTimeZones();
  }, []);

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className="w-[200px] justify-between"
        >
          {value || "Select your timezone ..."}
          {
            //value
            //? timezones.find((tz: { id: string }) => tz.id === value)?.id
            //:// "Select your timezone ..."
          }

          <CaretSortIcon className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[200px] p-0">
        <Command>
          <CommandInput placeholder="Search framework..." className="h-9" />
          <CommandList>
            <CommandEmpty>No timezone found</CommandEmpty>
            <CommandGroup>
              {timezones.map((tz: { id: string, displayName: string }) => (
                <CommandItem
                  key={tz.id}
                  value={tz.id}
                  onSelect={(currentValue) => {
                    setValue(currentValue === value ? "" : currentValue)
                    setOpen(false)
                  }}
                >
                  {tz.displayName}
                  <CheckIcon
                    className={cn(
                      "ml-auto h-4 w-4",
                      value === tz.id ? "opacity-100" : "opacity-0"
                    )}
                  />
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  )
}
