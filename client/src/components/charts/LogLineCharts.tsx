"use client"

import { TrendingUp } from "lucide-react"
import { Line, LineChart, CartesianGrid, XAxis, YAxis } from "recharts"

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
  1: { name: "Debug", color: "hsl(195, 70%, 50%)" },
  2: { name: "Info", color: "hsl(220, 70%, 50%)" },
  3: { name: "Warning", color: "hsl(45, 70%, 50%)" },
  4: { name: "Error", color: "hsl(0, 70%, 50%)" },
  5: { name: "Critical", color: "hsl(300, 70%, 50%)" },
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
      const newEntry: any = { time: formattedTime, originalDate: date }
      Object.keys(logLevels).forEach((level) => {
        newEntry[`level${level}`] = item.level === Number(level) ? 1 : 0
      })
      acc.push(newEntry)
    }
    return acc
  }, [] as any[])

  // Sort chartData by time in ascending order
  chartData.sort((a, b) => a.originalDate.getTime() - b.originalDate.getTime())

  // Remove the originalDate field as it's no longer needed
  chartData.forEach((entry) => delete entry.originalDate)

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
          
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="time" tickLine={false} tickMargin={10} axisLine={false} />
              <YAxis />
              <ChartTooltip content={<ChartTooltipContent />} />
              <ChartLegend content={<ChartLegendContent />} />
              {levels.map((level) => (
                <Line
                  key={`level${level}`}
                  type="monotone"
                  dataKey={`level${level}`}
                  stroke={logLevels[level]?.color || `hsl(${level * 60}, 70%, 50%)`}
                  strokeWidth={2}
                  dot={false}
                />
              ))}
            </LineChart>
          
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

