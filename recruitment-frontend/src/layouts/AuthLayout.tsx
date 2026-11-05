import { BriefcaseBusiness } from 'lucide-react';
import { Outlet } from 'react-router-dom';

export function AuthLayout() {
  return <main className="auth-page"><section className="auth-brand"><div className="brand-mark large">R</div><h1>RecruitFlow</h1><p>Connect great people with meaningful opportunities.</p></section><section className="auth-card"><Outlet /></section></main>;
}
