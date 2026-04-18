"use client";

import * as React from "react";
import { CalendarIcon } from "@radix-ui/react-icons";
import { addMonths, addWeeks, addDays, addHours, addMinutes, format } from "date-fns";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Input } from "@/components/ui/input";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area";

export type RelativeUnit = "minutes" | "hours" | "days" | "weeks" | "months";

export type DatePickerValue =
  | { mode: "absolute"; date: Date }
  | { mode: "relative"; amount: number; unit: RelativeUnit }
  | { mode: "now" };

type DateTimePickerProps = {
  value: DatePickerValue;
  onChange: (value: DatePickerValue) => void;
  allowNow?: boolean;
};

const relativeUnits: Array<{ value: RelativeUnit; label: string }> = [
  { value: "minutes", label: "Minutes" },
  { value: "hours", label: "Hours" },
  { value: "days", label: "Days" },
  { value: "weeks", label: "Weeks" },
  { value: "months", label: "Months" },
];

const applyRelativeOffset = (date: Date, amount: number, unit: RelativeUnit) => {
  switch (unit) {
    case "minutes":
      return addMinutes(date, -amount);
    case "hours":
      return addHours(date, -amount);
    case "days":
      return addDays(date, -amount);
    case "weeks":
      return addWeeks(date, -amount);
    case "months":
      return addMonths(date, -amount);
  }
};

export const resolveDatePickerValue = (value: DatePickerValue, now = new Date()) => {
  if (value.mode === "absolute") {
    return value.date;
  }

  if (value.mode === "now") {
    return now;
  }

  return applyRelativeOffset(now, value.amount, value.unit);
};

const getLabel = (value: DatePickerValue, nowLabelDate: Date) => {
  if (value.mode === "absolute") {
    return format(value.date, "yyyy/MM/dd HH:mm");
  }

  if (value.mode === "now") {
    return `Now (${format(nowLabelDate, "yyyy/MM/dd HH:mm")})`;
  }

  return `Last ${value.amount} ${value.unit}`;
};

