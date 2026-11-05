export function LoadingSpinner({ label = 'Loading...' }: { label?: string }) {
  return <div className="loading" role="status"><span className="spinner" />{label}</div>;
}
