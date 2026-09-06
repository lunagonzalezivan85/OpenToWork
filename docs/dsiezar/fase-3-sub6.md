# Fase 3, Sub-fase 3.6: Pruebas de Habilidades (Retos Tecnicos)

**IA:** Dsiezar
**Fecha:** 2026-09-01
**Estado:** Completada

---

## Respuestas a las preguntas de la sub-fase 3.6

**1. ¿Los retos son multiple choice, codigo ejecutable, o ambos?**

Ya decidido en la sub-fase 3.1 (ver `fase-3-sub1.md`): **solo multiple choice**.
`PTSkillTest.Questions` es JSON `[{question, options, correctIndex}]`. Codigo ejecutable
necesitaria un judge/sandbox externo (Judge0, Piston, o propio) - alcance mucho mayor,
explicitamente fuera de esta sub-fase.

**2. ¿El puntaje es automatico o requiere revision manual de TD?**

100% automatico - al ser multiple choice, `SubmitTestAsync` compara cada respuesta contra
`correctIndex` y calcula `Score = correctas/total * 100` sin intervencion humana.

**3. ¿Judge online o tests unitarios propios?**

No aplica - ya no hay codigo ejecutable que evaluar (pregunta 1).

**4. ¿Que medidas anti-copia tiene?**

- **Cambio de pestana/blur:** el cliente reporta un contador (`antiCheatFlags`, query param de
  `submit`) que se guarda en `PTCandidateTestResult.AntiCheatFlags` - informativo para revision
  futura, no invalida el intento automaticamente (definir un umbral de penalizacion no estaba
  en el plan y hubiera sido una regla inventada sin base).
- **Tiempo limite:** enforced del lado del servidor con el nuevo campo `StartedAt` (no estaba
  en el esquema de 3.1) comparado contra `PTSkillTest.TimeLimit` (minutos) - si se supera, el
  intento se auto-completa con `Score=0` de forma perezosa (sin job programado, mismo criterio
  ya usado en 3.2-3.5).
- **Copiar/pegar:** no es bloqueable de forma confiable desde el backend - depende de JS en el
  cliente, queda para la UI en la sub-fase 3.8.

**5. ¿Cuantos intentos tiene el candidato por reto?**

1 intento por reto. Si el intento existente nunca se completo y **no vencio** el tiempo,
`StartTestAsync` es idempotente y devuelve el mismo intento en curso (no crea uno nuevo,
pregunta 8). Si ya se completo (enviado o vencido por tiempo), un segundo `StartTestAsync`
lanza un error - verificado en pruebas con ambos casos (submit exitoso y timeout).

**6. ¿Los resultados suman al `CandidateScore`? ¿A que indice?**

Si, a `EvidenceIndex` - un reto de habilidades aprobado es evidencia objetiva de una
competencia, igual espiritu que los demas checks de ese indice. Como ya eran 5 componentes de
20 pts (sub-fase 3.5), se re-pesa a numeros redondos que reflejan cuanto dice cada check:
**LinkedIn 15 / Portfolio 15 / CvCoherence 20 / Identity 15 / Reference 15 / SkillTest 20**
(suma 100 - los checks binarios simples bajan a 15, los mas sustantivos - coherencia de todo
el CV, reto aprobado - quedan en 20). El componente `SkillTest` cuenta completo si el
candidato tiene al menos 1 resultado completado con `Score >= 60` (mismo umbral que "aprobado").

**7. ¿Se pueden ver los retos antes de completar el perfil?**

La lista (`GetAvailableTestsAsync` / `GET api/skill-tests/available`) es visible siempre, sin
requisito. **Iniciar** un intento (`StartTestAsync`) si requiere `WizardCompleted=true` - mismo
criterio de elegibilidad ya usado en `CompatibilityService` (3.4). Verificado: la lista
respondio `200` con el wizard incompleto, pero `start` devolvio 400 "Wizard not completed".

**8. ¿Se puede retomar un reto tras cerrar el navegador, o se anula?**

Se puede retomar mientras no haya vencido `StartedAt + TimeLimit` - verificado que un segundo
`start` sobre el mismo reto devuelve el mismo `resultId` con `secondsRemaining` actualizado
(no crea un intento nuevo). Si ya vencio, se auto-completa como intento fallido (`Score=0`) la
proxima vez que se toca (via `submit`, `start`, o `GetTestResultsAsync`) - no se anula
silenciosamente, cuenta como el unico intento disponible (pregunta 5).

