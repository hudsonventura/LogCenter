import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Popover, PopoverTrigger, PopoverContent } from "@/components/ui/popover";
import { Avatar, AvatarImage, AvatarFallback } from "@/components/ui/avatar";
import { Separator } from "@/components/ui/separator";
import { useEffect, useState } from "react";
import { ThemeToggle } from "./theme-toggle";
import { TimeZoneSelect } from "./TimeZoneSelect";
import { UserManagementDialog } from "./UserManagementDialog";
import { getStoredToken } from "@/lib/auth-storage";

type DecodedToken = {
  name?: string;
  email?: string;
  [key: string]: unknown;
};

export default function HeaderBar() {
  const navigate = useNavigate();

  const handleLogoff = () => {
    navigate("/logoff"); // Redireciona sem recarregar a página
  };

  const [userData, setUserData] = useState<DecodedToken | null>(null);
  const decodeJWT = (token: string): DecodedToken | null => {
    try {
      const base64Url = token.split(".")[1]; // Pegando a parte do payload
      const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
      return JSON.parse(atob(base64));
    } catch (error) {
      console.error("Erro ao decodificar o JWT:", error);
      return null;
    }
  };
  useEffect(() => {
    const token = getStoredToken();
    if (token) {
      const decodedData = decodeJWT(token);

      if (decodedData) {
        decodedData.email = String(
          decodedData["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ?? ""
        );
        setUserData(decodedData);
      }
    }
  }, []);

  return (
    <header className="sticky top-0 z-20 border-b bg-background/95 px-4 py-3 shadow-sm backdrop-blur">
      <div className="mx-auto flex w-full max-w-7xl items-center justify-between gap-4">
        <Button variant="ghost" className="h-auto gap-3 px-2">
          <img src="/logo.png" alt="Logo" className="h-10 w-10" />
          <div className="text-left">
            <p className="text-lg font-semibold leading-none">LogCenter</p>
            <p className="text-xs text-muted-foreground">Log explorer</p>
          </div>
        </Button>

        <div className="flex items-center gap-3">
          <TimeZoneSelect />
          <ThemeToggle />
          <Popover>
            <PopoverTrigger asChild>
              <Avatar className="cursor-pointer">
                <AvatarImage src="https://github.com/shadcn.png" alt="@shadcn" />
                <AvatarFallback>LC</AvatarFallback>
              </Avatar>
            </PopoverTrigger>
            <PopoverContent className="min-w-[260px]" align="end">
              <div>
                <div className="space-y-1">
                  <h2 className="font-medium">
                    {userData !== null ? userData.name : ""}
                  </h2>
                  <p className="text-sm text-muted-foreground">
                    {userData !== null ? userData.email : ""}
                  </p>
                </div>
                <Separator className="my-4" />
                <div className="flex items-center gap-3 text-sm">
                  <UserManagementDialog />
                </div>
                <Separator className="my-4" />
                <div className="flex items-center gap-3 text-sm">
                  <a href={`${import.meta.env.VITE_API_HOST}/docs/swagger/index.html`} target="_blank" rel="noopener noreferrer">Docs</a>
                  <Separator orientation="vertical" />
                  <a href="https://github.com/hudsonventura/LogCenter" target="_blank" rel="noopener noreferrer">Repository</a>
                  <Separator orientation="vertical" />
                  <button onClick={handleLogoff} className="text-sm text-red-500">
                    Logoff
                  </button>
                </div>
              </div>
            </PopoverContent>
          </Popover>
        </div>
      </div>
    </header>
  );
}
