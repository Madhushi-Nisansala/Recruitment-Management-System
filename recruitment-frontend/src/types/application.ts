export type ApplicationStatus = 'Pending' | 'Reviewed' | 'Accepted' | 'Rejected' | 'Withdrawn';

export interface Application {
  id: number;
  candidateId: number;
  candidateName: string;
  jobId: number;
  jobTitle: string;
  recruiterId: number;
  recruiterName: string;
  appliedAt: string;
  status: ApplicationStatus;
  coverLetter: string | null;
  cvFilePath: string | null;
}

export interface CreateApplicationRequest {
  jobId: number;
  coverLetter?: string;
}
