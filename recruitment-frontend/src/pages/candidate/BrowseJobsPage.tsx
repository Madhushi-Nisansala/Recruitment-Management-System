import { Search, MapPin, BriefcaseBusiness, ChevronLeft, ChevronRight } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { getJobs } from '../../api/jobsApi';
import { getApiErrorMessage } from '../../api/errorHandler';
import type { Job, JobPage } from '../../types/job';
import { LoadingSpinner } from '../../components/LoadingSpinner';
import { ErrorMessage } from '../../components/ErrorMessage';

export function BrowseJobsPage() {
  const [data, setData] = useState<JobPage | null>(null); const [page, setPage] = useState(1); const [search, setSearch] = useState(''); const [loading, setLoading] = useState(true); const [error, setError] = useState('');
  useEffect(() => { setLoading(true); getJobs(page, 50).then(setData).catch((e) => setError(getApiErrorMessage(e))).finally(() => setLoading(false)); }, [page]);
  const jobs = useMemo(() => { const term = search.toLowerCase(); return (data?.items ?? []).filter((job) => !term || `${job.title} ${job.location ?? ''}`.toLowerCase().includes(term)); }, [data, search]);
  return <div><div className="page-heading"><div><p className="eyebrow">OPPORTUNITIES</p><h1>Browse jobs</h1><p>Discover roles that match your goals.</p></div></div><div className="search-bar"><Search size={18} /><input value={search} onChange={(e) => { setSearch(e.target.value); setPage(1); }} placeholder="Search by title or location" /></div>{loading ? <LoadingSpinner /> : error ? <ErrorMessage message={error} /> : jobs.length === 0 ? <div className="empty panel"><BriefcaseBusiness size={30} /><h3>No matching jobs</h3><p>Try a different search or check back later.</p></div> : <div className="job-grid">{jobs.map((job) => <JobCard key={job.id} job={job} />)}</div>}<div className="pagination"><button className="button secondary" disabled={page <= 1} onClick={() => setPage(page - 1)}><ChevronLeft size={16} /> Previous</button><span>Page {page}{data?.totalPages ? ` of ${data.totalPages}` : ''}</span><button className="button secondary" disabled={!data || page >= data.totalPages} onClick={() => setPage(page + 1)}>Next <ChevronRight size={16} /></button></div></div>;
}
function JobCard({ job }: { job: Job }) { return <article className="job-card"><div className="job-card-icon"><BriefcaseBusiness size={22} /></div><div className="job-card-body"><div className="job-card-top"><h3>{job.title}</h3><span className="status status-open">Open</span></div><p className="muted">{job.recruiter.fullName}</p><div className="job-meta"><span><MapPin size={15} />{job.location || 'Remote / unspecified'}</span><span>{job.employmentType || 'Employment type not specified'}</span></div><p className="description-preview">{job.description.length > 150 ? `${job.description.slice(0, 150)}...` : job.description}</p><div className="job-card-footer"><span>{job.salaryMin || job.salaryMax ? `$${job.salaryMin ?? 0} - $${job.salaryMax ?? 'open'}` : 'Salary not listed'}</span><Link className="text-link" to={`/candidate/jobs/${job.id}`}>View details <ChevronRight size={15} /></Link></div></div></article>; }
