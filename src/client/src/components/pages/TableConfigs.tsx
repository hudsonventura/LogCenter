import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import {
	Form,
	FormControl,
	FormDescription,
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
import { useLocation, useNavigate } from "react-router-dom";
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
import { ArrowLeft } from "lucide-react";
import cronstrue from "cronstrue";

function getCronFeedback(expression: string) {
	const trimmedExpression = expression.trim();

	if (!trimmedExpression) {
		return {
			isValid: false,
			message: "Type a cron expression with 5 parts, like 0 0 * * *.",
		};
	}

	try {
		return {
			isValid: true,
			message: cronstrue.toString(trimmedExpression, {
				throwExceptionOnParseError: true,
			}),
		};
	} catch (error) {
		return {
			isValid: false,
			message: error instanceof Error ? error.message : "Invalid cron expression.",
		};
	}
}


const FormSchema = z.object({
	delete: z.boolean().default(false),
	delete_input: z.coerce.number().int().min(0, {
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
}).superRefine((data, ctx) => {
	if (data.delete && data.delete_input <= 0) {
		ctx.addIssue({
			code: z.ZodIssueCode.custom,
			path: ["delete_input"],
			message: "Inform the number of days",
		});
	}
});

export function TableConfigs() {
	const [tableName, setTableName] = useState("");
	const [isDropConfirmOpen, setIsDropConfirmOpen] = useState(false);
	const [isDropSecondConfirmOpen, setIsDropSecondConfirmOpen] = useState(false);
	const [isDropFinalConfirmOpen, setIsDropFinalConfirmOpen] = useState(false);
	const location = useLocation();
	const navigate = useNavigate();
	const { pathname } = location;

	useEffect(() => {
		const tableName = pathname.split("/")[2];
		setTableName(tableName);

		api.get(`/TablesConfig/${tableName}`).then(({ data }) => {
			form.reset(data);
			console.log(data);
		});
	}, []);

	const form = useForm<z.infer<typeof FormSchema>>({
		resolver: zodResolver(FormSchema),
		defaultValues: {
			delete: false,
			delete_input: 0,
			vacuum: false,
			vacuum_input: "0 0 1 1 *",
			vacuum_full: false,
			vacuum_full_input: "0 0 1 1 *",
		},
	});
	const deleteEnabled = form.watch("delete");
	const vacuumEnabled = form.watch("vacuum");
	const vacuumFullEnabled = form.watch("vacuum_full");
	const vacuumCronFeedback = getCronFeedback(form.watch("vacuum_input"));
	const vacuumFullCronFeedback = getCronFeedback(form.watch("vacuum_full_input"));

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
				const rowsAffected = (res.data as { rows_affected?: number } | undefined)?.rows_affected;
				toast("Deleted!", {
					description: rowsAffected !== undefined
						? `${rowsAffected} rows deleted from ${tableName.toUpperCase()}`
						: `Table ${tableName.toUpperCase()} deleted`,
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

	function runDropTable(): void {
		api.delete(`/Drop/${tableName}`, {
		}).then((res) => {
			if (res.status === 200) {
				toast("Dropped!", {
					description: `Table ${tableName.toUpperCase()} was dropped`,
				});
			} else {
				toast("Error!", {
					description: `Error dropping table ${tableName.toUpperCase()}`,
				});
			}
		});
	}

	return (
		<Form {...form}
		>

			<EnsureLogin />
			<HeaderBar />
			<div className="mt-5 mb-4 flex items-center justify-center gap-3">
				<Button
					type="button"
					variant="outline"
					size="icon"
					aria-label="Back to tables"
					onClick={() => navigate("/tables")}
				>
					<ArrowLeft className="h-4 w-4" />
				</Button>
				<h1 className="text-3xl font-bold text-center">
					Table {tableName.toUpperCase()} Configs
				</h1>
			</div>

			<form
				onSubmit={form.handleSubmit(onSubmit)}
				className="flex flex-col flex-grow justify-center items-center"
			>
				<div className="flex flex-wrap justify-center gap-5 p-5">
					<div className="flex p-1 w-[320px]">
						<FormField
							control={form.control}
							name="delete"
							render={({ field }) => (
								<FormItem className="flex h-full w-full flex-col justify-between rounded-lg border px-4 py-5 shadow-sm">
									<div className="flex flex-1 flex-col">
										<div className="mb-5 flex items-center justify-between">
											<FormControl>
												<Switch id="delete" onCheckedChange={field.onChange} checked={field.value} />
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
															<div className="max-w-xs">
																<p>
																	Delete is a way to remove rows from a table.
																</p>
																<p>
																	Delete will remove the rows from the table and also remove the index.
																</p>
																<p>
																	<span className="text-red-500">
																		You can set the number of days to delete, but data cannot be recovered.
																	</span>
																</p>
															</div>
														</TooltipContent>
													</Tooltip>
												</TooltipProvider>
											</FormLabel>
										</div>
										<FormControl className="mb-4 min-h-24">
											<Label htmlFor="delete" className="text-sm leading-5">
												Remove rows older than days
											</Label>
										</FormControl>

										<div className="flex items-center gap-3">
											<FormField
												control={form.control}
												name="delete_input"
												render={({ field }) => (
													<Input
														type="number"
														placeholder="Num. of days"
														disabled={!deleteEnabled}
														className="flex-1"
														{...field}
														value={field.value ?? ""}
														onChange={(event) => field.onChange(event.target.value)}
													/>
												)}
											/>
											<FormLabel className="w-10 shrink-0 text-sm text-muted-foreground">days</FormLabel>
										</div>
									</div>
									<p className="mt-4 text-center text-sm text-muted-foreground">
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

					<div className="flex p-1 w-[320px]">
						<FormField
							control={form.control}
							name="vacuum"
							render={({ field }) => (
								<FormItem className="flex h-full w-full flex-col justify-between rounded-lg border px-4 py-5 shadow-sm">
									<div className="flex flex-1 flex-col">
										<div className="mb-5 flex items-center justify-between">
											<FormControl>
												<Switch id="vacuum" onCheckedChange={field.onChange} checked={field.value} />
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
															<p>VACUUM is a database maintenance operation that reclaims storage occupied by dead tuples.</p>
															<p>When updates or deletes occur on a table, the old versions of rows are retained to support transactional features.</p>
															<p>Over time, these dead rows can accumulate and lead to wasted space and degraded performance.</p>
															<p>By running VACUUM, the database cleans up these dead rows, thus freeing up space and optimizing database performance by allowing the reused space for new data insertion.</p>
														</TooltipContent>
													</Tooltip>
												</TooltipProvider>
											</FormLabel>
										</div>
										<FormControl className="mb-4 min-h-24">
											<Label htmlFor="vacuum" className="text-sm leading-5">
												VACUUM cleans dead rows,freeing up space for reuse and improving performance
											</Label>
										</FormControl>
										<FormField
											control={form.control}
											name="vacuum_input"
											render={({ field: { onChange, ...field } }) => (
												<div className="space-y-2">
													<Input
														type="text"
														placeholder="0 0 * * *"
														disabled={!vacuumEnabled}
														className="w-full"
														{...field}
														onChange={(e) => {
															onChange(e.target.value.toUpperCase());
														}}
													/>
													<FormDescription
														className={
															vacuumCronFeedback.isValid
																? "text-emerald-600"
																: "text-destructive"
														}
															>
																{vacuumCronFeedback.message}
															</FormDescription>
												</div>
											)}
										/>
										<p className="mt-1 text-center text-sm text-muted-foreground">
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

					<div className="flex p-1 w-[320px]">
						<FormField
							control={form.control}
							name="vacuum_full"
							render={({ field }) => (
								<FormItem className="flex h-full w-full flex-col justify-between rounded-lg border px-4 py-5 shadow-sm">
									<div className="flex flex-1 flex-col">
										<div className="mb-5 flex items-center justify-between align-center">
											<FormControl>
												<Switch
													id="vacuum_full"
													onCheckedChange={field.onChange}
													checked={field.value}
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
															VACUUM FULL ANALYZE is a PostgreSQL operation that performs two key actions: 
															<p>
																it compacts the table to reclaim disk space
															</p>
															<p>
																and analyzes the table to update statistics
															</p>
															<p>
																This process is resource-intensive as it requires exclusive access, meaning the table is locked, preventing any read or write operations during the process.
															</p>
															<p>
																The compaction helps in reducing the physical disk space usage,
															</p>
															<p>
																while the analysis updates statistics that the query planner uses to optimize query execution plans.
															</p>
															<p>
																It is crucial for maintaining database performance but should be used with caution as it can temporarily halt access to the table.
															</p>
															<p>
																Note: The table will be locked, and no data can be read or written during this operation.
															</p>
														</TooltipContent>
													</Tooltip>
												</TooltipProvider>
											</FormLabel>
										</div>
										<FormControl className="mb-4 min-h-24">
											<Label htmlFor="vacuum_full" className="text-sm leading-5">
												Compacts the table, reclaiming disk space, but requires
												exclusive access. 
												<p className="mt-1 text-destructive">
													It is going to LOCK THE TABLE and no data can be
													written or read until finished
												</p>
											</Label>
										</FormControl>
										<FormField
											control={form.control}
											name="vacuum_full_input"
											render={({ field: { onChange, ...field } }) => (
												<div className="space-y-2">
													<Input
														type="text"
														placeholder="0 0 * * *"
														disabled={!vacuumFullEnabled}
														className="w-full"
														{...field}
														onChange={(e) => {
															onChange(e.target.value.toUpperCase());
														}}
													/>
													<FormDescription
														className={
															vacuumFullCronFeedback.isValid
																? "text-emerald-600"
																: "text-destructive"
														}
															>
																{vacuumFullCronFeedback.message}
															</FormDescription>
												</div>
											)}
										/>
										<p className="mt-1 text-center text-sm text-muted-foreground">
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
					<AlertDialog open={isDropConfirmOpen} onOpenChange={setIsDropConfirmOpen}>
						<AlertDialogTrigger asChild>
							<Button type="button" className="text-destructive">
								DROP TABLE
							</Button>
						</AlertDialogTrigger>
						<AlertDialogContent>
							<AlertDialogHeader>
								<AlertDialogTitle>Whoa there, chaos engineer.</AlertDialogTitle>
								<AlertDialogDescription>
									<p className="text-red-500">
										You are one click away from sending this table to the great database in the sky. If this was just a dramatic misclick, now is a beautiful time to stop.
									</p>
								</AlertDialogDescription>
							</AlertDialogHeader>
							<AlertDialogFooter>
								<AlertDialogCancel>Cancel</AlertDialogCancel>
								<AlertDialogAction
									className="text-red-500"
									onClick={() => {
										setIsDropConfirmOpen(false);
										setIsDropSecondConfirmOpen(true);
									}}
								>
									Yes, keep the drama going
								</AlertDialogAction>
							</AlertDialogFooter>
						</AlertDialogContent>
					</AlertDialog>
					<AlertDialog open={isDropSecondConfirmOpen} onOpenChange={setIsDropSecondConfirmOpen}>
						<AlertDialogContent>
							<AlertDialogHeader>
								<AlertDialogTitle>Second checkpoint</AlertDialogTitle>
								<AlertDialogDescription>
									<p className="text-red-500">
										This action will drop the entire table and all of its data. There is no undo and no recovery path here.
									</p>
								</AlertDialogDescription>
							</AlertDialogHeader>
							<AlertDialogFooter>
								<AlertDialogCancel>Cancel</AlertDialogCancel>
								<AlertDialogAction
									className="text-red-500"
									onClick={() => {
										setIsDropSecondConfirmOpen(false);
										setIsDropFinalConfirmOpen(true);
									}}
								>
									I still want to continue
								</AlertDialogAction>
							</AlertDialogFooter>
						</AlertDialogContent>
					</AlertDialog>
					<AlertDialog open={isDropFinalConfirmOpen} onOpenChange={setIsDropFinalConfirmOpen}>
						<AlertDialogContent>
							<AlertDialogHeader>
								<AlertDialogTitle>Final confirmation</AlertDialogTitle>
								<AlertDialogDescription>
									<p className="text-red-500">
										Last check: the table <strong>{tableName.toUpperCase()}</strong> will be permanently removed right now.
									</p>
								</AlertDialogDescription>
							</AlertDialogHeader>
							<AlertDialogFooter>
								<AlertDialogCancel>Cancel</AlertDialogCancel>
								<AlertDialogAction
									className="text-red-500"
									onClick={() => {
										setIsDropFinalConfirmOpen(false);
										runDropTable();
									}}
								>
									Drop it for real
								</AlertDialogAction>
							</AlertDialogFooter>
						</AlertDialogContent>
					</AlertDialog>
					<Button type="button" onClick={() => window.history.back()}>
						Voltar
					</Button>
					<Button type="submit">Salvar</Button>
				</div>
			</form>
		</Form>
	);
}
