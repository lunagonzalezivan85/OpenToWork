# Fase 3, Sub-fase 3.7: Estado "Verificado TD" Automatico

**IA:** Dsiezar
**Fecha:** 2026-09-01
**Estado:** Completada

---

## Respuestas a las preguntas de la sub-fase 3.7

**1. ¿Cual es el `OverallScore` minimo para "Verificado TD"?**

**70.** `EvidenceIndex` no puede llegar a 100 hoy de forma estructural (el stub de
`VerifyIdentityAsync` siempre da `Pending`, nunca `Verified` - `fase-3-sub2.md`), asi que un
umbral de 80 haria el estado practicamente inalcanzable para cualquier candidato actual; 60 lo
haria demasiado laxo para un distintivo que se supone que es la categoria mas alta.

**2. ¿Cuantas verificaciones deben pasar como minimo? ¿Todas o un subconjunto?**

Un subconjunto: **LinkedIn, Portfolio, CvCoherence y Reference** (4 de los 5 `VerificationType`)
- se excluye **Identity** explicitamente porque es un stub permanente que nunca deja de estar
`Pending` (documentado desde `fase-3-sub2.md`). Exigir literalmente "todas" incluiria Identity
y volveria "Verificado TD" inalcanzable para siempre, lo cual contradice el objetivo de la
sub-fase.

**3. ¿Las referencias verificadas son obligatorias o solo recomendadas?**

Obligatorias, tal como ya lo dice el propio criterio de "Verificado TD" en el plan
("referencias verificadas"). Se exige al menos 1 referencia con `Status=Verified` - mismo
umbral que ya usa el componente Reference del `EvidenceIndex` (`fase-3-sub5.md`), para no
introducir un segundo numero distinto (ej. las 3 recomendadas de `HasMinimumReferences`) sin
necesidad.

**4. ¿Se recalcula automaticamente o hay un job periodico?**

Se calcula **en vivo en cada lectura** (`GetVerificationStatusAsync`/
`EvaluateVerificationStatusAsync`) contra el estado actual de `PTCandidateScore`,
`PT_Verifications` y `PTCandidateReference` - **no se persiste en ninguna tabla nueva**, no
hace falta migracion para esta sub-fase. Mismo criterio "sin Hangfire/Quartz" ya repetido en
3.2-3.6.

**5. ¿Si pierde una verificacion despues (ej: portfolio cae), pierde el estado automaticamente?**

Si, es consecuencia directa de la pregunta 4: al no persistirse nada, cada consulta refleja la
realidad actual. **Verificado en pruebas reales**: un candidato en `VerifiedTD` (score 82, 4/4
gating verificado) paso a `InProgress` (score 78, 3/4 verificado) en el momento en que su
portfolio dejo de ser alcanzable y se re-corrieron las verificaciones - sin ninguna accion
manual aparte de re-ejecutar `POST verifications/run`.

**6. ¿El distintivo ★ aparece en el perfil publico? ¿Como se muestra?**

La UI (el badge visual, donde se muestra, si las empresas lo ven) es la **sub-fase 3.8**. Lo
que se agrega ahora es el campo `IsVerifiedTD` (bool) en `VerificationStatusDto` para que 3.8
lo consuma directamente sin tener que tocar el backend de nuevo.

**7. ¿El candidato recibe notificacion al alcanzar "Verificado TD"?**

No se implementa. No hay SMTP ni ninguna infraestructura de notificaciones generica en el
proyecto (mismo gap ya documentado repetidamente desde `fase-3-sub5.md` pregunta 2). El
candidato se entera al consultar su propio `GET verification-status` (o, mas adelante, via el
badge que construya 3.8).

**8. ¿Se puede revocar manualmente desde el admin? ¿Quien tiene ese poder?**

**No se construye un endpoint dedicado de "revocar estado".** Como el estado no se persiste
(pregunta 4), no hay un valor que "revocar" directamente - el estado es siempre el resultado de
evaluar datos que ya existen. Un admin puede influir en el resultado indirectamente marcando
una verificacion individual como `Failed` desde la futura pantalla de "verificaciones
manuales" (el item de Fase 4 que quedo desbloqueado desde `fase-3-sub1.md`, UI pendiente para
3.8). No se fabrica un campo de override que no esta en el esquema.

