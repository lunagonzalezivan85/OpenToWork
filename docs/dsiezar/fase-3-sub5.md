# Fase 3, Sub-fase 3.5: Referencias Laborales Automaticas

**IA:** Dsiezar
**Fecha:** 2026-09-01
**Estado:** Completada

---

## Respuestas a las preguntas de la sub-fase 3.5

**1. ¿Cuantas referencias minimas se exigen?**

3, pero no bloqueante a nivel de servicio - `AddReferenceAsync` no rechaza si hay menos.
`GetReferencesAsync` expone `HasMinimumReferences` (bool) para que una sub-fase futura (3.7,
"Estado Verificado TD Automatico", que define los criterios de cada estado) lo use como
criterio de gating sin tener que reimplementar el conteo.

**2. ¿El email se envia via SMTP, o se genera un link que el candidato comparte?**

**No hay infraestructura SMTP en todo el proyecto** - `AuthService.RequestPasswordResetAsync`
ya tiene el mismo gap documentado desde antes (`// TODO: Send email with reset link`, nunca
implementado, el token se devuelve tal cual). Se sigue exactamente ese mismo patron:
`SendReferenceRequestAsync` genera un token + link y lo devuelve en la respuesta del endpoint
para que el candidato lo copie y comparta manualmente (WhatsApp, email personal, etc.) - no se
fabrica un envio real que no existe en ningun otro lado del sistema.

**3. ¿El contacto necesita cuenta, o responde via link publico con token?**

Link publico con token, sin cuenta. El token se genera con
`ITokenCryptoService.GenerateRefreshToken()` y se guarda hasheado (`TokenHash`) con
`HashToken()` - mismo mecanismo que `SCUser.PasswordResetToken`, nunca se persiste en claro.

**4. ¿Que informacion se le pide al contacto?**

Rating (1-5, clamped) + Feedback (texto libre) - los campos que ya existian en el esquema de
3.1. No se pide confirmar datos del candidato aparte porque no hay campos para eso y agregar
una tabla de confirmacion separada seria expandir el esquema sin necesidad.

**5. ¿Las referencias verificadas suman al `EvidenceIndex`? ¿Cuanto?**

