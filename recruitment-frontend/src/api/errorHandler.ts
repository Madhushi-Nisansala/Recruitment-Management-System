import axios from 'axios';

export function getApiErrorMessage(error: unknown, fallback = 'Something went wrong. Please try again.') {
  if (!axios.isAxiosError(error)) return fallback;
  if (!error.response) return 'The API could not be reached. Check that the backend is running and HTTPS is trusted.';
  const data = error.response.data as { message?: string; title?: string; errors?: Record<string, string[]> } | undefined;
  if (data?.errors) return Object.values(data.errors).flat().join(' ');
  const requestUrl = error.config?.url?.toLowerCase() ?? '';
  if (error.response.status === 401 && requestUrl.includes('/api/auth/login')) {
    return data?.message || 'Invalid email or password.';
  }
  if (error.response.status === 401) return data?.message || 'Your session is missing or has expired. Please sign in again.';
  if (error.response.status === 403) return 'You do not have permission to perform this action.';
  if (error.response.status === 404) return 'The requested resource was not found.';
  if (error.response.status === 409) return data?.message || 'This operation conflicts with existing data.';
  if (error.response.status >= 500) return 'The server could not complete the request.';
  return data?.message || data?.title || fallback;
}
