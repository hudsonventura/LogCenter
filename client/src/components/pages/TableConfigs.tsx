
import { zodResolver } from "@hookform/resolvers/zod"
import { useForm } from "react-hook-form"
import { z } from "zod"
 
import { toast } from "sonner";
import { Button } from "@/components/ui/button"
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form"
import { Input } from "@/components/ui/input"


import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
  } from "@/components/ui/tooltip"
  import { QuestionMarkCircledIcon } from "@radix-ui/react-icons"


import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import { useLocation } from "react-router-dom";
import { useEffect, useState } from "react";

const FormSchema = z.object({
    delete: z.boolean().default(false),
    delete_input: z.string().min(1, {
        message: "The day must be a number",
    }),

     vacuum: z.boolean().default(false),
     vacuum_input: z.string().min(1, {
         message: "The day must be a cron expression",
     }),

     vacuum_full: z.boolean().default(false),
     vacuum_full_input: z.string().min(1, {
         message: "The day must be a cron expression",
     }),
    
  })

export function TableConfigs() {
    const [tableName, setTableName] = useState("");
    const location = useLocation();
    const { pathname } = location;
    

    useEffect(() => {
        const tableName = pathname.split("/")[2];
        setTableName(tableName);
    }, [])
    
    const form = useForm<z.infer<typeof FormSchema>>({
        resolver: zodResolver(FormSchema),

      })
     
      function onSubmit(data: z.infer<typeof FormSchema>) {
        toast(
          "You submitted the following values:",{
          description: (
            <pre className="mt-2 w-[340px] rounded-md bg-slate-950 p-4">
              <code className="text-white">{JSON.stringify(data, null, 2)}</code>
            </pre>
          )
        })

        console.log("Post para /config/" + tableName)

        console.log(JSON.stringify(data, null, 2))
      }



    return (
        <Form {...form}>
            <h1 className="text-3xl font-bold mb-4">Table {tableName.toUpperCase()} Configs <small className="text-muted-foreground">BE CAREFUL HERE</small></h1>
            <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-col space-y-6">

            <div className="grid grid-cols-3 gap-4">

                <div className="flex p-4">
                    <div className="flex ">
                        <FormField control={form.control} name="delete"
                            render={({ field }) => (
                                <FormItem className="flex flex-row items-center justify-between rounded-lg border p-3 shadow-sm">
                                    
                                    <div className="space-y-0.5">
                                        <h1 className="text-2xl font-bold mb-4">
                                        <FormControl>
                                            <Switch id="delete" onCheckedChange={field.onChange} />
                                        </FormControl>
                                        DELETE
                                            
                                        <FormLabel> 
                                            <TooltipProvider>
                                                <Tooltip>
                                                    <TooltipTrigger asChild>
                                                    <Button variant="link" size="icon">
                                                        <QuestionMarkCircledIcon className="h-4 w-4" />
                                                    </Button>
                                                    </TooltipTrigger>
                                                    <TooltipContent>
                                                        It refers to removing records from a database table that are older than a specified time or date, typically based on a timestamp column created_at. This is often used for data cleanup.
                                                    </TooltipContent>
                                                </Tooltip>
                                            </TooltipProvider>
                                        </FormLabel>
                                        </h1>

                                        <FormControl>
                                            <Label htmlFor="delete">Able DELETE rows table older than </Label>
                                        </FormControl>
                                            
                                        
                                        <FormField
                                            control={form.control}
                                            name="delete_input"
                                            render={({ field }) => (
                                                <Input type="number" placeholder="Num. of days" {...field} />
                                            )}
                                        />
                                        <FormLabel>days</FormLabel>
                                        <p className="text-sm text-muted-foreground">(runs every midnight)</p>
                                        <br />
                                        <Button type="button" onClick={() => alert()} className="self-end">RUN NOW</Button>
                                    </div>
                                </FormItem>
                            )}
                        />
                    </div>
                </div>





                
                <div className="flex items-center space-x-2 justify-between p-4">
                    <div className="flex items-center space-x-2">
                        <FormField control={form.control} name="vacuum"
                            render={({ field }) => (
                                <FormItem className="flex flex-row items-center justify-between rounded-lg border p-3 shadow-sm">
                                    <div className="space-y-0.5">
                                        <h1 className="text-2xl font-bold mb-4">
                                        <FormControl>
                                            <Switch id="vacuum" onCheckedChange={field.onChange} />
                                        </FormControl>
                                        VACUUM
                                        <FormLabel> 
                                            <TooltipProvider>
                                                <Tooltip>
                                                    <TooltipTrigger asChild>
                                                    <Button variant="link" size="icon">
                                                        <QuestionMarkCircledIcon className="h-4 w-4" />
                                                    </Button>
                                                    </TooltipTrigger>
                                                    <TooltipContent>
                                                    VACUUM in PostgreSQL cleans up dead rows, freeing space for reuse and improving performance
                                                    </TooltipContent>
                                                </Tooltip>
                                            </TooltipProvider>
                                        </FormLabel>
                                        </h1>

                                        <FormControl>
                                            <Label htmlFor="vacuum">Able VACUUM to remove deleted rows and reuse the space </Label>
                                        </FormControl>
                                            
                                        
                                        <FormField
                                            control={form.control}
                                            name="vacuum_input"
                                            render={({ field: { onChange, ...field } }) => (
                                                <Input
                                                    type="text"
                                                    placeholder="0 0 * * *"
                                                    {...field}
                                                    onChange={(e) => {
                                                        const value = e.target.value.toUpperCase();
                                                        const cronPattern = /^(\d|\*)\s(\d|\*)\s(\d|\*)\s(\d|\*)\s(\d|\*)$/;
                                                        if (value.match(cronPattern)) {
                                                            onChange(e);
                                                        }
                                                    }}
                                                />
                                            )}
                                        />
                                        <p className="text-sm text-muted-foreground">(cron schedulle)</p>
                                        <br />
                                        <Button type="button" onClick={() => alert()} className="self-end">RUN NOW</Button>
                                    </div>
                                </FormItem>
                            )}
                        />
                    </div>
                </div>
                
                <div className="flex items-center space-x-2 justify-between ">
                    <div className="flex items-center space-x-2">
                        <FormField control={form.control} name="vacuum_full"
                            render={({ field }) => (
                                <FormItem className="flex flex-row items-center justify-between rounded-lg border p-3 shadow-sm">
                                    <div className="space-y-0.5">
                                        <h1 className="text-2xl font-bold mb-4 text-destructive">
                                        <FormControl>
                                            <Switch id="vacuum_full" onCheckedChange={field.onChange} />
                                        </FormControl>
                                    VACUUM FULL ANALYSE
                                        <FormLabel> 
                                            <TooltipProvider>
                                                <Tooltip>
                                                    <TooltipTrigger asChild>
                                                    <Button variant="link" size="icon">
                                                        <QuestionMarkCircledIcon className="h-4 w-4" />
                                                    </Button>
                                                    </TooltipTrigger>
                                                    <TooltipContent>
                                                        VACUUM FULL, the table is fully compacted, reclaiming disk space but requires exclusive access, making it more intensive.
                                                        <p>It is going to LOCK THE TABLE and no data can be write or read.</p>
                                                    </TooltipContent>
                                                </Tooltip>
                                            </TooltipProvider>
                                        </FormLabel>
                                        </h1>

                                        <FormControl>
                                            <Label htmlFor="vacuum_full">Able VACUUM FULL ANALYSE to remove deleted rows and claim free space on disk</Label>
                                        </FormControl>
                                            
                                        
                                        <FormField
                                            control={form.control}
                                            name="vacuum_full_input"
                                            render={({ field: { onChange, ...field } }) => (
                                                <Input
                                                    type="text"
                                                    placeholder="0 0 * * *"
                                                    {...field}
                                                    onChange={(e) => {
                                                        const value = e.target.value.toUpperCase();
                                                        const cronPattern = /^(\d|\*)\s(\d|\*)\s(\d|\*)\s(\d|\*)\s(\d|\*)$/;
                                                        if (value.match(cronPattern)) {
                                                            onChange(e);
                                                        }
                                                    }}
                                                />
                                            )}
                                        />
                                        <p className="text-sm text-muted-foreground">(cron schedulle)</p>
                                        <br />
                                        <Button type="button" onClick={() => alert()} className="self-end text-destructive">RUN NOW</Button>
                                    </div>
                                </FormItem>
                            )}
                        />
                    </div>
                </div>


            </div>

            <div className="flex items-center justify-between space-x-2">
                <Button type="button" onClick={() => window.history.back()} className="self-start">Back</Button>
                <Button type="submit" className="self-end">Save</Button>
            </div>
            </form>
        </Form>
  )

}

