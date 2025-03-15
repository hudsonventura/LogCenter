//import "./App.css";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { TableLogs } from "./components/pages/TableLogs";
import { Toaster } from "@/components/ui/sonner";
import { Tables } from "./components/pages/Tables";
import { TableConfigs } from "./components/pages/TableConfigs";
import Logoff from "./components/pages/Logoff";
import LoginPage from "./components/pages/LoginPage";



export default function App() {
  return (
    <Router>
      <div className="App">
        <Toaster />
        <Routes>
          <Route path="/" element={<Navigate to="/login" />} /> 
          <Route path="/login" element={<LoginPage />} /> {/* Página de logs */}
          <Route path="/tables" element={<Tables />} /> {/* Página inicial */}
          <Route path="/table-logs" element={<TableLogs />} /> {/* Página de logs */}
          <Route path="/table-configs/:tableId" element={<TableConfigs />} /> {/* Página de logs */}
          <Route path="/logoff" element={<Logoff />} /> {/* Página de logs */}
        </Routes>
      </div>
    </Router>
  );
}
