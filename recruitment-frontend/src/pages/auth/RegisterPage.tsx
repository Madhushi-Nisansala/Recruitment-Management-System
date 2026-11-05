import { zodResolver } from '@hookform/resolvers/zod';
import { Eye, EyeOff } from 'lucide-react';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { register as registerUser } from '../../api/authApi';
import { getApiErrorMessage } from '../../api/errorHandler';

const schema = z.object({
  fullName: z.string().trim().min(2, 'Enter your full name.').max(100),
  email: z.string().trim().email('Enter a valid email address.'),
  password: z.string().min(12, 'Use at least 12 characters.').max(128),
  confirmPassword: z.string(),
}).refine((value) => value.password === value.confirmPassword, { path: ['confirmPassword'], message: 'Passwords do not match.' });
type FormValues = z.infer<typeof schema>;

export function RegisterPage() {
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);
  const [serverError, setServerError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const { register, handleSubmit, formState: { errors } } = useForm<FormValues>({ resolver: zodResolver(schema) });
  const onSubmit = async (values: FormValues) => {
    setServerError(''); setSubmitting(true);
    try { await registerUser({ ...values, requestedRole: 'Candidate' }); navigate('/login', { replace: true, state: { message: 'Registration successful. You can now sign in.' } }); }
    catch (error) { setServerError(getApiErrorMessage(error, 'Registration could not be completed.')); }
    finally { setSubmitting(false); }
  };
  return <div>
    <div className="form-heading"><p className="eyebrow">GET STARTED</p><h2>Create your account</h2><p>Join the recruitment platform as a Candidate.</p></div>
    {serverError && <div className="alert alert-error">{serverError}</div>}
    <form className="form-stack" onSubmit={handleSubmit(onSubmit)}>
      <label>Full name<input placeholder="Alex Morgan" autoComplete="name" {...register('fullName')} /></label>{errors.fullName && <span className="field-error">{errors.fullName.message}</span>}
      <label>Email<input type="email" placeholder="alex@example.com" autoComplete="email" {...register('email')} /></label>{errors.email && <span className="field-error">{errors.email.message}</span>}
      <label>Password<div className="input-with-icon"><input type={showPassword ? 'text' : 'password'} autoComplete="new-password" {...register('password')} /><button type="button" className="icon-button" onClick={() => setShowPassword(!showPassword)} aria-label="Toggle password visibility">{showPassword ? <EyeOff size={17} /> : <Eye size={17} />}</button></div></label>{errors.password && <span className="field-error">{errors.password.message}</span>}
      <label>Confirm password<input type={showPassword ? 'text' : 'password'} autoComplete="new-password" {...register('confirmPassword')} /></label>{errors.confirmPassword && <span className="field-error">{errors.confirmPassword.message}</span>}
      <button className="button primary full" disabled={submitting}>{submitting ? 'Creating account...' : 'Create account'}</button>
    </form>
    <p className="form-footer">Already registered? <Link to="/login">Sign in</Link></p>
  </div>;
}
