import { Navigate, Outlet } from 'react-router-dom';
import type { UserRole } from '../types/auth';
import { useAuth } from '../context/AuthContext';

export function RoleBasedRoute({ roles }: { roles: UserRole[] }) {
  const { user } = useAuth();
  return user && roles.includes(user.role) ? <Outlet /> : <Navigate to="/unauthorized" replace />;
}
