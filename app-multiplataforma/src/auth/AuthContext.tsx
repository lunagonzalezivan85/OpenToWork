import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react';
import type { AuthResponse, User } from '../types';
import { api } from '../services/api';
import { clearSession, getAccessToken, getRefreshToken, getStoredUser, setSession } from '../services/session';

interface AuthState {
  user: User | null;
  /** true mientras se intenta la renovacion silenciosa del arranque. */
  loading: boolean;
  login(email: string, password: string): Promise<User>;
  logout(): Promise<void>;
}

const AuthCtx = createContext<AuthState>({
  user: null,
  loading: true,
  login: async () => { throw new Error('not ready'); },
  logout: async () => {},
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  // Arranque: si hay refresh guardado y no hay token en memoria (app recien
  // abierta), renovar en silencio - igual que el portal web con la cookie.
  useEffect(() => {
    (async () => {
      try {
        if (!getAccessToken() && (await getRefreshToken())) {
          const auth = await api.post<AuthResponse>('/api/auth/refresh', {
            refreshToken: await getRefreshToken(),
          });
          setSession(auth);
          setUser(auth.user);
        } else if (getAccessToken()) {
          setUser(await getStoredUser());
        }
      } catch {
        await clearSession();
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const auth = await api.post<AuthResponse>('/api/auth/login', { email, password });
    setSession(auth);
    setUser(auth.user);
    return auth.user;
  }, []);

  const logout = useCallback(async () => {
    try { await api.post('/api/auth/revoke', { refreshToken: await getRefreshToken() }); }
    catch { /* la sesion local se limpia igual */ }
    await clearSession();
    setUser(null);
  }, []);

  return <AuthCtx.Provider value={{ user, loading, login, logout }}>{children}</AuthCtx.Provider>;
}

export const useAuth = () => useContext(AuthCtx);
