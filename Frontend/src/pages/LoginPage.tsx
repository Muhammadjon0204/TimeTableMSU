import { AxiosError } from 'axios';
import { Building2, LockKeyhole, Mail } from 'lucide-react';
import { useState } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { ErrorAlert } from '../components/ErrorAlert';

export function LoginPage() {
  const navigate = useNavigate();
  const { accessToken, isAdmin, login, logout } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (accessToken && isAdmin) {
    return <Navigate to="/admin" replace />;
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      const result = await login(email, password);

      if (result.role !== 'Admin') {
        setError('Доступ разрешен только администраторам.');
        logout();
        return;
      }

      navigate('/admin');
    } catch (requestError) {
      const data = requestError instanceof AxiosError ? (requestError.response?.data as { error?: string }) : undefined;
      setError(data?.error ?? 'Не удалось войти с указанными данными.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="login-page">
      <section className="login-card">
        <div className="login-brand">
          <div className="brand-mark large">
            <Building2 size={30} />
          </div>
          <div>
            <p className="eyebrow">Административный доступ</p>
            <h1>Вход в систему</h1>
            <p className="login-subtitle">Панель управления университетским расписанием</p>
          </div>
        </div>

        {error ? <ErrorAlert message={error} /> : null}

        <form className="login-form" onSubmit={handleSubmit}>
          <label>
            <span>Email</span>
            <div className="input-with-icon">
              <Mail size={17} />
              <input type="email" value={email} required onChange={(event) => setEmail(event.target.value)} />
            </div>
          </label>

          <label>
            <span>Пароль</span>
            <div className="input-with-icon">
              <LockKeyhole size={17} />
              <input type="password" value={password} required onChange={(event) => setPassword(event.target.value)} />
            </div>
          </label>

          <button className="primary-button full" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Вход...' : 'Войти'}
          </button>
        </form>
      </section>
    </main>
  );
}
