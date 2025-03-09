"use client"

import { TrendingUp } from "lucide-react"
import { PieChart, Pie, Cell, LabelList } from "recharts"

import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import {
  type ChartConfig,
  ChartContainer,
  ChartLegend,
  ChartLegendContent,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart"

type RawDataItem = {
  content: string | null
  traceId: null
  created_at: string
  id: string
  level: number
  message: string
}

type ComponentProps = {
  rawData: RawDataItem[]
}

// Define log levels with their corresponding names and colors
const logLevels = {
  1: { name: "Debug", color: "hsl(195, 70%, 50%)" },
  2: { name: "Info", color: "hsl(220, 70%, 50%)" },
  3: { name: "Warning", color: "hsl(45, 70%, 50%)" },
  4: { name: "Error", color: "hsl(0, 70%, 50%)" },
  5: { name: "Critical", color: "hsl(300, 70%, 50%)" },
}

export default function Component({ rawData }: ComponentProps) {
  // Count occurrences of each log level
  const levelCounts = rawData.reduce(
    (acc, item) => {
      acc[item.level] = (acc[item.level] || 0) + 1
      return acc
    },
    {} as Record<number, number>,
  )

  // Prepare data for the pie chart
  const chartData = Object.entries(levelCounts).map(([level, count]) => ({
    name: logLevels[Number(level)]?.name || `Nível ${level}`,
    value: count,
    color: logLevels[Number(level)]?.color || `hsl(${Number(level) * 60}, 70%, 50%)`,
  }))

  // Generate chart config
  const chartConfig: ChartConfig = chartData.reduce((config, item) => {
    config[item.name] = {
      label: item.name,
      color: item.color,
    }
    return config
  }, {} as ChartConfig)

  // Custom label for the pie slices
  const renderCustomizedLabel = ({ cx, cy, midAngle, innerRadius, outerRadius, percent, index }) => {
    const RADIAN = Math.PI / 180
    const radius = innerRadius + (outerRadius - innerRadius) * 0.5
    const x = cx + radius * Math.cos(-midAngle * RADIAN)
    const y = cy + radius * Math.sin(-midAngle * RADIAN)

    return (
      <text x={x} y={y} fill="white" textAnchor={x > cx ? "start" : "end"} dominantBaseline="central">
        {`${(percent * 100).toFixed(0)}%`}
      </text>
    )
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Distribuição de Níveis de Log</CardTitle>
        <CardDescription>
          {new Date(rawData[0]?.created_at).toLocaleDateString("pt-BR", {
            day: "2-digit",
            month: "long",
            year: "numeric",
          })}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <ChartContainer config={chartConfig}>
          <PieChart width={423} height={238}>
            <Pie
              data={chartData}
              labelLine={false}
              label={renderCustomizedLabel}
              outerRadius={150}
              fill="#8884d8"
              dataKey="value"
            >
              {chartData.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={entry.color} />
              ))}
              <LabelList dataKey="name" position="outside" />
            </Pie>
            <ChartTooltip content={<ChartTooltipContent />} />
            <ChartLegend content={<ChartLegendContent />} />
          </PieChart>
        </ChartContainer>
      </CardContent>
      <CardFooter className="flex-col items-start gap-2 text-sm">
        <div className="flex gap-2 font-medium leading-none">
          Total records: {rawData.length} <TrendingUp className="h-4 w-4" />
        </div>
      </CardFooter>
    </Card>
  )
}

