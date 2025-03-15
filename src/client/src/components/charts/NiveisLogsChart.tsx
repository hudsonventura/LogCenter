"use client";

import {
  Bar,
  BarChart,
  XAxis,
  YAxis,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
} from "recharts";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import { useLocation } from "react-router-dom";

const colorMap = {
  1: "hsl(var(--chart-1))",
  2: "hsl(var(--chart-2))",
  3: "hsl(var(--chart-3))",
  4: "hsl(var(--chart-4))",
  5: "hsl(var(--chart-5))",
};

export default function NiveisLogsChart({ data }) {
  const chartData = data;

  // Verificando se os dados existem
  if (!chartData || !Array.isArray(chartData) || chartData.length === 0) {
    return <div>Nenhum dado disponível para o gráfico.</div>;
  }

  // Agrupando e contando os níveis
  const levelCounts = chartData.reduce((acc, item) => {
    acc[item.level] = (acc[item.level] || 0) + 1;
    return acc;
  }, {});

  const levelNames = {
    1: "Info",
    2: "Debug",
    3: "Warning",
    4: "Error",
    5: "Critical",
    6: "Success",
    7: "Fatal",
  };

  // Transformando os dados no formato esperado
  const formattedData = Object.entries(levelCounts).map(([level, count]) => ({
    level: `${levelNames[level]}`,
    value: count,
    color: colorMap[level] || "hsl(var(--chart-1))",
  }));

  return (
    <Card className="w-full p-5">
      <CardHeader>
        <CardTitle className="text-xl sm:text-2xl">
          Distribuição de Níveis de Log
        </CardTitle>
        <CardDescription className="text-sm sm:text-base">
          Contagem de logs por nível de severidade
        </CardDescription>
      </CardHeader>
      <CardContent>
        <ChartContainer
          config={Object.fromEntries(
            formattedData.map((item) => [
              item.level,
              { label: item.level, color: item.color },
            ])
          )}
          className="sm:h-[500px] md:h-[300px] lg:h-[300px]"
        >
          <ResponsiveContainer width="100%" height="100%">
            <BarChart
              data={formattedData}
              margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
            >
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis
                dataKey="level"
                tick={{ fontSize: 12 }}
                interval={0}
                angle={-45}
                textAnchor="end"
                height={60}
              />
              <YAxis tick={{ fontSize: 12 }} />
              <ChartTooltip content={<ChartTooltipContent />} />
              <Legend wrapperStyle={{ fontSize: "12px" }} />
              <Bar dataKey="value" name="Quantidade de Logs">
                {formattedData.map((entry, index) => (
                  <Bar key={`cell-${index}`} fill={entry.color} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </ChartContainer>
      </CardContent>
    </Card>
  );
}
