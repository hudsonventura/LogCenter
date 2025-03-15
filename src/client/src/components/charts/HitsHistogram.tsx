import { useState } from "react";
import { Bar, BarChart, ResponsiveContainer, XAxis, YAxis } from "recharts";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { ChartTooltip } from "@/components/ui/chart";
import { format } from "date-fns";

export default function HitsHistogram() {
  

  const generateMockedHits = () => {
    const now = new Date();
    const data = [
      {
        timestamp: now,
        count: 50,
      },
      {
        timestamp: now,
        count: 20,
      },
      {
        timestamp: now,
        count: 30,
      },
      {
        timestamp: now,
        count: 40,
      },
      {
        timestamp: now,
        count: 50,
      },
    ];
  
    return data;
  };

  const [data, setData] = useState(generateMockedHits());
  return (
    <Card className="w-full mb-8">
      <CardHeader>
        <CardTitle>Hits</CardTitle>
        <CardDescription>
          Quantidade de hits nas últimas 24 horas
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="h-[200px]">
          <ResponsiveContainer width="50%" height="100%">
            <BarChart data={data}>
              <XAxis
                dataKey="timestamp"
                tickFormatter={(value) => format(new Date(value), "HH:mm")}
                stroke="#888888"
                fontSize={12}
                tickLine={false}
                axisLine={false}
              />
              <YAxis
                stroke="#888888"
                fontSize={12}
                tickLine={false}
                axisLine={false}
                tickFormatter={(value) => `${value}`}
              />
              <Bar
                dataKey="count"
                fill="currentColor"
                radius={[4, 4, 0, 0]}
                className="fill-primary"
              />
              <ChartTooltip
                content={({ active, payload }) => {
                  if (active && payload && payload.length) {
                    return (
                      <div className="rounded-lg border bg-background p-2 shadow-sm">
                        <div className="grid grid-cols-2 gap-2">
                          <div className="flex flex-col">
                            <span className="text-[0.70rem] uppercase text-muted-foreground">
                              Time
                            </span>
                            <span className="font-bold text-muted-foreground">
                              {format(
                                new Date(payload[0].payload.timestamp),
                                "HH:mm"
                              )}
                            </span>
                          </div>
                          <div className="flex flex-col">
                            <span className="text-[0.70rem] uppercase text-muted-foreground">
                              Hits
                            </span>
                            <span className="font-bold">
                              {payload[0].value}
                            </span>
                          </div>
                        </div>
                      </div>
                    );
                  }
                  return null;
                }}
              />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  );
}
