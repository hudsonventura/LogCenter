// src/services/api.ts
import axios, { AxiosInstance } from 'axios';

// Instância do Axios com uma configuração padrão
const api: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_HOST,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 5000, // Tempo limite para requisi es (em milissegundos),
});

api.interceptors.request.use((config) => {
  const token = window.sessionStorage.getItem('token');

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  } else if (config.headers.Authorization) {
    delete config.headers.Authorization;
  }

  return config;
});


export default api;
