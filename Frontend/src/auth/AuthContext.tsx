import { createContext, type ReactNode, useContext, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { axiosClient, setLogoutCallback } from '../api/axiosClient';

type LoginResponse = {
  accessToken: string;
  refreshToken?: string;
  role: string;
  fullName?: string;
  email?: string;
};

type AuthState = {
  accessToken: string | null;
  refreshToken: string | null;
  role: string | null;
  fullName: string | null;
  email: string | null;
};

type AuthContextValue = AuthState & {
  isAdmin: boolean;
  login: (email: string, password: string) => Promise<LoginResponse>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const initialAuthState: AuthState = {
  accessToken: localStorage.getItem('accessToken'),
  refreshToken: localStorage.getItem('refreshToken'),
  role: localStorage.getItem('role'),
  fullName: localStorage.getItem('fullName'),
  email: localStorage.getItem('email'),
};

export function AuthProvider({ children }: { children: ReactNode }) {
  const [authState, setAuthState] = useState<AuthState>(initialAuthState);
  const navigate = useNavigate();

  async function login(email: string, password: string) {
    const response = await axiosClient.post<unknown>('/auth/login', { email, password });
    const data = response.data as Record<string, unknown>;

    // Support multiple response formats for accessToken
    const accessToken =
      (data.accessToken as string) ??
      (data.token as string) ??
      (data.jwtToken as string) ??
      ((data.value as Record<string, unknown>)?.accessToken as string) ??
      ((data.value as Record<string, unknown>)?.token as string);

    if (!accessToken) {
      throw new Error('Не удалось получить access token от сервера');
    }

    // Support multiple response formats for refreshToken
    const refreshToken =
      (data.refreshToken as string) ??
      ((data.value as Record<string, unknown>)?.refreshToken as string);

    // Support multiple response formats for role
    const role =
      (data.role as string) ??
      (data.userRole as string) ??
      ((data.value as Record<string, unknown>)?.role as string) ??
      ((data.value as Record<string, unknown>)?.userRole as string);

    // Support multiple response formats for email
    const emailFromResponse =
      (data.email as string) ??
      ((data.value as Record<string, unknown>)?.email as string);

    const nextState: AuthState = {
      accessToken,
      refreshToken: refreshToken ?? null,
      role: role ?? null,
      fullName: (data.fullName as string) ?? ((data.value as Record<string, unknown>)?.fullName as string) ?? null,
      email: emailFromResponse ?? email,
    };

    localStorage.setItem('accessToken', nextState.accessToken ?? '');
    localStorage.setItem('refreshToken', nextState.refreshToken ?? '');
    localStorage.setItem('role', nextState.role ?? '');
    localStorage.setItem('fullName', nextState.fullName ?? '');
    localStorage.setItem('email', nextState.email ?? '');
    setAuthState(nextState);

    return {
      accessToken,
      refreshToken: refreshToken ?? undefined,
      role: role ?? '',
      fullName: nextState.fullName ?? undefined,
      email: nextState.email ?? undefined,
    };
  }

  function logout() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('role');
    localStorage.removeItem('fullName');
    localStorage.removeItem('email');
    setAuthState({
      accessToken: null,
      refreshToken: null,
      role: null,
      fullName: null,
      email: null,
    });
  }

  // Initialize logout callback for 401 handling
  useEffect(() => {
    const handleUnauthorized = () => {
      logout();
      navigate('/login', { replace: true });
      alert('Сессия истекла. Войдите снова.');
    };

    setLogoutCallback(handleUnauthorized);
  }, [navigate]);

  const value = useMemo<AuthContextValue>(
    () => ({
      ...authState,
      isAdmin: authState.role === 'Admin',
      login,
      logout,
    }),
    [authState],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider');
  }

  return context;
}
