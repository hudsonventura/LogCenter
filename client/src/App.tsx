//import "./App.css";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { TableLogs } from "./components/pages/TableLogs";
import { Toaster } from "@/components/ui/sonner";
import { ListTables } from "./components/pages/ListTables";
import { TableConfigs } from "./components/pages/TableConfigs";
import LogCenterChart from "./components/pages/LogCenterChart";

export default function App() {
  return (
    <Router>
      <div className="App">
        <Toaster />
        <Routes>
          <Route path="/" element={<ListTables />} /> {/* Página inicial */}
          <Route path="/table-logs" element={<TableLogs />} /> {/* Página de logs */}
          <Route path="/table-configs/:tableId" element={<TableConfigs />} /> {/* Página de logs */}
          <Route path="/charts" element={<LogCenterChart />} />
        </Routes>
      </div>
    </Router>
  );
}