export function DateTimePicker({
  value,
  onChange,
  allowNow = false,
}: DateTimePickerProps) {
  const [isOpen, setIsOpen] = React.useState(false);
  const [activeTab, setActiveTab] = React.useState<DatePickerValue["mode"]>(
    value.mode
  );
  const [nowLabelDate, setNowLabelDate] = React.useState(new Date());
  const absoluteDate = value.mode === "absolute" ? value.date : new Date();
  const hours = Array.from({ length: 12 }, (_, i) => i + 1);

  React.useEffect(() => {
    setActiveTab(value.mode);
  }, [value.mode]);

  React.useEffect(() => {
    if (value.mode !== "now") {
      return;
    }

    const interval = setInterval(() => {
      setNowLabelDate(new Date());
    }, 1000);

    return () => clearInterval(interval);
  }, [value.mode]);

  const handleDateSelect = (selectedDate: Date | undefined) => {
    if (!selectedDate) {
      return;
    }

    const nextDate = new Date(absoluteDate);
    nextDate.setFullYear(selectedDate.getFullYear(), selectedDate.getMonth(), selectedDate.getDate());
    onChange({ mode: "absolute", date: nextDate });
  };

  const handleTimeChange = (
    type: "hour" | "minute" | "ampm",
    rawValue: string
  ) => {
    const nextDate = new Date(absoluteDate);

    if (type === "hour") {
      const parsedHour = Number.parseInt(rawValue, 10) % 12;
      nextDate.setHours(parsedHour + (nextDate.getHours() >= 12 ? 12 : 0));
    } else if (type === "minute") {
      nextDate.setMinutes(Number.parseInt(rawValue, 10));
    } else {
      const hoursValue = nextDate.getHours();
      if (rawValue === "PM" && hoursValue < 12) {
        nextDate.setHours(hoursValue + 12);
      }
      if (rawValue === "AM" && hoursValue >= 12) {
        nextDate.setHours(hoursValue - 12);
      }
    }

    onChange({ mode: "absolute", date: nextDate });
  };

  const setAbsoluteTab = () => {
    setActiveTab("absolute");
    if (value.mode !== "absolute") {
      onChange({ mode: "absolute", date: resolveDatePickerValue(value) });
    }
  };

  const setRelativeTab = () => {
    setActiveTab("relative");
    if (value.mode !== "relative") {
      onChange({ mode: "relative", amount: 15, unit: "minutes" });
    }
  };

  const setNowTab = () => {
    if (!allowNow) {
      return;
    }
    setActiveTab("now");
    onChange({ mode: "now" });
  };

  return (
    <Popover open={isOpen} onOpenChange={setIsOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          className={cn(
            "h-10 w-full min-w-0 justify-start px-3 text-left font-normal text-xs sm:text-sm",
            !value && "text-muted-foreground"
          )}
        >
          <CalendarIcon className="mr-2 h-4 w-4 shrink-0" />
          <span className="truncate">{getLabel(value, nowLabelDate)}</span>
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[360px] p-0 sm:w-[460px]" align="start">
        <div className="border-b p-1">
          <div className="grid grid-cols-3 gap-1">
            <Button
              type="button"
              variant={activeTab === "absolute" ? "default" : "ghost"}
              onClick={setAbsoluteTab}
            >
              Absolute
            </Button>
            <Button
              type="button"
              variant={activeTab === "relative" ? "default" : "ghost"}
              onClick={setRelativeTab}
            >
              Relative
            </Button>
            <Button
              type="button"
              variant={activeTab === "now" ? "default" : "ghost"}
              onClick={setNowTab}
              disabled={!allowNow}
            >
              Now
            </Button>
          </div>
        </div>

        {activeTab === "absolute" ? (
          <div className="sm:flex">
            <Calendar
              mode="single"
              selected={absoluteDate}
              defaultMonth={absoluteDate}
              onSelect={handleDateSelect}
              initialFocus
            />
            <div className="flex flex-col sm:h-[300px] sm:flex-row sm:divide-x">
              <ScrollArea className="w-64 sm:w-auto">
                <div className="flex p-2 sm:flex-col">
                  {hours.reverse().map((hour) => (
                    <Button
                      key={hour}
                      size="icon"
                      variant={
                        absoluteDate.getHours() % 12 === hour % 12
                          ? "default"
                          : "ghost"
                      }
                      className="aspect-square shrink-0 sm:w-full"
                      onClick={() => handleTimeChange("hour", hour.toString())}
                    >
                      {hour}
                    </Button>
                  ))}
                </div>
                <ScrollBar orientation="horizontal" className="sm:hidden" />
              </ScrollArea>
              <ScrollArea className="w-64 sm:w-auto">
                <div className="flex p-2 sm:flex-col">
                  {Array.from({ length: 12 }, (_, i) => i * 5).map((minute) => (
                    <Button
                      key={minute}
                      size="icon"
                      variant={
                        absoluteDate.getMinutes() === minute ? "default" : "ghost"
                      }
                      className="aspect-square shrink-0 sm:w-full"
                      onClick={() => handleTimeChange("minute", minute.toString())}
                    >
                      {minute.toString().padStart(2, "0")}
                    </Button>
                  ))}
                </div>
                <ScrollBar orientation="horizontal" className="sm:hidden" />
              </ScrollArea>
              <div className="flex p-2 sm:flex-col">
                {["AM", "PM"].map((ampm) => (
                  <Button
                    key={ampm}
                    size="icon"
                    variant={
                      (ampm === "AM" && absoluteDate.getHours() < 12) ||
                      (ampm === "PM" && absoluteDate.getHours() >= 12)
                        ? "default"
                        : "ghost"
                    }
                    className="aspect-square shrink-0 sm:w-full"
                    onClick={() => handleTimeChange("ampm", ampm)}
                  >
                    {ampm}
                  </Button>
                ))}
              </div>
            </div>
          </div>
        ) : null}

        {activeTab === "relative" ? (
          <div className="space-y-4 p-4">
            <div className="space-y-2">
              <p className="text-sm font-medium">Relative range</p>
              <div className="flex gap-2">
                <Input
                  type="number"
                  min={1}
                  value={value.mode === "relative" ? value.amount : 15}
                  onChange={(event) =>
                    onChange({
                      mode: "relative",
                      amount: Math.max(1, Number.parseInt(event.target.value || "1", 10)),
                      unit: value.mode === "relative" ? value.unit : "minutes",
                    })
                  }
                />
                <select
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  value={value.mode === "relative" ? value.unit : "minutes"}
                  onChange={(event) =>
                    onChange({
                      mode: "relative",
                      amount: value.mode === "relative" ? value.amount : 15,
                      unit: event.target.value as RelativeUnit,
                    })
                  }
                >
                  {relativeUnits.map((unit) => (
                    <option key={unit.value} value={unit.value}>
                      {unit.label}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <div className="rounded-md border bg-muted/40 p-3 text-sm text-muted-foreground">
              Resolved date: {format(resolveDatePickerValue(value), "yyyy/MM/dd HH:mm:ss")}
            </div>
          </div>
        ) : null}

        {activeTab === "now" ? (
          <div className="space-y-3 p-4">
            <p className="text-sm font-medium">Use current time</p>
            <div className="rounded-md border bg-muted/40 p-3 text-sm text-muted-foreground">
              The backend request will receive the current date and time when you click search.
            </div>
            <div className="font-mono text-sm">{format(nowLabelDate, "yyyy/MM/dd HH:mm:ss")}</div>
          </div>
        ) : null}
      </PopoverContent>
    </Popover>
  );
}
