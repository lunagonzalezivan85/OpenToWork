import { API_URL } from '../config';
import { clearSession, getAccessToken, getRefreshToken, setSession } from './session';
import type { AuthResponse } from '../types';

/** 401 -> intenta una renovacion silenciosa con el refresh guardado y reintenta UNA vez. */
async function request(path: string, init: RequestInit = {}, retry = true): Promise<Response> {
  const token = getAccessToken();
  const headers = new Headers(init.headers);
  headers.set('Content-Type', 'application/json');
  if (token) headers.set('Authorization', `Bearer ${token}`);

  const res = await fetch(`${API_URL}${path}`, { ...init, headers });
  if (res.status !== 401 || !retry) return res;

  const refreshToken = await getRefreshToken();
  if (!refreshToken) return res;

  const refresh = await fetch(`${API_URL}/api/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken }),
  });
  if (!refresh.ok) {
    await clearSession();
    return res;
  }
  const auth = (await refresh.json()) as AuthResponse;
  setSession(auth);
  return request(path, init, false);
}

async function json<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await request(path, init);
  if (!res.ok) {
    const body = await res.text().catch(() => '');
    throw new ApiError(res.status, body);
  }
  return res.status === 204 ? (undefined as T) : (await res.json()) as T;
}

export class ApiError extends Error {
  status: number;
  constructor(status: number, body: string) {
    super(body || `HTTP ${status}`);
    this.status = status;
  }
}

export const api = {
  get: <T>(path: string) => json<T>(path),
  post: <T>(path: string, body?: unknown) =>
    json<T>(path, { method: 'POST', body: JSON.stringify(body ?? {}) }),
  put: <T>(path: string, body?: unknown) =>
    json<T>(path, { method: 'PUT', body: JSON.stringify(body ?? {}) }),
};
