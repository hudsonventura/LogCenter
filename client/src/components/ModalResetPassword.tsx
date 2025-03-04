"use client"

import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useState } from "react"

import api from "@/services/api"
import { toast } from "sonner"

export function ModalResetPassword() {
	const [isOpen, setIsOpen] = useState(false)
	const [password, setPassword] = useState("")
	const [confirmPassword, setConfirmPassword] = useState("")

	const handleResetPassword = async () => {
		if (!password || !confirmPassword) {
			alert("Please fill in all fields.")
			return
		}

		if (password !== confirmPassword) {
			alert("Passwords do not match.")
			return
		}

		try {
			const response = await api.post("/ResetPassword", {
				password,
			})

			if (response.status === 200) {
				toast.success("Password reset successfully!")
				setIsOpen(false)
			} else {
				alert("Error resetting password. Please try again.")
			}
		} catch (error) {
			console.error("Error resetting password:", error)
			alert("Error resetting password. Please try again.")
		}
	}

	return (
		<Popover>
			<PopoverTrigger asChild>
				<span style={{ cursor: "pointer" }}>Reset Password</span>
			</PopoverTrigger>
			<PopoverContent className="p-4">
				<div className="grid gap-4 py-4">
					<div className="grid grid-cols-4 items-center gap-4">
						<Label htmlFor="password" className="text-right">
							New Password
						</Label>
						<Input
							id="password"
							type="password"
							value={password}
							onChange={(e) => setPassword(e.target.value)}
							className="col-span-3"
							placeholder="Enter new password"
						/>

						<Label htmlFor="confirmPassword" className="text-right">
							Confirm New Password
						</Label>
						<Input
							id="confirmPassword"
							type="password"
							value={confirmPassword}
							onChange={(e) => setConfirmPassword(e.target.value)}
							className="col-span-3"
							placeholder="Confirm new password"
						/>
					</div>
				</div>
				<div className="flex justify-end gap-4">
					<Button onClick={() => setIsOpen(false)}>Cancel</Button>
					<Button variant="default" onClick={handleResetPassword}>
						Reset
					</Button>
				</div>
			</PopoverContent>
		</Popover>
	)
}

