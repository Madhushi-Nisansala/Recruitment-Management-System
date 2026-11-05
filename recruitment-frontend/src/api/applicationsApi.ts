import { api } from './axios';
import type { Application, CreateApplicationRequest, ApplicationStatus } from '../types/application';

export async function getApplications() {
  const response = await api.get<Application[]>('/api/applications');
  return response.data;
}

export async function getApplication(id: number) {
  const response = await api.get<Application>(`/api/applications/${id}`);
  return response.data;
}

export async function createApplication(request: CreateApplicationRequest) {
  const response = await api.post<Application>('/api/applications', request);
  return response.data;
}

export async function updateApplicationStatus(id: number, status: ApplicationStatus) {
  const response = await api.put<Application>(`/api/applications/${id}/status`, { status });
  return response.data;
}
