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
				await api.post("/logoff", {});

			} catch (error: any) {
				if(error.status === 401){
					navigate("/login");
					sessionStorage.removeItem("token");
				}
			}
		};

	}, []);



	return <></>;
}

