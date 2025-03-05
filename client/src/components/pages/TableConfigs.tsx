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
import HeaderBar from "../HeaderBar";
import {
	AlertDialog,
	AlertDialogAction,
	AlertDialogCancel,
	AlertDialogContent,
	AlertDialogDescription,
	AlertDialogFooter,
	AlertDialogHeader,
	AlertDialogTitle,
	AlertDialogTrigger,
} from "@/components/ui/alert-dialog"
import { format } from "date-fns";


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


	function runDelete(): void {
		api.delete(`/${tableName}`, {
			params: {
				before_date: new Date().toISOString() // or use the appropriate date
			}
		}).then((res) => {
			if (res.status === 200) {
				toast("Deleted!", {
					description: `Table ${tableName.toUpperCase()} deleted`,
				});
			} else {
				toast("Error!", {
					description: `Error deleting table ${tableName.toUpperCase()}`,
				});
			}
		});
	}

	function runVacummFull(): void {
		api.post(`/VacuumFull/${tableName}`, {

		}).then((res) => {
			if (res.status === 200) {
				toast("Deleted!", {
					description: `Table ${tableName.toUpperCase()} is goint to be vacuumed`,
				});
			} else {
				toast("Error!", {
					description: `Error vacuumming table ${tableName.toUpperCase()}`,
				});
			}
		});
	}

	function runVacumm(): void {
		api.post(`/Vacuum/${tableName}`, {

		}).then((res) => {
			if (res.status === 200) {
				toast("Deleted!", {
					description: `Table ${tableName.toUpperCase()} is goint to be vacuumed`,
				});
			} else {
				toast("Error!", {
					description: `Error vacuumming table ${tableName.toUpperCase()}`,
				});
			}
		});
	}

	return (
		<Form {...form}
		>

			<EnsureLogin />
			<HeaderBar />
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

									<AlertDialog>
										<AlertDialogTrigger>
											<Button type="button">
												RUN NOW
											</Button>
										</AlertDialogTrigger>
										<AlertDialogContent>
											<AlertDialogHeader>
												<AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
												<AlertDialogDescription>
													This action will delete all records older than {format(new Date(Date.now() - 1000 * 60 * 60 * 24 * 90), "yyyy-MM-dd")}
												</AlertDialogDescription>
											</AlertDialogHeader>
											<AlertDialogFooter>
												<AlertDialogCancel>Cancel</AlertDialogCancel>
												<AlertDialogAction onClick={() => runDelete()}>Continue</AlertDialogAction>
											</AlertDialogFooter>
										</AlertDialogContent>
									</AlertDialog>
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
									<AlertDialog>
										<AlertDialogTrigger>
											<Button type="button">
												RUN NOW
											</Button>
										</AlertDialogTrigger>
										<AlertDialogContent>
											<AlertDialogHeader>
												<AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
												<AlertDialogDescription>
														This action will do a VACUUM in your table. It may take a long time depending on the size of the table.
												</AlertDialogDescription>
											</AlertDialogHeader>
											<AlertDialogFooter>
												<AlertDialogCancel>Cancel</AlertDialogCancel>
												<AlertDialogAction onClick={() => runVacumm()}>Continue</AlertDialogAction>
											</AlertDialogFooter>
										</AlertDialogContent>
									</AlertDialog>
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
									<AlertDialog>
										<AlertDialogTrigger>
											<Button type="button" className="text-destructive">
												RUN NOW
											</Button>
										</AlertDialogTrigger>
										<AlertDialogContent>
											<AlertDialogHeader>
												<AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
												<AlertDialogDescription>
													<p className="text-red-500">
														This action will do a VACUUM FULL ANALYZE in your table. It will lock the table and no data can be written or read until the operation is finished. It may take a long time depending on the size of the table.
													</p>
												</AlertDialogDescription>
											</AlertDialogHeader>
											<AlertDialogFooter>
												<AlertDialogCancel>Cancel</AlertDialogCancel>
												<AlertDialogAction className="text-red-500" onClick={() => runVacummFull()}>I got it and go ahead!</AlertDialogAction>
											</AlertDialogFooter>
										</AlertDialogContent>
									</AlertDialog>
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