## Que se implemento

Migracion `SkillTestAttemptTracking`: agrega `StartedAt` y `Answers` (JSON) a
`PTCandidateTestResult` (no estaban en el esquema de 3.1, necesarios para enforcar tiempo del
lado servidor y guardar lo enviado). Mismo problema de `Guid.NewGuid()` en el seed de
`SY_WizardSteps`, limpiado de la migracion de la misma forma que las anteriores.

`ISkillTestService`/`SkillTestService` (`OpenToWork.Core`), sin HTTP:
- CRUD admin: `CreateSkillTestAsync`, `GetAllSkillTestsAsync`, `GetSkillTestByIdAsync`,
  `UpdateSkillTestAsync`, `DeleteSkillTestAsync`.
- Candidato: `GetAvailableTestsAsync` (usa `SkillTestPublicDto`, **nunca** expone
  `CorrectIndex` - verificado en la respuesta real), `StartTestAsync` (idempotente + gate de
  wizard), `SubmitTestAsync` (califica, marca `CompletedAt`, dispara
  `IScoringService.RecalculateAsync` automaticamente - mismo patron de auto-recalculo ya
  establecido en `ProfileService` desde 3.3), `GetTestResultsAsync` (lazy-expira intentos
  vencidos).
- `HasPassingResultAsync` (estatico) - usado por `ScoringService.CalculateEvidenceIndex` sin
  crear una dependencia circular de constructor entre los dos servicios.

Endpoints:
- CRUD admin bajo **`api/admin/skill-tests`** (no `api/skill-tests` como en el plan literal,
  para no colisionar con las rutas candidate-facing) en el nuevo `SkillTestsController` de
  `OpenToWork.AdminAPI`, `[Authorize(Roles="Admin")]` - mismo criterio ya usado en 3.4.
- Candidato en el nuevo `SkillTestsController` de `OpenToWork.API`: `GET
  api/skill-tests/available`, `POST api/skill-tests/{id}/start`, `POST
  api/skill-tests/results/{id}/submit`, `GET api/skill-tests/results` (historial propio, sin
  `{id}` en la ruta porque siempre es del candidato autenticado).

## Verificacion

Contra MySQL real y ambas APIs corriendo (login `admin@opentowork.com` / `donald@gmail.com`):

1. **CRUD admin**: se creo un test de 2 preguntas via `POST api/admin/skill-tests`.
2. **Sin fuga de respuestas**: `GET api/skill-tests/available` devolvio las preguntas **sin**
   `correctIndex` en el JSON.
3. **Flujo start → resume idempotente → submit**: un segundo `start` sobre el mismo reto
   devolvio el mismo `resultId` con `secondsRemaining` decreciente (60→40); `submit` con las 2
   respuestas correctas dio `Score=100`.
4. **EvidenceIndex actualizado automaticamente**: sin llamar al endpoint de recalculo aparte,
   el `submit` ya disparo `RecalculateAsync` - `EvidenceIndex=50` (LinkedIn 15 + Portfolio 15 +
   SkillTest 20, con CvCoherence/Identity/Reference en 0 de estado previo) coincide
   exactamente con la formula re-pesada.
5. **Rechazo de reintento**: un segundo `start` sobre el mismo reto ya completado devolvio 400
   "Attempt already used for this test".
6. **Vencimiento perezoso**: se creo un 2do test, se inicio, se adelanto `StartedAt` 5 minutos
   atras via SQL (tiempo limite de 1 minuto), y `GET results` lo marco automaticamente
   `Score=0, TimeTaken=300s` sin ningun job corriendo; un `start` posterior sobre ese mismo test
   tambien devolvio 400 (el timeout cuenta como el intento usado).
7. **Gate de wizard**: con `WizardCompleted=false`, la lista `available` siguio respondiendo
   `200`, pero `start` sobre un 3er test devolvio 400 "Wizard not completed".
8. Datos de prueba (3 `PTSkillTest`, 2 `PTCandidateTestResult`) borrados despues de verificar;
   `EvidenceIndex` volvio a 30 (LinkedIn+Portfolio con los nuevos pesos), confirmando la
   formula reescalada de forma consistente.

`dotnet build` sin errores en `OpenToWork.API` y `OpenToWork.AdminAPI`.
