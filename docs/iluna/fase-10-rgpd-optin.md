# Fase 10 — RGPD: opt-in de visibilidad del candidato (rama `iluna-sec-rgpd`)

Base legal y minimización para poder reabrir la búsqueda de candidatos (H-39/H-27)
de forma conforme. La búsqueda sigue **cerrada** (403, contención de Dsiezar) — esta
fase construye la infraestructura de consentimiento que el rediseño necesita.

## Problema

`PTCandidate.IsProfilePublic` existía (default `true` y checkbox en el perfil), pero:

- todos los candidatos existentes eran "públicos" sin haberlo pedido nunca;
- no había **registro del consentimiento** (cuándo, con qué texto, si se revocó);
- el resultado de búsqueda exponía el nombre completo en una lista agregada.

## Cambios

### Modelo (`PTCandidate`, migración `CandidateVisibilityConsent`)

| Campo | Significado |
|---|---|
| `VisibilityConsentAt` | Último consentimiento explícito (UTC). `null` = nunca consintió |
| `VisibilityConsentRevokedAt` | Última revocación (queda como rastro de auditoría) |
| `VisibilityConsentVersion` | Versión del texto aceptado (`rgpd-v1`; subir si cambia el texto) |

**Criterio de visibilidad efectiva** (visible para empresas/matching):
`IsProfilePublic && VisibilityConsentAt != null && (RevokedAt == null || RevokedAt < ConsentAt)`

### Registro del consentimiento (`ProfileService.UpdateProfileAsync`)

- Guardar con el checkbox **marcado** y sin consentimiento previo, o reactivarlo tras
  haberlo quitado → `VisibilityConsentAt = ahora`, `RevokedAt = null`, versión `rgpd-v1`.
- Desmarcarlo → `IsProfilePublic = false` + `VisibilityConsentRevokedAt = ahora`
  (se conserva `ConsentAt` como evidencia histórica).

Candidatos preexistentes: `IsProfilePublic=true` pero `ConsentAt=null` → **dejan de ser
visibles en búsqueda/matching** hasta que confirmen (una sola vez: abrir el perfil y
guardar con el checkbox marcado, ya con el texto informado al lado).

### Minimización de datos

`CandidateSearchService` y `CompatibilityService` ya filtran por el criterio de
consentimiento. El resultado de búsqueda devuelve el nombre enmascarado
(`"María García" → "María G."`): la identidad completa solo viaja por la vía de
entrega controlada (`CanViewCandidateAsync` — el candidato a sí mismo o empresa a la
que TD lo entregó).

### UI (`Profile.razor`)

El checkbox pasa a ser "Mostrar mi perfil a empresas verificadas" con texto informado
debajo (qué se comparte y qué no, a quién, y que se puede retirar). Si hay
consentimiento, se muestra la fecha (`consentSince`). Textos es/en.

## EIPD (resumen de evaluación de impacto)

| Punto | Decisión |
|---|---|
| Finalidad | Presentar candidatos a empresas verificadas para empleo |
| Base jurídica | Consentimiento explícito del candidato (art. 6.1.a), revocable |
| Datos compartidos | Titular, ciudad, país, habilidades, score — sin apellido completo ni contacto |
| Destinatarios | Solo empresas `IsVerified` por el equipo TD (cuando se reabra) |
| Revocación | Checkbox del perfil, efecto inmediato, timestamp registrado |
| Conservación | Consentimiento/revocación se conservan como evidencia; perfil dura la cuenta |

## Verificación

- `PUT /api/profile` con `isProfilePublic:true` → `VisibilityConsentAt` + `rgpd-v1` ✅
- `PUT` con `false` → `IsProfilePublic=0` + `RevokedAt` ✅ (verificado en BD)
- Migración aplicada sin errores; build 0 errores.
- Búsqueda sigue respondiendo 403 (cierre de contención intacto).

## Pendiente del rediseño de búsqueda (H-39)

- Decidir el flujo de entrega concreto (match → TD revisa → entrega a empresa).
- Checkbox de opt-in también en el registro del candidato (doble consentimiento:
  servicio + visibilidad).
- Posible panel del candidato: "a qué empresas me han presentado".
