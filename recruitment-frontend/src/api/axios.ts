import axios from 'axios';

export const ACCESS_TOKEN_KEY = 'rms.accessToken';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'https://localhost:7043',
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const requestUrl = config.url?.toLowerCase() ?? '';
  const isAuthenticationRequest = requestUrl.includes('/api/auth/login')
    || requestUrl.includes('/api/auth/register');

  if (!isAuthenticationRequest) {
    const token = sessionStorage.getItem(ACCESS_TOKEN_KEY);
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
  }

  if (import.meta.env.DEV) {
    console.debug('[API request]', {
      method: config.method?.toUpperCase(),
      url: `${config.baseURL ?? ''}${config.url ?? ''}`,
      authenticated: !isAuthenticationRequest && Boolean(sessionStorage.getItem(ACCESS_TOKEN_KEY)),
    });
  }

  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (import.meta.env.DEV) {
      console.warn('[API response error]', {
        method: error.config?.method?.toUpperCase(),
        url: `${error.config?.baseURL ?? ''}${error.config?.url ?? ''}`,
        status: error.response?.status ?? 'NETWORK_ERROR',
      });
    }

    if (error.response?.status === 401) {
      const requestUrl = error.config?.url?.toLowerCase() ?? '';
      const isAuthenticationRequest = requestUrl.includes('/api/auth/login')
        || requestUrl.includes('/api/auth/register');

      if (!isAuthenticationRequest) {
        sessionStorage.removeItem(ACCESS_TOKEN_KEY);
        window.dispatchEvent(new Event('rms:auth-expired'));
      }
    }
    return Promise.reject(error);
  },
);
