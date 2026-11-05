import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { getCurrentUser, login as loginRequest } from '../api/authApi';
import { ACCESS_TOKEN_KEY } from '../api/axios';
import type { AuthResponse, AuthUser, LoginRequest } from '../types/auth';

interface AuthContextValue {
  user: AuthUser | null;
  token: string | null;
  isLoading: boolean;
  login: (request: LoginRequest) => Promise<AuthResponse>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [token, setToken] = useState<string | null>(() => sessionStorage.getItem(ACCESS_TOKEN_KEY));
  const [isLoading, setIsLoading] = useState(true);

  const logout = useCallback(() => {
    sessionStorage.removeItem(ACCESS_TOKEN_KEY);
    setToken(null);
    setUser(null);
  }, []);

  useEffect(() => {
    const restore = async () => {
      if (!token) {
        setIsLoading(false);
        return;
      }
      try {
        setUser(await getCurrentUser());
      } catch {
        logout();
      } finally {
        setIsLoading(false);
      }
    };
    void restore();
  }, [token, logout]);

  useEffect(() => {
    const onExpired = () => logout();
    window.addEventListener('rms:auth-expired', onExpired);
    return () => window.removeEventListener('rms:auth-expired', onExpired);
  }, [logout]);

  const login = useCallback(async (request: LoginRequest) => {
    const response = await loginRequest(request);
    sessionStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    setToken(response.accessToken);
    setUser(response.user);
    return response;
  }, []);

  const value = useMemo(() => ({ user, token, isLoading, login, logout }), [user, token, isLoading, login, logout]);
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
