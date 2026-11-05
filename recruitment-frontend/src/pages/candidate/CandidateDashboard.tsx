import { ArrowRight, BriefcaseBusiness, CheckCircle2, Clock3, FileText } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { getApplications } from '../../api/applicationsApi';
import { getApiErrorMessage } from '../../api/errorHandler';
import type { Application } from '../../types/application';
import { useAuth } from '../../context/AuthContext';
import { LoadingSpinner } from '../../components/LoadingSpinner';
import { ErrorMessage } from '../../components/ErrorMessage';

export function CandidateDashboard() {
  const { user } = useAuth(); const [items, setItems] = useState<Application[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState('');
  useEffect(() => { getApplications().then(setItems).catch((e) => setError(getApiErrorMessage(e))).finally(() => setLoading(false)); }, []);
  if (loading) return <LoadingSpinner />; if (error) return <ErrorMessage message={error} />;
  const pending = items.filter((x) => x.status === 'Pending').length; const reviewed = items.filter((x) => x.status === 'Reviewed').length; const accepted = items.filter((x) => x.status === 'Accepted').length;
  return <div><div className="page-heading"><div><p className="eyebrow">CANDIDATE SPACE</p><h1>Welcome back, {user?.fullName.split(' ')[0]}</h1><p>Keep track of your career opportunities from one place.</p></div><Link className="button primary" to="/candidate/jobs"><BriefcaseBusiness size={17} /> Browse jobs</Link></div>
    <div className="stat-grid"><Stat icon={<FileText />} label="Total applications" value={items.length} /><Stat icon={<Clock3 />} label="Pending review" value={pending} /><Stat icon={<CheckCircle2 />} label="Accepted" value={accepted} /><Stat icon={<Clock3 />} label="Under review" value={reviewed} /></div>
    <section className="panel"><div className="panel-heading"><div><h2>Recent applications</h2><p>Your latest submissions and their current status.</p></div><Link to="/candidate/applications" className="text-link">View all <ArrowRight size={15} /></Link></div>{items.length === 0 ? <Empty text="You have not applied to any jobs yet." /> : <div className="table-wrap"><table><thead><tr><th>Job</th><th>Recruiter</th><th>Applied</th><th>Status</th></tr></thead><tbody>{items.slice(0, 5).map((item) => <tr key={item.id}><td><strong>{item.jobTitle}</strong></td><td>{item.recruiterName}</td><td>{new Date(item.appliedAt).toLocaleDateString()}</td><td><span className={`status status-${item.status.toLowerCase()}`}>{item.status}</span></td></tr>)}</tbody></table></div>}</section>
  </div>;
}
function Stat({ icon, label, value }: { icon: React.ReactNode; label: string; value: number }) { return <div className="stat-card"><div className="stat-icon">{icon}</div><div><span>{label}</span><strong>{value}</strong></div></div>; }
function Empty({ text }: { text: string }) { return <div className="empty"><FileText size={28} /><p>{text}</p></div>; }
