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
import { getLogLevelChartTheme, getLogLevelLabel } from "@/lib/log-levels";

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
  0: { name: getLogLevelLabel(0), theme: getLogLevelChartTheme(0) },
  1: { name: getLogLevelLabel(1), theme: getLogLevelChartTheme(1) },
  2: { name: getLogLevelLabel(2), theme: getLogLevelChartTheme(2) },
  3: { name: getLogLevelLabel(3), theme: getLogLevelChartTheme(3) },
  4: { name: getLogLevelLabel(4), theme: getLogLevelChartTheme(4) },
  5: { name: getLogLevelLabel(5), theme: getLogLevelChartTheme(5) },
  99: { name: getLogLevelLabel(99), theme: getLogLevelChartTheme(99) },
  200: { name: getLogLevelLabel(200), theme: getLogLevelChartTheme(200) },
  201: { name: getLogLevelLabel(201), theme: getLogLevelChartTheme(201) },
  202: { name: getLogLevelLabel(202), theme: getLogLevelChartTheme(202) },
  204: { name: getLogLevelLabel(204), theme: getLogLevelChartTheme(204) },
  301: { name: getLogLevelLabel(301), theme: getLogLevelChartTheme(301) },
  302: { name: getLogLevelLabel(302), theme: getLogLevelChartTheme(302) },
  304: { name: getLogLevelLabel(304), theme: getLogLevelChartTheme(304) },
  400: { name: getLogLevelLabel(400), theme: getLogLevelChartTheme(400) },
  401: { name: getLogLevelLabel(401), theme: getLogLevelChartTheme(401) },
  403: { name: getLogLevelLabel(403), theme: getLogLevelChartTheme(403) },
  404: { name: getLogLevelLabel(404), theme: getLogLevelChartTheme(404) },
  405: { name: getLogLevelLabel(405), theme: getLogLevelChartTheme(405) },
  408: { name: getLogLevelLabel(408), theme: getLogLevelChartTheme(408) },
  409: { name: getLogLevelLabel(409), theme: getLogLevelChartTheme(409) },
  422: { name: getLogLevelLabel(422), theme: getLogLevelChartTheme(422) },
  429: { name: getLogLevelLabel(429), theme: getLogLevelChartTheme(429) },
  500: { name: getLogLevelLabel(500), theme: getLogLevelChartTheme(500) },
  501: { name: getLogLevelLabel(501), theme: getLogLevelChartTheme(501) },
  502: { name: getLogLevelLabel(502), theme: getLogLevelChartTheme(502) },
  503: { name: getLogLevelLabel(503), theme: getLogLevelChartTheme(503) },
  504: { name: getLogLevelLabel(504), theme: getLogLevelChartTheme(504) },
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
