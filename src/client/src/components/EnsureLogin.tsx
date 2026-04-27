import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import api from "@/services/api";
import { clearStoredToken, getStoredToken } from "@/lib/auth-storage";

export default function EnsureLogin() {
	const navigate = useNavigate();

	useEffect(() => {
		const checkLogin = async () => {
			if (!getStoredToken()) {
				navigate("/login");
				return;
			}

			try {
				await api.get("/CheckToken");
			} catch {
				navigate("/login");
				clearStoredToken();
			}
		};

		void checkLogin();
	}, [navigate]);

	return <></>;
}
