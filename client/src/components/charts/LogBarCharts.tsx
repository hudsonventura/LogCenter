"use client"

import { TrendingUp } from "lucide-react"
import { Bar, BarChart, CartesianGrid, XAxis, YAxis } from "recharts"

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
  correlation: null
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
  1: { name: "Debug", color: "hsl(157, 7.60%, 53.30%)" },
  2: { name: "Info", color: "hsl(199, 70.20%, 50.00%)" },
  3: { name: "Warning", color: "hsl(45, 70%, 50%)" },
  4: { name: "Error", color: "hsl(0, 69.50%, 60.20%)" },
  5: { name: "Critical", color: "hsl(0, 70.20%, 50.00%)" },
}

export default function Component({ rawData }: ComponentProps) {
  // Transform and group the data by created_at and level
  const chartData = rawData.reduce((acc, item) => {
    const date = new Date(item.created_at)
    const formattedTime = date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })

    const existingEntry = acc.find((entry) => entry.time === formattedTime)
    if (existingEntry) {
      existingEntry[`level${item.level}`] = (existingEntry[`level${item.level}`] || 0) + 1
    } else {
      const newEntry: any = { time: formattedTime }
      newEntry[`level${item.level}`] = 1
      acc.push(newEntry)
    }
    return acc
  }, [] as any[])

  // Get unique levels
  const levels = Array.from(new Set(rawData.map((item) => item.level))).sort()

  // Generate chart config dynamically
  const chartConfig: ChartConfig = levels.reduce((config, level) => {
    const levelInfo = logLevels[level] || { name: `Nível ${level}`, color: `hsl(${level * 60}, 70%, 50%)` }
    config[`level${level}`] = {
      label: levelInfo.name,
      color: levelInfo.color,
    }
    return config
  }, {} as ChartConfig)

  return (
    <Card>
      <CardHeader>
        <CardTitle>Dados por Nível de Log</CardTitle>
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
          <BarChart accessibilityLayer data={chartData}>
            <CartesianGrid vertical={false} />
            <XAxis dataKey="time" tickLine={false} tickMargin={10} axisLine={false} />
            <YAxis />
            <ChartTooltip content={<ChartTooltipContent />} />
            <ChartLegend content={<ChartLegendContent />} />
            {levels.map((level) => (
              <Bar
                key={`level${level}`}
                dataKey={`level${level}`}
                stackId="a"
                fill={logLevels[level]?.color || `hsl(${level * 60}, 70%, 50%)`}
                radius={[4, 4, 0, 0]}
              />
            ))}
          </BarChart>
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

