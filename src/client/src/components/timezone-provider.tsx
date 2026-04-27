import * as React from "react";

type TimezoneContextValue = {
  timezone: string;
  setTimezone: (timezone: string) => void;
};

const TimezoneContext = React.createContext<TimezoneContextValue | null>(null);

export const timezoneStorageKey = "logcenter-timezone";

const getDefaultTimezone = () => {
  if (typeof window === "undefined") {
    return "UTC";
  }

  return (
    window.localStorage.getItem(timezoneStorageKey) ||
    Intl.DateTimeFormat().resolvedOptions().timeZone ||
    "UTC"
  );
};

export function TimezoneProvider({ children }: { children: React.ReactNode }) {
  const [timezone, setTimezone] = React.useState(getDefaultTimezone);

  React.useEffect(() => {
    window.localStorage.setItem(timezoneStorageKey, timezone);
  }, [timezone]);

  const value = React.useMemo(
    () => ({
      timezone,
      setTimezone,
    }),
    [timezone]
  );

  return (
    <TimezoneContext.Provider value={value}>
      {children}
    </TimezoneContext.Provider>
  );
}

export function useTimezone() {
  const context = React.useContext(TimezoneContext);

  if (!context) {
    throw new Error("useTimezone must be used within a TimezoneProvider");
  }

  return context;
}
