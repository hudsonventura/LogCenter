import React from "react";
import { useNavigate } from "react-router-dom";

export default function Logoff() {
  const navigate = useNavigate();

  React.useEffect(() => {
    sessionStorage.removeItem("token");
    navigate("/login");
  }, []);

  return null;
}
