import { useEffect } from "react";
import { useNavigate } from "react-router-dom";

export default function EnsureLogin() {
  const navigate = useNavigate();

  useEffect(() => {
        if (!sessionStorage.getItem("token")) {
            navigate("/login");
        }
  }, []);

  


}
