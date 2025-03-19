import { useEffect, useState } from "react";
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
				console.log("Aqui 1")
				const response = await api.post("/logoff", {});

			} catch (error) {
				if(error.status === 401){
					console.log("Aqui 2")
					navigate("/login");
					sessionStorage.removeItem("token");
				}
			}
		};

	}, []);



	return <></>;
}

