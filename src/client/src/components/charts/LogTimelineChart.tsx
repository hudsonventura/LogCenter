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

const logLevels: Record<number, { name: string; color: string }> = {
  0: { name: "Trace", color: "hsl(var(--chart-5))" },
  1: { name: "Info", color: "hsl(var(--chart-1))" },
  2: { name: "Debug", color: "hsl(var(--chart-2))" },
  3: { name: "Warning", color: "hsl(var(--chart-4))" },
  4: { name: "Error", color: "hsl(var(--destructive))" },
  5: { name: "Critical", color: "hsl(var(--chart-3))" },
  6: { name: "Success", color: "hsl(var(--chart-2))" },
  7: { name: "Fatal", color: "hsl(var(--foreground))" },
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
      color: logLevels[level].color,
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
                strokeWidth={2}
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
