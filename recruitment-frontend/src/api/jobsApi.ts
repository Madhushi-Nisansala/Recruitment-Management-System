import { api } from './axios';
import type { Job, JobPage, JobRequest } from '../types/job';

export async function getJobs(pageNumber = 1, pageSize = 10) {
  const response = await api.get<JobPage>('/api/jobs', { params: { pageNumber, pageSize } });
  return response.data;
}

export async function getJob(id: number) {
  const response = await api.get<Job>(`/api/jobs/${id}`);
  return response.data;
}

export async function createJob(request: JobRequest) {
  const response = await api.post<Job>('/api/jobs', request);
  return response.data;
}

export async function updateJob(id: number, request: JobRequest) {
  const response = await api.put<Job>(`/api/jobs/${id}`, request);
  return response.data;
}

export async function deleteJob(id: number) {
  await api.delete(`/api/jobs/${id}`);
}
