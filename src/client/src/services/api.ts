// src/services/api.ts
import axios, { AxiosInstance } from 'axios';

// Instância do Axios com uma configuração padrão
const api: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_HOST,
  headers: {
    'Content-Type': 'application/json',
    'Authorization':  `Bearer ${window.sessionStorage.getItem('token') || ''}`,
  },
  timeout: 5000, // Tempo limite para requisi es (em milissegundos),
});



export default api;
