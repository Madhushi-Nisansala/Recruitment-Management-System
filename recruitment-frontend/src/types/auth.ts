export type UserRole = 'Candidate' | 'Recruiter' | 'Admin';

export interface AuthUser {
  id: number;
  fullName: string;
  email: string;
  role: UserRole;
}

export interface AuthResponse {
  accessToken: string;
  tokenType: string;
  expiresAtUtc: string;
  user: AuthUser;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  requestedRole?: 'Candidate' | 'Recruiter';
}

export interface LoginRequest {
  email: string;
  password: string;
}
