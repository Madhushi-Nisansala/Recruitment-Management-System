export interface RecruiterSummary {
  id: number;
  fullName: string;
  email: string;
}

export interface Job {
  id: number;
  title: string;
  description: string;
  location: string | null;
  employmentType: string | null;
  salaryMin: number | null;
  salaryMax: number | null;
  postedAt: string;
  closingDate: string | null;
  recruiter: RecruiterSummary;
}

export interface JobPage {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: Job[];
}

export interface JobRequest {
  title: string;
  description: string;
  location?: string;
  employmentType?: string;
  salaryMin?: number;
  salaryMax?: number;
  closingDate?: string;
  recruiterId?: number;
}
