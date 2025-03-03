import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Popover, PopoverTrigger, PopoverContent } from "@/components/ui/popover";
import { Avatar, AvatarImage, AvatarFallback } from "@/components/ui/avatar";
import { Separator } from "@/components/ui/separator";

export default function HeaderBar() {
  const navigate = useNavigate();

  const handleLogoff = () => {
    navigate("/logoff"); // Redireciona sem recarregar a página
  };

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
              <h4 className="text-sm font-medium leading-none">My name</h4>
              <p className="text-sm text-muted-foreground">
                An open-source UI component library.
              </p>
            </div>
            <Separator className="my-4" />
            <div className="flex h-5 items-center space-x-4 text-sm">
              <div>Profile</div>
              <Separator orientation="vertical" />
              <div>Docs</div>
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
