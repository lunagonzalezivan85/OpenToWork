# Trato Directo — App multiplataforma

Ionic React + Capacitor. Una sola base de código para **web, Android e iOS**, contra la
misma API .NET que usa el portal Blazor (`OpenToWork.API`).

## Arranque en desarrollo

```bash
cd app-multiplataforma
npm install
npm run dev          # vite en http://localhost:5173 (o el puerto libre)
```

La API espera en `src/config.ts` (`VITE_API_URL`, por defecto `http://localhost:5100`).
En dev la API debe tener el origen del dev-server en `Cors:AllowedOrigins`
(ya incluye `:5150` en `appsettings.Development.json`).

## Builds nativas

```bash
npm run build        # genera dist/
npx cap add android  # (requiere Android Studio / SDK)
npx cap sync
npx cap open android
```

iOS requiere macOS con Xcode (`npx cap add ios`).

## Arquitectura

```
src/
├── config.ts                API base
├── types.ts                 DTOs espejo de OpenToWork.Shared
├── services/
│   ├── session.ts           access token en MEMORIA + refresh en Capacitor
│   │                        Preferences (misma politica H-04 que el portal:
│   │                        nada de sesión en localStorage en nativo)
│   └── api.ts               fetch con Bearer + renovación silenciosa 1 vez en 401
├── auth/AuthContext.tsx     login/logout/refresh de arranque
└── pages/
    ├── Login.tsx
    ├── candidate/  Vacancies.tsx (buscar + postularse), Applications.tsx
    ├── company/    MyVacancies.tsx, CandidateSearch.tsx (verificada + solicitar)
    └── compartidas Messages.tsx, Profile.tsx (logout + revoke-all)
```

## Seguridad heredada

- Mismos gates del servidor: búsqueda de candidatos solo empresa verificada (403),
  resultados anonimizados, acceso completo solo tras entrega del staff.
- `POST /api/auth/revoke-all` expuesto en Perfil ("cerrar sesión en todos los dispositivos").
- El refresh viaja por body `{refreshToken}` (la API lo acepta además de la cookie
  `td_refresh`; en nativo no hay navegador que gestione cookies).

## Pendiente (roadmap funcional)

- Registro / recuperar contraseña / verificación de correo por código.
- Vacante detalle, postulación con carta y salario esperado.
- Perfil completo (wizard), CV y foto.
- Mensajes: hilo + envío (ahora solo lista de conversaciones).
- Empresa: crear/editar/publicar/cerrar vacante, entregas, scorecard.
- Notificaciones push, geolocalización (radio de búsqueda), cámara.
- i18n es/en (los textos actuales van en español).
- `Cypress`/`vitest` del template sin adaptar; el `npm audit` marca dependencias
  solo-dev (cypress/vitest) y `react-router` 6 ligado a `@ionic/react-router` 9.
