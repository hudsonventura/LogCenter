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
  dateFrom: Date
  dateTo: Date
}

// Definição dos níveis de log
const logLevels = {
  1: { name: "Debug", color: "hsl(195, 70%, 50%)" },
  2: { name: "Info", color: "hsl(220, 70%, 50%)" },
  3: { name: "Warning", color: "hsl(45, 70%, 50%)" },
  4: { name: "Error", color: "hsl(0, 70%, 50%)" },
  5: { name: "Critical", color: "hsl(300, 70%, 50%)" },
}

export default function Component({ rawData, dateFrom, dateTo }: ComponentProps) {
  // Transforma os dados e agrupa por tempo
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

  // Ordena os dados por horário
  chartData.sort((a, b) => a.originalDate.getTime() - b.originalDate.getTime())

  // Remove campo auxiliar
  chartData.forEach((entry) => delete entry.originalDate)

  // Gera uma série contínua minuto a minuto
  const generateCompleteTimeSeries = (start: Date, end: Date) => {
    const timeSeries = new Map<string, any>()
    let current = new Date(start)

    while (current <= end) {
      const formattedTime = current.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
      timeSeries.set(formattedTime, {
        time: formattedTime,
        datetime: current.toISOString().slice(0, 10) + ' '+ current.toISOString().slice(11, 16),
        ...Object.fromEntries(Object.keys(logLevels).map((level) => [`level${level}`, 0])),
      })
      current.setMinutes(current.getMinutes() + 1)
    }
    return timeSeries
  }

  // Preenche os minutos ausentes
  const timeSeries = generateCompleteTimeSeries(dateFrom, dateTo)
  chartData.forEach((entry) => {
    if (timeSeries.has(entry.time)) {
      timeSeries.set(entry.time, { ...timeSeries.get(entry.time), ...entry })
    }
  })

  const completeChartData = Array.from(timeSeries.values())

  // Configuração dinâmica do gráfico
  const levels = Object.keys(logLevels).map(Number)
  const chartConfig: ChartConfig = levels.reduce((config, level) => {
    config[`level${level}`] = {
      label: logLevels[level].name,
      color: logLevels[level].color,
    }
    return config
  }, {} as ChartConfig)

  return (
    <Card>
      <CardHeader>
        <CardTitle>Logs by time</CardTitle>
        <CardDescription>
          {new Date(rawData[0]?.created_at).toLocaleDateString("en-US", {
            day: "2-digit",
            month: "numeric",
            year: "numeric",
          })}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <ChartContainer config={chartConfig}>
          <LineChart data={completeChartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="datetime" tickLine={false} tickMargin={10} axisLine={false} 
              
            />
            <YAxis />
            <ChartTooltip content={<ChartTooltipContent />} />
            <ChartLegend content={<ChartLegendContent />} />
            {levels.map((level) => (
              <Line
                key={`level${level}`}
                type="monotone"
                dataKey={`level${level}`}
                stroke={logLevels[level].color}
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
