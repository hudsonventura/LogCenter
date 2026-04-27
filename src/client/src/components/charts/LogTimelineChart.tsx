"use client";

import * as React from "react";
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

const minute = 60000;
const maxChartPoints = 240;

const resolveBucketMinutes = (start: Date, end: Date) => {
  const rangeInMinutes = Math.max(
    1,
    Math.ceil((end.getTime() - start.getTime()) / minute)
  );

  if (rangeInMinutes <= maxChartPoints) {
    return 1;
  }

  if (rangeInMinutes <= 24 * 60) {
    return 5;
  }

  if (rangeInMinutes <= 7 * 24 * 60) {
    return 60;
  }

  if (rangeInMinutes <= 31 * 24 * 60) {
    return 6 * 60;
  }

  return 24 * 60;
};

const getBucketDate = (date: Date, bucketMinutes: number) => {
  const bucketSize = bucketMinutes * minute;
  return new Date(Math.floor(date.getTime() / bucketSize) * bucketSize);
};

const getBucketLabelFormat = (bucketMinutes: number, start: Date, end: Date) => {
  if (bucketMinutes >= 24 * 60) {
    return "MM/dd";
  }

  if (bucketMinutes >= 60 || end.getTime() - start.getTime() > 86400000) {
    return "MM/dd HH:mm";
  }

  return "HH:mm";
};

const buildBuckets = (start: Date, end: Date, bucketMinutes: number) => {
  const buckets = new Map<string, Record<string, number | string>>();
  const current = getBucketDate(start, bucketMinutes);
  const labelFormat = getBucketLabelFormat(bucketMinutes, start, end);

  while (current <= end) {
    const key = format(current, "yyyy-MM-dd HH:mm");
    buckets.set(key, {
      time: format(current, labelFormat),
      datetime: key,
      ...Object.fromEntries(
        Object.keys(logLevels).map((level) => [`level${level}`, 0])
      ),
    });
    current.setMinutes(current.getMinutes() + bucketMinutes);
  }

  return buckets;
};

export default function LogTimelineChart({
  rawData,
  dateFrom,
  dateTo,
}: LogTimelineChartProps) {
  const dateFromTime = dateFrom.getTime();
  const dateToTime = dateTo.getTime();

  const { bucketMinutes, chartData, visibleLevels } = React.useMemo(() => {
    const start = new Date(dateFromTime);
    const end = new Date(dateToTime);
    const nextBucketMinutes = resolveBucketMinutes(start, end);
    const buckets = buildBuckets(start, end, nextBucketMinutes);
    const activeLevelSet = new Set<number>();

    rawData.forEach((item) => {
      if (!logLevels[item.level]) {
        return;
      }

      const parsedDate = new Date(item.timestamp);

      if (Number.isNaN(parsedDate.getTime())) {
        return;
      }

      const key = format(getBucketDate(parsedDate, nextBucketMinutes), "yyyy-MM-dd HH:mm");
      const currentBucket = buckets.get(key);

      if (!currentBucket) {
        return;
      }

      const bucketKey = `level${item.level}`;
      const currentValue = Number(currentBucket[bucketKey] || 0);
      currentBucket[bucketKey] = currentValue + 1;
      activeLevelSet.add(item.level);
    });

    return {
      bucketMinutes: nextBucketMinutes,
      chartData: Array.from(buckets.values()),
      visibleLevels: activeLevelSet.size > 0 ? Array.from(activeLevelSet).sort((a, b) => a - b) : [1],
    };
  }, [dateFromTime, dateToTime, rawData]);

  const chartConfig = React.useMemo(
    () =>
      visibleLevels.reduce((config, level) => {
        config[`level${level}`] = {
          label: logLevels[level].name,
          theme: logLevels[level].theme,
        };
        return config;
      }, {} as ChartConfig),
    [visibleLevels]
  );

  return (
    <Card>
      <CardHeader>
        <CardTitle>Logs by time</CardTitle>
        <CardDescription>
          Timeline grouped every {bucketMinutes === 1 ? "minute" : `${bucketMinutes} minutes`}
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
