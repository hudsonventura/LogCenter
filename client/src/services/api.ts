// src/services/api.ts
import axios, { AxiosInstance } from 'axios';

// Instância do Axios com uma configuração padrão
const api: AxiosInstance = axios.create({
  baseURL: 'https://logcenter.hudsonventura.ddnsgeek.com',
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 5000, // Tempo limite para requisições (em milissegundos)
});

export default api;
