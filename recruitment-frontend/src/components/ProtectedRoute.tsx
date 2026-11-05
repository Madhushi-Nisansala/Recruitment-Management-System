import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LoadingSpinner } from './LoadingSpinner';

export function ProtectedRoute() {
  const { user, isLoading } = useAuth();
  const location = useLocation();
  if (isLoading) return <LoadingSpinner label="Restoring your session..." />;
  if (!user) return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  return <Outlet />;
}
