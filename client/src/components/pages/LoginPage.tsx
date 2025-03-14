import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useForm } from "react-hook-form"
import { toast } from "sonner"
import api from "@/services/api";
import { useNavigate } from "react-router-dom"
import { useEffect } from "react"



export default function LoginPage() {
	const { register, handleSubmit } = useForm()
	const navigate = useNavigate();

	useEffect(() => {
		const token = sessionStorage.getItem("token");
		if (token) {
			navigate("/tables");
		}
	}, []);

	const onSubmit = async (data: { email: string; password: string }) => {
		try {
			const response = await api.post("/Login", JSON.stringify(data), {
				headers: {
					"Content-Type": "application/json",
				},
			})
			const token = await response.data;
			//toast.success(`Welcome back, ${token}!`)
			sessionStorage.setItem("token", token);
			// Redirect to dashboard or something
			navigate("/tables");
		} catch (error) {
			toast.error(error.message)
		}
	}

	return (
		<div >
			
			<div className="flex min-h-svh w-full items-center justify-center p-6 md:p-10">
				<div className="w-full max-w-sm">
					{/* <div className="relative flex justify-center">
						<img
							src="/logo.png"
							alt="Login illustration"
							className="items-center md:p-20"
						/>
					</div> */}
					<h1 className="text-4xl font-bold text-center mb-4">
						LogCenter
					</h1>
					
					<div className={cn("flex flex-col gap-6")}>
						<Card>
							
							<CardHeader>
								<CardTitle className="text-2xl">Login</CardTitle>
								<CardDescription>
									Enter your email below to login to your account
								</CardDescription>
							</CardHeader>
							<CardContent>
								<form onSubmit={handleSubmit(onSubmit)}>
									<div className="flex flex-col gap-6">
										<div className="grid gap-2">
											<Label htmlFor="email">Email</Label>
											<Input
												id="email"
												type="email"
												placeholder="m@example.com"
												required
												{...register("email")}
											/>
										</div>
										<div className="grid gap-2">
											<div className="flex items-center">
												<Label htmlFor="password">Password</Label>
												<a
													href="#"
													className="ml-auto inline-block text-sm underline-offset-4 hover:underline"
												>
													Forgot your password?
												</a>
											</div>
											<Input
												id="password"
												type="password"
												required
												{...register("password")}
											/>
										</div>
										<Button type="submit" className="w-full">
											Login
										</Button>
									</div>
								</form>
							</CardContent>
						</Card>
					</div>
				</div>
			</div>
		</div>
	)
}

