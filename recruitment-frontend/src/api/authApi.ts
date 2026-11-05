import { api } from './axios';
import type { AuthResponse, AuthUser, LoginRequest, RegisterRequest } from '../types/auth';

export async function register(request: RegisterRequest) {
  const response = await api.post<AuthUser>('/api/Auth/register', {
    ...request,
    fullName: request.fullName.trim(),
    email: request.email.trim(),
  });
  return response.data;
}

export async function login(request: LoginRequest) {
  const response = await api.post<AuthResponse>('/api/Auth/login', {
    email: request.email.trim(),
    password: request.password,
  });
  return response.data;
}

export async function getCurrentUser() {
  const response = await api.get<AuthUser>('/api/Auth/me');
  return response.data;
}
