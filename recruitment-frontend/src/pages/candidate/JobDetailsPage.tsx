import { ArrowLeft, BriefcaseBusiness, CalendarDays, MapPin, Send } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { getJob } from '../../api/jobsApi';
import { createApplication } from '../../api/applicationsApi';
import { getApiErrorMessage } from '../../api/errorHandler';
import type { Job } from '../../types/job';
import { useAuth } from '../../context/AuthContext';
import { LoadingSpinner } from '../../components/LoadingSpinner';
import { ErrorMessage } from '../../components/ErrorMessage';

export function JobDetailsPage() {
  const { id } = useParams(); const { user } = useAuth(); const navigate = useNavigate(); const [job, setJob] = useState<Job | null>(null); const [loading, setLoading] = useState(true); const [error, setError] = useState(''); const [submitError, setSubmitError] = useState(''); const [submitted, setSubmitted] = useState(false); const [submitting, setSubmitting] = useState(false); const { register, handleSubmit } = useForm<{ coverLetter: string }>();
  useEffect(() => { if (!id) return; getJob(Number(id)).then(setJob).catch((e) => setError(getApiErrorMessage(e))).finally(() => setLoading(false)); }, [id]);
  if (loading) return <LoadingSpinner />; if (error || !job) return <ErrorMessage message={error || 'Job not found.'} />;
  const closed = job.closingDate ? new Date(job.closingDate) <= new Date() : false;
  const submit = async ({ coverLetter }: { coverLetter: string }) => { setSubmitting(true); setSubmitError(''); try { await createApplication({ jobId: job.id, coverLetter: coverLetter || undefined }); setSubmitted(true); } catch (e) { setSubmitError(getApiErrorMessage(e)); } finally { setSubmitting(false); } };
  return <div><Link className="back-link" to="/candidate/jobs"><ArrowLeft size={16} /> Back to jobs</Link><section className="detail-hero"><div className="job-card-icon large-icon"><BriefcaseBusiness size={30} /></div><div><p className="eyebrow">JOB DETAILS</p><h1>{job.title}</h1><p className="muted">{job.recruiter.fullName} · {job.recruiter.email}</p></div></section><div className="detail-grid"><article className="panel detail-content"><div className="job-meta large-meta"><span><MapPin size={16} />{job.location || 'Location unspecified'}</span><span>{job.employmentType || 'Employment type unspecified'}</span><span><CalendarDays size={16} />{job.closingDate ? `Closes ${new Date(job.closingDate).toLocaleDateString()}` : 'Open until filled'}</span></div><h2>About the role</h2><p className="preserve-lines">{job.description}</p></article><aside className="panel apply-panel"><h2>Apply for this role</h2>{!user ? <Link className="button primary full" to="/login">Sign in to apply</Link> : closed ? <div className="alert alert-error">Applications for this job are closed.</div> : submitted ? <div className="alert alert-success">Application submitted successfully.</div> : <form className="form-stack" onSubmit={handleSubmit(submit)}>{submitError && <div className="alert alert-error">{submitError}</div>}<label>Cover letter <textarea rows={7} placeholder="Tell the recruiter why you are a strong fit..." {...register('coverLetter')} /></label><button className="button primary full" disabled={submitting}><Send size={16} />{submitting ? 'Submitting...' : 'Submit application'}</button></form>}</aside></div></div>;
}
