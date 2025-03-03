import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { QuestionMarkCircledIcon } from "@radix-ui/react-icons";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { useLocation } from "react-router-dom";
import { useEffect, useState } from "react";
import api from "@/services/api";
import EnsureLogin from "../EnsureLogin";

const FormSchema = z.object({
  delete: z.boolean().default(false),
  delete_input: z.string().min(1, {
    message: "Must be a number",
  }),

  vacuum: z.boolean().default(false),
  vacuum_input: z.string().min(1, {
    message: "Must be a cron expression",
  }),

  vacuum_full: z.boolean().default(false),
  vacuum_full_input: z.string().min(1, {
    message: "Must be a cron expression",
  }),
});

export function TableConfigs() {
  const [tableName, setTableName] = useState("");
  const location = useLocation();
  const { pathname } = location;

  useEffect(() => {
    const tableName = pathname.split("/")[2];
    setTableName(tableName);
  }, []);

  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
  });

  function onSubmit(data: z.infer<typeof FormSchema>) {
    const updateTable = async () => {
      try {
        await api.put(`/Config/${tableName}`, JSON.stringify(data, null, 2));
        toast("Saved!", {
          description: (
            <pre className="mt-2 w-[340px] rounded-md bg-slate-950 p-4">
              <code className="text-white">
                {JSON.stringify(data, null, 2)}
              </code>
            </pre>
          ),
        });
      } catch (error: unknown) {
        console.log(error);
        toast("Error!", {
          description: (
            <pre className="mt-2 w-[340px] rounded-md bg-slate-950 p-4">
              <code className="text-white">
                {(error as { response: { data: string } }).response.data}
              </code>
            </pre>
          ),
        });
      }
    };

    updateTable();
  }

  return (
    <Form {...form}
    >
      <EnsureLogin />
      <h1 className="text-3xl font-bold mb-4 text-center mt-5">
        Table {tableName.toUpperCase()} Configs{" "}
      </h1>

      <form
        onSubmit={form.handleSubmit(onSubmit)}
        className="flex flex-col flex-grow justify-center items-center"
      >
        <div className="flex flex-wrap justify-center gap-4 p-5">
          <div className="flex p-1 w-[300px]">
            <FormField
              control={form.control}
              name="delete"
              render={({ field }) => (
                <FormItem className="flex flex-col justify-between rounded-lg border p-4 shadow-sm h-full">
                  <div className="flex flex-col">
                    <div className="flex items-center justify-between">
                      <FormControl>
                        <Switch id="delete" onCheckedChange={field.onChange} />
                      </FormControl>
                      <h1 className="text-2xl font-bold mb-2">DELETE</h1>
                      <FormLabel>
                        <TooltipProvider>
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <Button variant="link" size="icon">
                                <QuestionMarkCircledIcon className="h-4 w-4" />
                              </Button>
                            </TooltipTrigger>
                            <TooltipContent>
                              Refere-se à remoção de registros de um banco de
                              dados tabela que é mais antiga que um horário
                              especificado ou data, normalmente com base em uma
                              coluna de carimbo de data/hora criado_em. Isso
                              geralmente é usado para limpeza de dados.
                            </TooltipContent>
                          </Tooltip>
                        </TooltipProvider>
                      </FormLabel>
                    </div>
                    <FormControl className="mb-5 mt-5">
                      <Label htmlFor="delete">
                        DELETE linhas tabela anterior a{" "}
                      </Label>
                    </FormControl>

                    <div className="flex justify-between items-center">
                      <FormField
                        control={form.control}
                        name="delete_input"
                        render={({ field }) => (
                          <Input
                            type="number"
                            placeholder="Num. of days"
                            {...field}
                          />
                        )}
                      />
                      <FormLabel className="p-2">days</FormLabel>
                    </div>
                  </div>
                  <p className="text-sm text-muted-foreground mb-2 text-center">
                    (runs every midnight)
                  </p>
                  <Button type="button" onClick={() => alert()}>
                    RUN NOW
                  </Button>
                </FormItem>
              )}
            />
          </div>

          <div className="flex p-1 w-[300px]">
            <FormField
              control={form.control}
              name="vacuum"
              render={({ field }) => (
                <FormItem className="flex flex-col justify-between rounded-lg border p-4 shadow-sm h-full">
                  <div className="flex flex-col">
                    <div className="flex items-center justify-between">
                      <FormControl>
                        <Switch id="vacuum" onCheckedChange={field.onChange} />
                      </FormControl>
                      <h1 className="text-2xl font-bold mb-2">VACUUM</h1>
                      <FormLabel>
                        <TooltipProvider>
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <Button variant="link" size="icon">
                                <QuestionMarkCircledIcon className="h-4 w-4" />
                              </Button>
                            </TooltipTrigger>
                            <TooltipContent>
                              VACUUM no PostgreSQL limpa linhas mortas,
                              liberando espaço para reutilização e melhoria de
                              desempenho.
                            </TooltipContent>
                          </Tooltip>
                        </TooltipProvider>
                      </FormLabel>
                    </div>
                    <FormControl className="mt-5 mb-2">
                      <Label htmlFor="vacuum">
                        Tabela VACUUM para remover linhas excluídas e reutilizar
                        o espaço{" "}
                      </Label>
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
                            const cronPattern =
                              /^(\d|\*)\s(\d|\*)\s(\d|\*)\s(\d|\*)\s(\d|\*)$/;
                            if (value.match(cronPattern)) {
                              onChange(e);
                            }
                          }}
                        />
                      )}
                    />
                    <p className="text-sm text-muted-foreground text-center mt-2">
                      (cron schedule, consider +0UTC)
                    </p>
                  </div>
                  <Button type="button" onClick={() => alert()}>
                    RUN NOW
                  </Button>
                </FormItem>
              )}
            />
          </div>

          <div className="flex p-1 w-[300px]">
            <FormField
              control={form.control}
              name="vacuum_full"
              render={({ field }) => (
                <FormItem className="flex flex-col justify-between rounded-lg border p-4 shadow-sm h-full">
                  <div className="flex flex-col">
                    <div className="flex items-center justify-between align-center">
                      <FormControl>
                        <Switch
                          id="vacuum_full"
                          onCheckedChange={field.onChange}
                        />
                      </FormControl>

                      <h1 className="text-xl font-bold mb-2 text-destructive text-center">
                        VACUUM FULL
                      </h1>

                      <FormLabel>
                        <TooltipProvider>
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <Button variant="link" size="icon">
                                <QuestionMarkCircledIcon className="h-4 w-4" />
                              </Button>
                            </TooltipTrigger>
                            <TooltipContent>
                              VACUUM FULL, a mesa está totalmente compactada,
                              recuperando espaço em disco, mas requer
                              exclusividade acesso, tornando-o mais intensivo.
                              <p>
                                It is going to LOCK THE TABLE and no data can be
                                written or read.
                              </p>
                            </TooltipContent>
                          </Tooltip>
                        </TooltipProvider>
                      </FormLabel>
                    </div>
                    <FormControl className="mb-2 mt-2">
                      <Label htmlFor="vacuum_full">
                        Table VACUUM FULL ANALYZE para remover linhas excluídas
                        e reivindicar espaço livre no disco
                      </Label>
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
                            const cronPattern =
                              /^(\d|\*)\s(\d|\*)\s(\d|\*)\s(\d|\*)\s(\d|\*)$/;
                            if (value.match(cronPattern)) {
                              onChange(e);
                            }
                          }}
                        />
                      )}
                    />
                    <p className="text-sm text-muted-foreground text-center mt-2">
                      (cron schedule, consider +0UTC)
                    </p>
                  </div>
                  <Button
                    type="button"
                    onClick={() => alert()}
                    className="text-destructive"
                  >
                    RUN NOW
                  </Button>
                </FormItem>
              )}
            />
          </div>
        </div>

        {/* Botões no final da página */}
        <div className="flex items-center justify-center mt-5 gap-5 mb-5">
          <Button type="button" onClick={() => window.history.back()}>
            Voltar
          </Button>
          <Button type="submit">Salvar</Button>
        </div>
      </form>
    </Form>
  );
}
