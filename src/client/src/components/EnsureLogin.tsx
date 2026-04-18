import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import api from "@/services/api";

export default function EnsureLogin() {
	const navigate = useNavigate();

	useEffect(() => {
		const checkLogin = async () => {
			if (!sessionStorage.getItem("token")) {
				navigate("/login");
				return;
			}

			try {
				await api.get("/CheckToken");
			} catch {
				navigate("/login");
				sessionStorage.removeItem("token");
			}
		};

		void checkLogin();
	}, [navigate]);

	return <></>;
}
