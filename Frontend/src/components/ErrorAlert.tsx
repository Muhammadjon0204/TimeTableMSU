import { AlertCircle } from 'lucide-react';

export function ErrorAlert({ message }: { message: string }) {
  return (
    <div className="error-alert">
      <AlertCircle size={18} />
      <span>{message}</span>
    </div>
  );
}
