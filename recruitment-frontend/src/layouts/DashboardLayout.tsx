import { BriefcaseBusiness, ClipboardList, LayoutDashboard, LogOut, Menu, ShieldCheck, UserRound, X } from 'lucide-react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { useAuth } from '../context/AuthContext';

export function DashboardLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  if (!user) return null;

  const links = user.role === 'Candidate'
    ? [['/candidate/dashboard', 'Dashboard', LayoutDashboard], ['/candidate/jobs', 'Browse jobs', BriefcaseBusiness], ['/candidate/applications', 'My applications', ClipboardList]]
    : user.role === 'Recruiter'
      ? [['/recruiter/dashboard', 'Dashboard', LayoutDashboard], ['/recruiter/jobs', 'My jobs', BriefcaseBusiness]]
      : [['/admin/dashboard', 'Overview', ShieldCheck], ['/admin/jobs', 'All jobs', BriefcaseBusiness], ['/admin/applications', 'All applications', ClipboardList]];

  return (
    <div className="app-shell">
      <aside className={`sidebar ${open ? 'sidebar-open' : ''}`}>
        <div className="brand"><div className="brand-mark">R</div><span>RecruitFlow</span></div>
        <nav className="side-nav">
          {links.map(([to, label, Icon]) => (
            <NavLink key={to as string} to={to as string} onClick={() => setOpen(false)} className={({ isActive }) => isActive ? 'nav-item active' : 'nav-item'}>
              <Icon size={18} /> {label as string}
            </NavLink>
          ))}
        </nav>
        <button className="nav-item logout-button" onClick={() => { logout(); navigate('/login'); }}><LogOut size={18} /> Sign out</button>
      </aside>
      {open && <button className="sidebar-backdrop" aria-label="Close navigation" onClick={() => setOpen(false)} />}
      <section className="main-shell">
        <header className="topbar">
          <button className="mobile-menu" aria-label="Open navigation" onClick={() => setOpen(true)}>{open ? <X /> : <Menu />}</button>
          <div className="topbar-spacer" />
          <div className="user-chip"><div className="avatar"><UserRound size={17} /></div><div><strong>{user.fullName}</strong><small>{user.role}</small></div></div>
        </header>
        <main className="page-content"><Outlet /></main>
      </section>
    </div>
  );
}
