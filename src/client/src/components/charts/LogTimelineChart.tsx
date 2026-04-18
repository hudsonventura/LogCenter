"use client";

import { format } from "date-fns";
import { Line, LineChart, CartesianGrid, XAxis, YAxis } from "recharts";

import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  ChartContainer,
  ChartLegend,
  ChartLegendContent,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import type { ChartConfig } from "@/components/ui/chart";

type TimelineRecord = {
  level: number;
  timestamp: string;
};

type LogTimelineChartProps = {
  rawData: TimelineRecord[];
  dateFrom: Date;
  dateTo: Date;
};

const logLevels: Record<
  number,
  {
    name: string;
    theme: {
      light: string;
      dark: string;
    };
  }
> = {
  0: { name: "Trace", theme: { light: "#334155", dark: "#e2e8f0" } },
  1: { name: "Info", theme: { light: "#075985", dark: "#489ac0" } },
  2: { name: "Debug", theme: { light: "#3f3f46", dark: "#12241e" } },
  3: { name: "Warning", theme: { light: "#92400e", dark: "#fcd34d" } },
  4: { name: "Error", theme: { light: "#ff0000", dark: "#ff2b2b" } },
  5: { name: "Critical", theme: { light: "#ff0000", dark: "#ff2b2b" } },
  7: { name: "Fatal", theme: { light: "#ff0000", dark: "#ff2b2b" } },
  6: { name: "Success", theme: { light: "#166534", dark: "#15be4b" } },
};

const buildMinuteBuckets = (start: Date, end: Date) => {
  const buckets = new Map<string, Record<string, number | string>>();
  const current = new Date(start);

  while (current <= end) {
    const key = format(current, "yyyy-MM-dd HH:mm");
    buckets.set(key, {
      time: format(current, end.getTime() - start.getTime() > 86400000 ? "MM/dd HH:mm" : "HH:mm"),
      datetime: key,
      ...Object.fromEntries(
        Object.keys(logLevels).map((level) => [`level${level}`, 0])
      ),
    });
    current.setMinutes(current.getMinutes() + 1);
  }

  return buckets;
};

export default function LogTimelineChart({
  rawData,
  dateFrom,
  dateTo,
}: LogTimelineChartProps) {
  const buckets = buildMinuteBuckets(dateFrom, dateTo);

  rawData.forEach((item) => {
    const parsedDate = new Date(item.timestamp);

    if (Number.isNaN(parsedDate.getTime())) {
      return;
    }

    const key = format(parsedDate, "yyyy-MM-dd HH:mm");
    const currentBucket = buckets.get(key);

    if (!currentBucket) {
      return;
    }

    const bucketKey = `level${item.level}`;
    const currentValue = Number(currentBucket[bucketKey] || 0);
    currentBucket[bucketKey] = currentValue + 1;
  });

  const chartData = Array.from(buckets.values());
  const activeLevels = Object.keys(logLevels)
    .map(Number)
    .filter((level) =>
      chartData.some((item) => Number(item[`level${level}`] || 0) > 0)
    );
  const visibleLevels = activeLevels.length > 0 ? activeLevels : [1];

  const chartConfig = visibleLevels.reduce((config, level) => {
    config[`level${level}`] = {
      label: logLevels[level].name,
      theme: logLevels[level].theme,
    };
    return config;
  }, {} as ChartConfig);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Logs by time</CardTitle>
        <CardDescription>
          Timeline grouped by minute for the selected time window
        </CardDescription>
      </CardHeader>
      <CardContent>
        <ChartContainer config={chartConfig} className="h-[320px] w-full">
          <LineChart data={chartData} margin={{ left: 12, right: 12 }}>
            <CartesianGrid vertical={false} />
            <XAxis
              dataKey="time"
              tickLine={false}
              axisLine={false}
              tickMargin={8}
              minTickGap={24}
            />
            <YAxis allowDecimals={false} />
            <ChartTooltip content={<ChartTooltipContent />} />
            <ChartLegend content={<ChartLegendContent />} />
            {visibleLevels.map((level) => (
              <Line
                key={level}
                type="monotone"
                dataKey={`level${level}`}
                stroke={`var(--color-level${level})`}
                strokeWidth={3}
                dot={false}
                activeDot={{ r: 4 }}
              />
            ))}
          </LineChart>
        </ChartContainer>
      </CardContent>
      <CardFooter className="text-sm text-muted-foreground">
        Total records in range: {rawData.length}
      </CardFooter>
    </Card>
  );
}
