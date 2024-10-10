import "./App.css";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { TableLogs } from "./components/pages/TableLogs";
import { Toaster } from "@/components/ui/sonner";
import { ListTables } from "./components/pages/ListTables";

export default function App() {
  return (
    <Router>
      <div className="App">
        <Toaster />
        <Routes>
          <Route path="/" element={<ListTables />} /> {/* Página inicial */}
          <Route path="/table-logs" element={<TableLogs />} /> {/* Página de logs */}
        </Routes>
      </div>
    </Router>
  );
}
