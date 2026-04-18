// src/services/api.ts
import axios, { AxiosInstance } from 'axios';
import { timezoneStorageKey } from '@/components/timezone-provider';

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
  const timezone =
    window.localStorage.getItem(timezoneStorageKey) ||
    Intl.DateTimeFormat().resolvedOptions().timeZone ||
    'UTC';

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  } else if (config.headers.Authorization) {
    delete config.headers.Authorization;
  }

  config.headers.Timezone = timezone;

  return config;
});


export default api;
