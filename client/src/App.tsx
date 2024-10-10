import "./App.css";
import { TableLogs } from "./components/TableLogs";
import { Toaster } from "@/components/ui/sonner"


export default function App() {
  return (
    <>
      <div className="App">
        <Toaster />
        <TableLogs />
      </div>
    </>
  );
}
