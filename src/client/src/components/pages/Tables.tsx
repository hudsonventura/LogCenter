import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { useNavigate } from "react-router-dom";
import api from "@/services/api";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuGroup,
	DropdownMenuItem,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { MoreHorizontal } from "lucide-react";
import HeaderBar from "@/components/HeaderBar";
import EnsureLogin from "../EnsureLogin";
export interface TableData {
	id: string;
	table_name: string;
	size: number;
}

export function Tables() {
	const [data, setData] = useState<TableData[]>([]);
	const navigate = useNavigate();

	useEffect(() => {
		listTables();
	}, []);

	const listTables = async () => {
		try {
			const response = await api.get("/TablesConfig");
			setData(response.data);
		} catch (error) {
			console.log(error);
		}
	};

	const consultarTabela = (tabela: string) => {
		navigate("/table-logs", { state: { tabela } });
	};

	const goToConfigsTable = (tabela: string) => {
		navigate(`/table-configs/${tabela}`, { state: { tabela } });
	};

	const formatSize = (size: number) => {
		if (size < 1024) return `${size} Bytes`;
		const i = Math.floor(Math.log(size) / Math.log(1024));
		const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
		return `${(size / Math.pow(1024, i)).toFixed(2)} ${sizes[i]}`;
	};

	return (
		<>
			<EnsureLogin />
			<HeaderBar />
			<h1 className="font-bold mb-3 text-center mt-3">Tables</h1>


			<div className="flex flex-wrap gap-4 text-center ">
				{data.map((item, index) => (
					<Card key={index} className="w-[300px]">
						<div className="flex justify-end">
							<DropdownMenu>
								<DropdownMenuTrigger asChild>
									<Button variant="ghost" className="h-8 w-8 p-0">
										<span className="sr-only">Open menu</span>
										<MoreHorizontal className="h-4 w-4" />
									</Button>
								</DropdownMenuTrigger>
								<DropdownMenuContent className="w-56">
									<DropdownMenuGroup>
										<DropdownMenuItem onClick={() => goToConfigsTable(item.table_name)}>
											Configurations
										</DropdownMenuItem>
									</DropdownMenuGroup>
								</DropdownMenuContent>
							</DropdownMenu>
						</div>
						<CardHeader>
							<CardTitle>{item?.table_name || "Tabela sem nome"}</CardTitle>
							Size: {formatSize(item?.size || 0)}
						</CardHeader>
						<CardFooter className="flex justify-between gap-2">
							<Button
								className="w-full"
								onClick={() => consultarTabela(item.table_name)}
							>
								See Logs
							</Button>
						</CardFooter>
					</Card>
				))}
			</div>

		</>
	);
}
