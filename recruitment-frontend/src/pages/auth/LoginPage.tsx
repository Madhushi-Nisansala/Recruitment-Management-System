import { zodResolver } from '@hookform/resolvers/zod';
import { Eye, EyeOff, LockKeyhole, Mail } from 'lucide-react';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { getApiErrorMessage } from '../../api/errorHandler';
import { useAuth } from '../../context/AuthContext';

const schema = z.object({ email: z.string().email('Enter a valid email address.'), password: z.string().min(1, 'Password is required.') });
type FormValues = z.infer<typeof schema>;

function dashboardFor(role: string) { return role === 'Candidate' ? '/candidate/dashboard' : role === 'Recruiter' ? '/recruiter/dashboard' : '/admin/dashboard'; }

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [showPassword, setShowPassword] = useState(false);
  const [serverError, setServerError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const message = (location.state as { message?: string } | null)?.message;
  const from = (location.state as { from?: string } | null)?.from;
  const { register, handleSubmit, formState: { errors } } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const onSubmit = async (values: FormValues) => {
    setServerError(''); setSubmitting(true);
    try {
      const response = await login(values);
      navigate(from?.startsWith('/') ? from : dashboardFor(response.user.role), { replace: true });
    } catch (error) { setServerError(getApiErrorMessage(error, 'Invalid email or password.')); }
    finally { setSubmitting(false); }
  };

  return <div>
    <div className="form-heading"><p className="eyebrow">WELCOME BACK</p><h2>Sign in to your account</h2><p>Manage your opportunities and applications in one place.</p></div>
    {message && <div className="alert alert-success">{message}</div>}{serverError && <div className="alert alert-error">{serverError}</div>}
    <form className="form-stack" onSubmit={handleSubmit(onSubmit)}>
      <label>Email<input type="email" autoComplete="email" placeholder="you@example.com" {...register('email')} /></label>{errors.email && <span className="field-error">{errors.email.message}</span>}
      <label>Password<div className="input-with-icon"><LockKeyhole size={17} /><input type={showPassword ? 'text' : 'password'} autoComplete="current-password" placeholder="Your password" {...register('password')} /><button type="button" className="icon-button" onClick={() => setShowPassword(!showPassword)} aria-label="Toggle password visibility">{showPassword ? <EyeOff size={17} /> : <Eye size={17} />}</button></div></label>{errors.password && <span className="field-error">{errors.password.message}</span>}
      <button className="button primary full" disabled={submitting}>{submitting ? 'Signing in...' : 'Sign in'}</button>
    </form>
    <p className="form-footer">New to RecruitFlow? <Link to="/register">Create an account</Link></p>
  </div>;
}
