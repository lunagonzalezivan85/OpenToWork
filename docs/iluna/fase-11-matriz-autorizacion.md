# Fase 11 — Matriz de autorización por endpoint (rama `iluna-sec-rgpd`, continuación)

Auditoría H-10: revisión de todos los controllers del candidato API y AdminAPI verificando
`[Authorize]`, ownership y rol esperado por endpoint.

## Resultado de la revisión

### `OpenToWork.API` (portal candidato/empresa)

| Controller | Protección | Estado |
|---|---|---|
| `AuthController` | Sin auth de clase; `[Authorize]` en email-verification/check-device/revoke; rate-limit `auth` en públicos | ✅ correcto |
| `CandidatesController` | `[Authorize]` clase; ownership (`myCandidate.Id == id`) o `CanViewCandidateAsync` en cada `{id}` | ✅ correcto (search cerrado 403) |
| `CompaniesController` | `me` con `[Authorize]`; `GET /` y `/{id}` públicos intencionados ("empresas que confían") | ✅ correcto |
| `ReferencesController` | `send` `[Authorize]`+ownership; `feedback` público intencionado (token por correo) | ✅ correcto |
| `VacanciesController` | `[Authorize]` clase; temp-vacancies scoped a `SCUserId` del creador; `search` público intencionado | ✅ correcto |
| `PermanentVacanciesController` | `[Authorize]` clase; `search`/`{idOrCode}`/`company/{id}` `[AllowAnonymous]` (rutas públicas TD-XXXX); `Update`/`Delete`/`publish`/`close` verifican `OwnsVacancyAsync` | ✅ correcto |
| `ApplicationsController` | `[Authorize]` clase; Apply requiere perfil de candidato | ⚠️→✅ corregido: **faltaba exigir vacante `Active`** — se podía postular a borradores/cerradas con el Guid |
| `DeliveriesController` | `[Authorize]`; `respond` verifica `delivery.Company.SCUserId` | ✅ correcto |
| `MessagesController` | `[Authorize]`; conversaciones y mensajes verifican `c.SCUserId == userId` | ✅ correcto |
| `ChallengesController` | `[Authorize]`; `company/candidates/{id}/challenge-results` vía `GetResultsForCompanyAsync` (relación de entrega) | ✅ correcto |
| `ProfileController` | `[Authorize]`; todo scoped a `userId` salvo `candidate/{id}` que usa `CanViewCandidateAsync` | ✅ correcto |
| `AlertsController`, `JobTypesController`, `SkillsController`, `SkillTestsController`, `VerificationRequestsController` | `[Authorize]` clase, scoped al usuario | ✅ correcto |
| `NewsController`, `PlansController`, `LegalController` | Públicos intencionados (contenido público/feature flags/legal) | ✅ correcto |

### `OpenToWork.AdminAPI`

| Patrón | Estado |
|---|---|
| Todos los controllers heredan `AdminControllerBase` = `[Authorize(Roles = "Admin")]` | ✅ |
| `AdminAuthController` (login) sin auth — intencionado | ✅ |

## Corrección aplicada

- `POST /api/applications`: ahora exige `vacancy.Status == Active` (además de existir y no
  estar borrada). Antes un candidato con el Guid podía postularse a un borrador o a una
  vacante cerrada — ni el search público ni la UI las muestran, pero el endpoint las aceptaba.

## Decisiones pendientes (documentadas, no aplicadas)

- **Empresa sin verificar** puede crear/editar vacantes y usar su dashboard. Las vacantes
  pasan por publicación controlada, pero si se decide bloquear, el punto es
  `PermanentVacanciesController.Create` + `VacanciesController.temp`.
- Rol "empresa" vs "candidato" en endpoints compartidos se resuelve por presencia de perfil
  (empresa sin PTCandidate → BadRequest). Funciona, pero `[Authorize(Roles=...)]` explícito
  sería más claro y fallaría antes — mejora opcional, no hueco.
