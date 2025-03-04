import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Popover, PopoverTrigger, PopoverContent } from "@/components/ui/popover";
import { Avatar, AvatarImage, AvatarFallback } from "@/components/ui/avatar";
import { Separator } from "@/components/ui/separator";
import { useEffect, useState } from "react";
import { ModalTokenGeneration } from "./ModalTokenGeneration";
import { ModalUserCreation } from "./ModalUserCreation";
import { ModalChangePassword } from "./ModalChangePassword";

export default function HeaderBar() {
  const navigate = useNavigate();

  const handleLogoff = () => {
    navigate("/logoff"); // Redireciona sem recarregar a página
  };

  const [userData, setUserData] = useState(null);
  const decodeJWT = (token) => {
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
    const token = sessionStorage.getItem("token"); // Obtendo do sessionStorage
    if (token) {
      const decodedData = decodeJWT(token);
      decodedData.email = decodedData["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];
      setUserData(decodedData);
      //console.log(decodedData["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]);
      
      //console.log("Dados do JWT:", decodedData);
    }
  }, []);

  return (
    <header className="bg-white shadow-md p-4 flex items-center justify-between">
      {/* Menu Button */}
      <Button variant="ghost" size="icon" className="ml-12">
        <img src="/logo.png" alt="Logo" />
        <h1 className="text-xl">LogCenter</h1>
      </Button>

      {/* Ações (Login, Perfil, etc.) */}
      <Popover>
        <PopoverTrigger asChild>
          <Avatar className="cursor-pointer">
            <AvatarImage src="https://github.com/shadcn.png" alt="@shadcn" />
            <AvatarFallback>CN</AvatarFallback>
          </Avatar>
        </PopoverTrigger>
        <PopoverContent className="min-w-[200px]">
          <div>
            <div className="space-y-1">
              <h2>{userData !== null ? userData.name : ""} <p className="text-sm text-muted-foreground">{userData !== null ? userData.email : ""}</p> </h2>
              <h4 className="text-sm font-medium leading-none">

              </h4>
              {/* <p className="text-sm text-muted-foreground">
                An open-source UI component library.
              </p> */}
            </div>
            <Separator className="my-4" />
            <div className="flex h-5 items-center space-x-4 text-sm">
              <ModalTokenGeneration />
              <Separator orientation="vertical" />
              <ModalUserCreation />
              <Separator orientation="vertical" />
              <ModalChangePassword />
            </div>
            <Separator className="my-4" />
            <div className="flex h-5 items-center space-x-4 text-sm">
              <Separator orientation="vertical" />
              <a href="https://github.com/hudsonventura/LogCenter" target="_blank" rel="noopener noreferrer">Docs</a>
              <Separator orientation="vertical" />
              <button onClick={handleLogoff} className="text-sm text-red-500">
                Logoff
              </button>
            </div>
          </div>
        </PopoverContent>
      </Popover>
    </header>
  );
}
