# Fase 3, Sub-fase 3.2: ValidationService — Verificaciones Automaticas

**IA:** Dsiezar
**Fecha:** 2026-09-01
**Estado:** Completada (Identity/LinkedIn/Portfolio/CvCoherence - Education/Reference quedan
para otras sub-fases, ver nota abajo)

---

## Respuestas a las preguntas de la sub-fase 3.2

**1. ¿LinkedIn: scraping real o solo formato/URL?**

Solo formato (`linkedin.com/in/{slug}` via regex) + chequeo de alcanzabilidad con HEAD. Scraping
real violaria los Terminos de Servicio de LinkedIn y no hay infraestructura para eso en el
proyecto (ni proxy, ni manejo de bloqueo anti-bot).

**2. ¿Timeout de portfolio? ¿403/401?**

5 segundos. Solo `200` exacto cuenta como verificado (tal como pide el plan: "verifica que
responde 200"); `403`/`401`/timeout/excepcion se marcan `Failed` en esa verificacion puntual,
nunca lanzan ni bloquean el resto del flujo (`RunAllVerificationsAsync` sigue con las demas).

**3. ¿Como se define un "gap inexplicable"?**

Mas de 6 meses sin empleo entre dos experiencias consecutivas. Penalizacion proporcional a la
duracion del gap (`min(30, 10 + meses/2)`), multiplicada por 1.5 si el gap termino hace menos
de 2 anos (mas reciente = pesa mas, ya que refleja mejor la situacion actual del candidato).

**4. ¿Que se considera "superposicion sospechosa"?**

Dos experiencias cuyos rangos de fecha se solapan mas de 30 dias (se permite hasta un mes de
transicion normal entre trabajos sin marcar nada).

**5. ¿La verificacion de identidad que valida exactamente?**

Nada todavia, de forma honesta: `PTCandidate` no tiene un campo de documento de identidad
subido (`Identification` es solo un numero de texto, no un archivo). `VerifyIdentityAsync`
devuelve `Status=Pending` con el motivo documentado en `Result`, en vez de simular una
verificacion que no puede hacerse. Agregar el campo/flujo de subida queda para una sub-fase
futura (posiblemente 3.8, cuando se construya la UI).

**6. ¿Cada cuanto se re-ejecutan las verificaciones automaticamente?**

Bajo demanda unicamente, via `POST /api/candidates/{id}/verifications/run`. No hay job
periodico - el proyecto no tiene infraestructura de jobs (Hangfire/Quartz) instalada.

**7. ¿Si una verificacion falla, se reintenta automaticamente?**

No. Queda en `Status=Failed` (o `Pending` para Identity) hasta que se vuelva a llamar el
mismo endpoint manualmente.

**8. ¿El `Score` es binario o hay matices?**

Binario (100/0) para LinkedIn y Portfolio - son checks pass/fail claros (URL valida y
alcanzable, o no). Proporcional para CvCoherence (arranca en 100, resta puntos por cada
issue encontrado, clamp 0-100) - ahi si hay grados reales entre "perfecto" y "con problemas".

**9. ¿Lista completa de red flags?**

- **Salto laboral:** experiencia no-actual con duracion < 3 meses.
- **Gap inexplicado:** > 6 meses entre dos experiencias consecutivas.
- **Cambios de sector frecuentes:** fuera de alcance - `PTCandidateExperience` no tiene un
  campo de industria/sector todavia, no hay dato estructurado para detectar esto. Documentado
  como bloqueado, no simulado.

**10. ¿Las red flags afectan `ReliabilityIndex` o tienen campo separado?**

Campo separado por ahora: viven dentro del `Result` JSON de la verificacion `CvCoherence`
(mismo array `issues` que ya usa para gaps/solapamientos/saltos - no se duplica logica). Como
`ReliabilityIndex` se define recien en la Sub-fase 3.3, ahi se decide formalmente como
incorporar este dato al indice.

## Que se implemento

`IValidationService`/`ValidationService` (`OpenToWork.Core`), inyecta `HttpClient` (mismo
patron que `CvParserService`, registrado con `AddHttpClient<IValidationService, ValidationService>`):

- `VerifyLinkedInAsync(candidateId)`, `VerifyPortfolioAsync(candidateId)`,
  `VerifyCvCoherenceAsync(candidateId)`, `VerifyIdentityAsync(candidateId)` (stub documentado,
  pregunta 5), `DetectRedFlagsAsync(candidateId)` (usado internamente por CvCoherence),
  `RunAllVerificationsAsync(candidateId)`.
- Cada `VerifyXAsync` hace upsert de una fila en `PT_Verifications` (por `PT_CandidateId` +
  `Type`, indice unico ya creado en sub-fase 3.1).

**Nota de alcance:** `VerificationType` tiene 6 valores (`Identity, LinkedIn, Portfolio,
CvCoherence, Education, Reference`). Esta sub-fase solo implementa los primeros 4 - el plan de
la sub-fase 3.2 no lista un metodo para `Education` ni `Reference` (esos corresponden al
checklist de investigacion ya existente de Iluna, y a la sub-fase 3.5 de referencias
respectivamente). `RunAllVerificationsAsync` solo corre los 4 tipos implementados.

Endpoint: `POST /api/candidates/{id}/verifications/run` en `CandidatesController` - `{id}` es
el `Id` de `PTCandidate` (no `SCUserId`), consistente con el esquema de la sub-fase 3.1. Se
agrego un guard de ownership (el candidato solo puede disparar sus propias verificaciones -
evita que cualquier usuario autenticado gatille peticiones HTTP salientes contra otro
candidato) que el plan no pedia explicitamente pero es necesario por seguridad.

## Verificacion

Contra MySQL real y la API corriendo, con el candidato de prueba (`donald@gmail.com`):

1. **Sin datos** (sin LinkedIn/Portfolio/experiencias): `Identity=Pending`,
   `LinkedIn=Failed(0)`, `Portfolio=Failed(0)`, `CvCoherence=Verified(100)` (sin experiencias
   no hay issues que detectar) - todo sin excepciones.
2. **Con datos reales**: se seteo `linkedInUrl` con formato valido, `portfolioUrl=https://example.com`,
   y 2 experiencias (una de 1 mes = salto laboral, con un gap de 15 meses antes de la
   siguiente). Resultado: `LinkedIn=Verified(100)`, `Portfolio=Verified(100)`,
   `CvCoherence=Failed(68)` con `issues: ["Salto laboral: 'Junior Dev' en StartupX duro 1
   mes(es) (< 3)", "Gap de 15 mes(es) entre 'Junior Dev' y 'Backend Dev'"]` - coincide
   exactamente con la formula documentada (100 - 15 - 17 = 68).
3. **Ownership guard**: `POST` contra un `candidateId` que no es el propio del usuario
   autenticado devuelve `403 Forbidden`.
4. Datos de prueba (las 2 experiencias) borrados despues de verificar.

`dotnet build` sin errores en `OpenToWork.API` (arrastra `Core`/`Models`).