## Nota de interpretacion (mas alla de las 8 preguntas)

El diagrama del plan ordena los estados como `Perfil registrado → Perfil completo → Evaluado →
Verificacion en proceso → Verificado TD`, poniendo "Evaluado" **antes** que "Verificacion en
proceso". Se interpreto asi: exactamente 3 de las 4 verificaciones gating corridas (sin
importar si pasaron o fallaron) + `OverallScore > 0` = **Evaluado**; las 4 corridas pero sin
cumplir todo (no todas verificadas, o falta el umbral de score, o falta la referencia) =
**Verificacion en proceso**; las 4 verificadas + score >= 70 + referencia verificada =
**Verificado TD**. Esto es coherente porque ninguno de los 4 checks gating queda nunca
realmente "pendiente" en un sentido asincrono (`ValidationService` resuelve todo de forma
sincronica al instante) - "pendiente" en la practica significa "todavia no se corrio esa
verificacion", no el enum `VerificationCheckStatus.Pending`.

## Que se implemento

**Sin migracion nueva** - todo se calcula en vivo. Nuevo enum
`OpenToWork.Shared.Enums.CandidateVerificationStatus` (ProfileRegistered=0, ProfileComplete=1,
Evaluated=2, InProgress=3, VerifiedTD=4).

`IVerificationStatusService`/`VerificationStatusService` (`OpenToWork.Core`):
- `GetVerificationStatusAsync`/`EvaluateVerificationStatusAsync` - ambos hacen lo mismo (el
  plan los lista como 2 metodos separados, se mantienen distintos en la interfaz por eso).
- Reutiliza la formula de completitud de perfil de `ApplicationService.CalculateProfileCompletion`
  (15 campos) **duplicada deliberadamente** en vez de extraerla a un helper compartido, para no
  tocar `ApplicationService` en esta sub-fase - documentado que hay que mantenerlas en sync si
  se agrega/quita un campo.

Endpoint: `GET api/candidates/{id}/verification-status` en `CandidatesController`, mismo guard
de ownership que `/verifications/run` y `/score/recalculate`.

**Nota tecnica**: un `array.Contains((VerificationType)v.Type)` dentro del predicado LINQ-a-SQL
fallaba en tiempo de ejecucion (`TypeLoadException` sobre `ReadOnlySpan` al intentar traducir la
expresion con EF Core 8 + .NET 10) - se resolvio precalculando la lista de enteros
(`GatingVerificationTypeInts`) fuera de la consulta y comparando contra `v.Type` (int) en vez de
castear el campo de la entidad dentro del predicado.

## Verificacion

Contra MySQL real y la API corriendo, con el candidato de prueba (`donald@gmail.com`):

1. **Estado "Evaluado"**: con LinkedIn+Portfolio verificados y CvCoherence fallido (3
   corridas), `OverallScore=42` → `status=2 (Evaluated)`.
2. **Camino a "Verificado TD"**: se agrego una experiencia de 2 anos (sube Stability), una
   skill que coincide con una vacante activa (sube Compatibility a 100), y una referencia
   verificada via el flujo publico completo de 3.5; se re-corrieron las verificaciones
   (CvCoherence paso a Verified al no haber gaps) → `OverallScore=82`, `gatingChecksRun=4`,
   `gatingChecksVerified=4`, `hasVerifiedReference=true` → `status=4 (VerifiedTD)`,
   `isVerifiedTD=true`.
3. **Perdida automatica (pregunta 5)**: se rompio el portfolio (URL invalida), se re-corrieron
   verificaciones y se recalculo el score → `OverallScore=78`, `gatingChecksVerified=3` →
   `status=3 (InProgress)`, `isVerifiedTD=false`, sin ninguna accion manual de "revocar".
4. Datos de prueba (experiencia, referencia, skills, verificacion Reference) limpiados,
   portfolio restaurado; el estado volvio a `Evaluated` (`OverallScore=48`, 3/3 gating
   verificado, coherente con el estado previo a la sub-fase.

`dotnet build` sin errores en `OpenToWork.API` y `OpenToWork.AdminAPI`.
