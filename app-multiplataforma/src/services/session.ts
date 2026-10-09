import { Preferences } from '@capacitor/preferences';
import type { AuthResponse } from '../types';

/**
 * Misma politica que el portal web (auditoria 08-Oct H-04):
 *  - access token SOLO en memoria de esta instancia de la app;
 *  - refresh token en Capacitor Preferences (en nativo es almacenamiento del
 *    sistema, no JS-webview-readable como localStorage; en web dev cae en
 *    localStorage - aceptable solo para desarrollo);
 *  - el refresh viaja por BODY (la API acepta {refreshToken}), no por cookie.
 */
const REFRESH_KEY = 'td-refresh';
const USER_KEY = 'td-user';

let accessToken: string | null = null;

export function setSession(auth: AuthResponse) {
  accessToken = auth.token;
  void Preferences.set({ key: REFRESH_KEY, value: auth.refreshToken });
  void Preferences.set({ key: USER_KEY, value: JSON.stringify(auth.user) });
}

export function getAccessToken() {
  return accessToken;
}

export async function getRefreshToken(): Promise<string | null> {
  return (await Preferences.get({ key: REFRESH_KEY })).value;
}

export async function getStoredUser() {
  const raw = (await Preferences.get({ key: USER_KEY })).value;
  return raw ? (JSON.parse(raw) as AuthResponse['user']) : null;
}

export async function clearSession() {
  accessToken = null;
  await Preferences.remove({ key: REFRESH_KEY });
  await Preferences.remove({ key: USER_KEY });
}
