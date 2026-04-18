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
import { useTimezone } from "./timezone-provider"

type TimezoneOption = {
  id: string;
  displayName: string;
}
export function TimeZoneSelect() {
  const { timezone, setTimezone } = useTimezone()
  const [open, setOpen] = React.useState(false)
  

  const [timezones, setTimezones] = React.useState<TimezoneOption[]>([]);

  const listTimeZones = async () => {
    try {
      const response = await api.get(
        "/Timezones"
      );
      setTimezones(response.data);
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
          className="w-[220px] justify-between"
        >
          <span className="truncate">{timezone || "Select your timezone ..."}</span>
          <CaretSortIcon className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[320px] p-0" align="end">
        <Command>
          <CommandInput placeholder="Search timezone..." className="h-9" />
          <CommandList>
            <CommandEmpty>No timezone found</CommandEmpty>
            <CommandGroup>
              {timezones.map((tz) => (
                <CommandItem
                  key={tz.id}
                  value={tz.id}
                  onSelect={() => {
                    setTimezone(tz.id)
                    setOpen(false)
                  }}
                >
                  {tz.displayName}
                  <CheckIcon
                    className={cn(
                      "ml-auto h-4 w-4",
                      timezone === tz.id ? "opacity-100" : "opacity-0"
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