Si. Se reescala `ScoringService.CalculateEvidenceIndex` de 4 componentes de 25 pts a **5 de 20
pts** (LinkedIn/Portfolio/CvCoherence/Identity/**Reference**), mismo maximo de 100. Se usa
`VerificationType.Reference` (ya reservado en el enum desde la sub-fase 3.1) sobre la misma
tabla `PT_Verifications` - cuando al menos 1 referencia del candidato esta `Verified`, se
upsertea esa fila a `Verified/100`, igual mecanismo que `ValidationService`.

**6. ¿Si no responde en X dias se marca fallida? ¿Cuanto es X?**

X = 7 dias, mismo plazo que la vigencia del link (`TokenExpiresAt = SentAt + 7 dias`). Se
evalua de forma **perezosa** (sin job programado, mismo criterio ya usado en 3.2/3.3/3.4): cada
vez que se lee o se toca una referencia (`GetReferencesAsync`, `VerifyReferenceAsync`,
`SubmitReferenceFeedbackAsync`) se chequea si esta `Sent` y vencida, y si lo esta se marca
`Failed` en ese momento.

**7. ¿El candidato puede ver el feedback, o es privado para TD?**

Visible para el candidato - es su propia referencia, la que el mismo aporto y gestiona desde su
perfil (distinta de `PTReferenceCheck` de Iluna, que si es la investigacion privada del
reclutador durante el Pipeline de Reclutamiento).

**8. ¿Se valida que las referencias no sean de la misma empresa donde trabajo?**

Validacion blanda, no bloqueante: si dos referencias del mismo candidato comparten el mismo
`CompanyName` (comparacion de texto simple, case-insensitive), ambas se marcan con
`SameCompanyAsAnotherReference=true` en la respuesta - es solo un aviso, no bloquea el alta ni
el envio. No se cruza contra el historial real en `PTCandidateExperience` porque los nombres de
empresa son texto libre no normalizado en ambos lados (mismo tipo de limitacion ya documentada
para comparaciones de texto libre en sub-fases anteriores).

## Que se implemento

Migracion `CandidateReferenceToken`: agrega `TokenHash`/`TokenExpiresAt`/`SentAt` a
`PTCandidateReference` (no estaban en el esquema original de 3.1, necesarios para el flujo de
link publico). Mismo problema de `Guid.NewGuid()` en el seed de `SY_WizardSteps` ya documentado
repetidamente, limpiado de la migracion generada de la misma forma que las anteriores.

`IReferenceService`/`ReferenceService` (`OpenToWork.Core`):
- `AddReferenceAsync`, `GetReferencesAsync` (con deteccion de empresa duplicada y
  `HasMinimumReferences`).
- `SendReferenceRequestAsync(candidateId, referenceId)` - firma extendida respecto al plan
  literal (`SendReferenceRequestAsync(referenceId)`) para poder validar ownership dentro del
  servicio, ya que la ruta HTTP (`api/references/{id}/send`) no trae un candidateId propio con
  el que comparar en el controller.
- `SubmitReferenceFeedbackAsync(token, rating, feedback)` - valida vencimiento perezoso, guarda
  la respuesta, y dispara `VerifyReferenceAsync` automaticamente ("el sistema valida la
  respuesta", sin paso manual adicional).
- `VerifyReferenceAsync` - marca `Verified` y actualiza el `PT_Verifications` agregado del
  candidato (pregunta 5).

Endpoints:
- `GET/POST api/candidates/{id}/references` en `CandidatesController`, mismo guard de
  ownership que `/verifications/run` y `/score/recalculate`.
- `POST api/references/{id}/send` en el nuevo `ReferencesController`, autenticado, ownership
  validado dentro del servicio.
- `POST api/references/feedback` (**no** `{id}/feedback` como en el plan literal) - el token va
  en el **body**, no en la URL, para igualar el patron ya usado en `POST api/auth/reset-password`
  (`ResetPasswordDto.Token`) y evitar tokens sensibles en logs de acceso/Referer. `[AllowAnonymous]`,
  sin autenticacion.

## Verificacion

Contra MySQL real y la API corriendo, con el candidato de prueba (`donald@gmail.com`):

1. **Alta de 2 referencias, misma empresa**: la segunda referencia (`Acme Corp`, repetida) hizo
   que ambas quedaran con `SameCompanyAsAnotherReference=true`; `HasMinimumReferences=false`
   con solo 2.
2. **Flujo completo send → feedback publico → verificacion automatica**: `POST .../send`
   devolvio un link con token; `POST api/references/feedback` (sin autenticacion, token en el
   body) con `rating=5` devolvio `204` y la referencia paso a `Status=Verified(3)` con el
   rating/feedback guardados.
3. **EvidenceIndex actualizado**: tras verificar la referencia, `POST .../score/recalculate`
   dio `EvidenceIndex=60` (LinkedIn 20 + Portfolio 20 + Reference 20, con CvCoherence/Identity
   en 0 de pruebas previas) - coincide exactamente con la formula reescalada.
4. **Rechazo de reenvio**: volver a mandar `feedback` con el mismo token ya usado devolvio
   `400` (la referencia ya no esta en `Sent`).
5. **Vencimiento perezoso**: se genero un token para la segunda referencia, se adelanto
   `TokenExpiresAt` manualmente a "ayer", y el siguiente `GET references` la marco `Failed(4)`
   automaticamente, sin ningun job corriendo.
6. **Ownership guard**: `POST /api/references/{guid-inexistente}/send` con el token de Donald
   devolvio `404` (ni siquiera revela si el Id existe pero no es suyo).
7. Datos de prueba (2 referencias, la verificacion `Reference` agregada) borrados despues de
   verificar; `EvidenceIndex` volvio a 40 (solo LinkedIn+Portfolio, el estado previo a esta
   sub-fase).

`dotnet build` sin errores en `OpenToWork.API` (arrastra `Core`/`Models`).
