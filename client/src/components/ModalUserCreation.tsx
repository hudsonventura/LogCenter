"use client"

import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useState } from "react"

import api from "@/services/api"
import { toast } from "sonner"

export function ModalUserCreation() {
	const [isOpen, setIsOpen] = useState(false)
	const [name, setName] = useState("")
	const [email, setEmail] = useState("")
	const [password, setPassword] = useState("")
	const [confirmPassword, setConfirmPassword] = useState("")

	const handleCreateUser = async () => {
		if (!name || !email || !password || !confirmPassword) {
			alert("Please fill in all fields.")
			return
		}

		if (password !== confirmPassword) {
			alert("Passwords do not match.")
			return
		}

		try {
			const response = await api.post("/CreateUser", {
				name: name,
				email: email,
				password: password,
			})

			if (response.status === 200) {
				toast.success("User created successfully!")
				setIsOpen(false)
			} else {
				alert("Error creating user. Please try again.")
			}
		} catch (error) {
			console.error("Error creating user:", error)
			alert("Error creating user. Please try again.")
		}
	}

	return (
		<Popover>
			<PopoverTrigger asChild>
				<span style={{ cursor: "pointer" }}>Create User</span>
			</PopoverTrigger>
			<PopoverContent className="p-4">
				<div className="grid gap-4 py-4">
					<div className="grid grid-cols-4 items-center gap-4">
						<Label htmlFor="name" className="text-right">
							Name
						</Label>
						<Input
							id="name"
							value={name}
							onChange={(e) => setName(e.target.value)}
							className="col-span-3"
							placeholder="Enter name"
						/>

						<Label htmlFor="email" className="text-right">
							Email
						</Label>
						<Input
							id="email"
							value={email}
							onChange={(e) => setEmail(e.target.value)}
							className="col-span-3"
							placeholder="Enter email"
						/>

						<Label htmlFor="password" className="text-right">
							Password
						</Label>
						<Input
							id="password"
							type="password"
							value={password}
							onChange={(e) => setPassword(e.target.value)}
							className="col-span-3"
							placeholder="Enter password"
						/>

						<Label htmlFor="confirmPassword" className="text-right">
							Confirm Password
						</Label>
						<Input
							id="confirmPassword"
							type="password"
							value={confirmPassword}
							onChange={(e) => setConfirmPassword(e.target.value)}
							className="col-span-3"
							placeholder="Confirm password"
						/>
					</div>
				</div>
				<div className="flex justify-end gap-4">
					<Button onClick={() => setIsOpen(false)}>Cancel</Button>
					<Button variant="default" onClick={handleCreateUser}>
						Create
					</Button>
				</div>
			</PopoverContent>
		</Popover>
	)
}

