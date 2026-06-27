import { createContext, type ReactNode, useContext, useMemo, useState } from 'react';
import { axiosClient } from '../api/axiosClient';

type LoginResponse = {
  accessToken: string;
  refreshToken: string;
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

  async function login(email: string, password: string) {
    const response = await axiosClient.post<LoginResponse>('/auth/login', { email, password });
    const nextState: AuthState = {
      accessToken: response.data.accessToken,
      refreshToken: response.data.refreshToken,
      role: response.data.role,
      fullName: response.data.fullName ?? null,
      email: response.data.email ?? email,
    };

    localStorage.setItem('accessToken', nextState.accessToken ?? '');
    localStorage.setItem('refreshToken', nextState.refreshToken ?? '');
    localStorage.setItem('role', nextState.role ?? '');
    localStorage.setItem('fullName', nextState.fullName ?? '');
    localStorage.setItem('email', nextState.email ?? '');
    setAuthState(nextState);

    return response.data;
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
