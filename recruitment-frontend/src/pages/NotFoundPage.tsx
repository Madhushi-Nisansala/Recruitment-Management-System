import { Link } from 'react-router-dom';

export function NotFoundPage() { return <div className="center-page"><h1>Page not found</h1><p>The page you requested does not exist.</p><Link className="button primary" to="/">Go home</Link></div>; }
export function UnauthorizedPage() { return <div className="center-page"><h1>Access denied</h1><p>Your account does not have permission to view this page.</p><Link className="button primary" to="/">Return to dashboard</Link></div>; }
