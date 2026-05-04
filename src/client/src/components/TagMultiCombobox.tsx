import * as React from "react";
import { Check, ChevronsUpDown, X } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { cn } from "@/lib/utils";

type TagMultiComboboxProps = {
  options: string[];
  value: string[];
  onValueChange: (value: string[]) => void;
  disabled?: boolean;
  loading?: boolean;
  placeholder?: string;
};

export function TagMultiCombobox({
  options,
  value,
  onValueChange,
  disabled = false,
  loading = false,
  placeholder = "Filter tags",
}: TagMultiComboboxProps) {
  const [open, setOpen] = React.useState(false);

  const handleToggle = (tag: string) => {
    if (value.includes(tag)) {
      onValueChange(value.filter((item) => item !== tag));
      return;
    }

    onValueChange([...value, tag]);
  };

  const handleRemove = (
    event: React.MouseEvent<HTMLButtonElement>,
    tag: string
  ) => {
    event.preventDefault();
    event.stopPropagation();
    onValueChange(value.filter((item) => item !== tag));
  };

  const handleTriggerKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (disabled) {
      return;
    }

    if (event.key === "Enter" || event.key === " ") {
      event.preventDefault();
      setOpen((current) => !current);
    }
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <div
          role="combobox"
          aria-expanded={open}
          aria-disabled={disabled}
          tabIndex={disabled ? -1 : 0}
          onKeyDown={handleTriggerKeyDown}
          className={cn(
            buttonVariants({ variant: "outline" }),
            "h-auto min-h-10 w-[320px] max-w-full justify-between gap-3 whitespace-normal px-3 py-2",
            "flex cursor-pointer items-center",
            disabled && "pointer-events-none cursor-not-allowed opacity-50"
          )}
        >
          <div className="flex min-w-0 flex-1 flex-wrap items-center gap-1.5 text-left">
            {value.length === 0 ? (
              <span className="truncate text-sm text-muted-foreground">
                {loading ? "Loading tags..." : placeholder}
              </span>
            ) : (
              value.map((tag) => (
                <Badge
                  key={tag}
                  variant="secondary"
                  className="flex max-w-full items-center gap-1 truncate px-2 py-0.5 text-xs"
                >
                  <span className="truncate">{tag}</span>
                  <button
                    type="button"
                    className="rounded-full p-0.5 transition-colors hover:bg-black/10 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                    onMouseDown={(event) => {
                      event.preventDefault();
                      event.stopPropagation();
                    }}
                    onClick={(event) => handleRemove(event, tag)}
                    aria-label={`Remove ${tag}`}
                  >
                    <X className="h-3 w-3" />
                  </button>
                </Badge>
              ))
            )}
          </div>
          <ChevronsUpDown className="h-4 w-4 shrink-0 opacity-50" />
        </div>
      </PopoverTrigger>
      <PopoverContent className="w-[320px] p-0" align="end">
        <Command>
          <CommandInput placeholder="Search tags..." />
          <CommandList>
            <CommandEmpty>
              {loading ? "Loading tags..." : "No tags found."}
            </CommandEmpty>
            <CommandGroup>
              {options.map((tag) => {
                const isSelected = value.includes(tag);

                return (
                  <CommandItem
                    key={tag}
                    value={tag}
                    onSelect={() => handleToggle(tag)}
                  >
                    <Check
                      className={cn(
                        "h-4 w-4",
                        isSelected ? "opacity-100" : "opacity-0"
                      )}
                    />
                    <span className="truncate">{tag}</span>
                  </CommandItem>
                );
              })}
            </CommandGroup>
          </CommandList>
          {value.length > 0 ? (
            <div className="border-t p-2">
              <Button
                type="button"
                variant="ghost"
                size="sm"
                className="w-full justify-center"
                onClick={() => onValueChange([])}
              >
                Clear selected tags
              </Button>
            </div>
          ) : null}
        </Command>
      </PopoverContent>
    </Popover>
  );
}
