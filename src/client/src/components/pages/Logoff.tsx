import React from "react";
import { useNavigate } from "react-router-dom";
import { clearStoredToken } from "@/lib/auth-storage";

export default function Logoff() {
  const navigate = useNavigate();

  React.useEffect(() => {
    clearStoredToken();
    navigate("/login");
  }, [navigate]);

  return null;
}
