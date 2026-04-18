"use client"

import { useState } from "react"

import api from "@/services/api"
import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { toast } from "sonner"

import { ComboBoxTimeExpiration } from "./ComboBoxTimeExpiration"

type ActivePanel = "token" | "user"

export function UserManagementDialog() {
  const [open, setOpen] = useState(false)
  const [activePanel, setActivePanel] = useState<ActivePanel>("token")

  const [tables, setTables] = useState("")
  const [tokenName, setTokenName] = useState("")
  const [expiresAt, setExpiresAt] = useState<Date | undefined>(new Date())
  const [generatedToken, setGeneratedToken] = useState("")

  const [name, setName] = useState("")
  const [email, setEmail] = useState("")
  const [password, setPassword] = useState("")
  const [confirmPassword, setConfirmPassword] = useState("")

  const handleGenerateToken = async () => {
    if (!tables || !expiresAt) {
      toast.error("Fill in the token name, tables and expiration.")
      return
    }

    try {
      const response = await api.post("/generateToken", {
        name: tokenName,
        tables: tables
          .split(",")
          .map((table) => table.trim())
          .filter(Boolean),
        expires: expiresAt.toISOString(),
      })

      if (response.status === 200) {
        setGeneratedToken(response.data)
        toast.success("Token successfully generated!")
      } else {
        toast.error("Error generating token. Please try again.")
      }
    } catch (error) {
      console.error("Erro ao gerar token:", error)
      toast.error("Error generating token. Please try again.")
    }
  }

  const handleCopyToken = async () => {
    await navigator.clipboard.writeText(generatedToken)
    toast.success("Token copied successfully!")
  }

  const handleCreateUser = async () => {
    if (!name || !email || !password || !confirmPassword) {
      toast.error("Please fill in all fields.")
      return
    }

    if (password !== confirmPassword) {
      toast.error("Passwords do not match.")
      return
    }

    try {
      const response = await api.post("/CreateUser", {
        name,
        email,
        password,
      })

      if (response.status === 200) {
        toast.success("User created successfully!")
        setName("")
        setEmail("")
        setPassword("")
        setConfirmPassword("")
      } else {
        toast.error("Error creating user. Please try again.")
      }
    } catch (error) {
      console.error("Error creating user:", error)
      toast.error("Error creating user. Please try again.")
    }
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <button type="button" className="text-sm">
          Manage Access
        </button>
      </DialogTrigger>
      <DialogContent className="max-w-3xl">
        <DialogHeader>
          <DialogTitle>User Management</DialogTitle>
          <DialogDescription>
            Generate integration tokens or create another user without leaving the dashboard.
          </DialogDescription>
        </DialogHeader>

        <div className="flex flex-wrap gap-2">
          <Button
            type="button"
            variant={activePanel === "token" ? "default" : "outline"}
            onClick={() => setActivePanel("token")}
          >
            Generate Token
          </Button>
          <Button
            type="button"
            variant={activePanel === "user" ? "default" : "outline"}
            onClick={() => setActivePanel("user")}
          >
            Create User
          </Button>
        </div>

        {activePanel === "token" ? (
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Generate Token</CardTitle>
              <CardDescription>
                Create a token for a system or integration and copy it immediately.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-2">
                <Label htmlFor="token-name">Token name</Label>
                <Input
                  id="token-name"
                  value={tokenName}
                  onChange={(event) => setTokenName(event.target.value)}
                  placeholder="Set system name"
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="token-tables">Tables</Label>
                <Input
                  id="token-tables"
                  value={tables}
                  onChange={(event) => setTables(event.target.value)}
                  placeholder="Enter table names separated by comma"
                />
              </div>
              <div className="grid gap-2">
                <Label>Expires in</Label>
                <ComboBoxTimeExpiration
                  setDate={(date) => {
                    setExpiresAt(date)
                  }}
                />
              </div>

              {generatedToken ? (
                <div className="grid gap-2 rounded-lg border bg-muted/30 p-3">
                  <Label htmlFor="generated-token">Generated token</Label>
                  <div className="flex gap-2">
                    <Input id="generated-token" value={generatedToken} readOnly />
                    <Button type="button" onClick={handleCopyToken}>
                      Copy
                    </Button>
                  </div>
                </div>
              ) : null}

              <div className="flex justify-end">
                <Button type="button" onClick={handleGenerateToken}>
                  Generate Token
                </Button>
              </div>
            </CardContent>
          </Card>
        ) : null}

        {activePanel === "user" ? (
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Create User</CardTitle>
              <CardDescription>
                Register another user with email and password access.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-2">
                <Label htmlFor="new-user-name">Name</Label>
                <Input
                  id="new-user-name"
                  value={name}
                  onChange={(event) => setName(event.target.value)}
                  placeholder="Enter name"
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="new-user-email">Email</Label>
                <Input
                  id="new-user-email"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  placeholder="Enter email"
                />
              </div>
              <div className="grid gap-2 sm:grid-cols-2">
                <div className="grid gap-2">
                  <Label htmlFor="new-user-password">Password</Label>
                  <Input
                    id="new-user-password"
                    type="password"
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
                    placeholder="Enter password"
                  />
                </div>
                <div className="grid gap-2">
                  <Label htmlFor="new-user-confirm-password">Confirm password</Label>
                  <Input
                    id="new-user-confirm-password"
                    type="password"
                    value={confirmPassword}
                    onChange={(event) => setConfirmPassword(event.target.value)}
                    placeholder="Confirm password"
                  />
                </div>
              </div>

              <div className="flex justify-end">
                <Button type="button" onClick={handleCreateUser}>
                  Create User
                </Button>
              </div>
            </CardContent>
          </Card>
        ) : null}

        <DialogFooter>
          <Button type="button" variant="outline" onClick={() => setOpen(false)}>
            Close
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
