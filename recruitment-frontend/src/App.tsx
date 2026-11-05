import { Navigate, Route, Routes } from 'react-router-dom';
import { AuthLayout } from './layouts/AuthLayout';
import { DashboardLayout } from './layouts/DashboardLayout';
import { ProtectedRoute } from './components/ProtectedRoute';
import { RoleBasedRoute } from './components/RoleBasedRoute';
import { useAuth } from './context/AuthContext';
import { LoginPage } from './pages/auth/LoginPage';
import { RegisterPage } from './pages/auth/RegisterPage';
import { CandidateDashboard } from './pages/candidate/CandidateDashboard';
import { BrowseJobsPage } from './pages/candidate/BrowseJobsPage';
import { JobDetailsPage } from './pages/candidate/JobDetailsPage';
import { MyApplicationsPage } from './pages/candidate/MyApplicationsPage';
import { RecruiterDashboard } from './pages/recruiter/RecruiterDashboard';
import { MyJobsPage } from './pages/recruiter/MyJobsPage';
import { JobFormPage } from './pages/recruiter/JobFormPage';
import { JobApplicationsPage } from './pages/recruiter/JobApplicationsPage';
import { AdminDashboard } from './pages/admin/AdminDashboard';
import { AdminJobsPage } from './pages/admin/AdminJobsPage';
import { AdminApplicationsPage } from './pages/admin/AdminApplicationsPage';
import { NotFoundPage, UnauthorizedPage } from './pages/NotFoundPage';

function HomeRedirect() {
  const { user, isLoading } = useAuth();
  if (isLoading) return null;
  if (!user) return <Navigate to="/login" replace />;
  return <Navigate to={user.role === 'Candidate' ? '/candidate/dashboard' : user.role === 'Recruiter' ? '/recruiter/dashboard' : '/admin/dashboard'} replace />;
}

export default function App() {
  return <Routes>
    <Route element={<AuthLayout />}><Route path="/login" element={<LoginPage />} /><Route path="/register" element={<RegisterPage />} /></Route>
    <Route element={<ProtectedRoute />}><Route element={<DashboardLayout />}>
      <Route element={<RoleBasedRoute roles={['Candidate']} />}>
        <Route path="/candidate/dashboard" element={<CandidateDashboard />} /><Route path="/candidate/jobs" element={<BrowseJobsPage />} /><Route path="/candidate/jobs/:id" element={<JobDetailsPage />} /><Route path="/candidate/applications" element={<MyApplicationsPage />} />
      </Route>
      <Route element={<RoleBasedRoute roles={['Recruiter']} />}>
        <Route path="/recruiter/dashboard" element={<RecruiterDashboard />} /><Route path="/recruiter/jobs" element={<MyJobsPage />} /><Route path="/recruiter/jobs/new" element={<JobFormPage />} /><Route path="/recruiter/jobs/:id/edit" element={<JobFormPage />} /><Route path="/recruiter/jobs/:id/applications" element={<JobApplicationsPage />} />
      </Route>
      <Route element={<RoleBasedRoute roles={['Admin']} />}>
        <Route path="/admin/dashboard" element={<AdminDashboard />} /><Route path="/admin/jobs" element={<AdminJobsPage />} /><Route path="/admin/applications" element={<AdminApplicationsPage />} />
      </Route>
    </Route></Route>
    <Route path="/unauthorized" element={<UnauthorizedPage />} /><Route path="/" element={<HomeRedirect />} /><Route path="*" element={<NotFoundPage />} />
  </Routes>;
}
