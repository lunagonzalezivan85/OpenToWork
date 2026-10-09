# Revisión de `iluna-sec-rgpd` (auditoría 8-Oct)

**Revisa:** Dsiezar · **Fecha:** 2026-10-09 · **Rama revisada:** `iluna-sec-rgpd` (74756f7, incluye `iluna-sec-sesiones`) · **Arreglos:** rama `dsiezar-fix-iluna-sec`, encima de la tuya.

Buen trabajo en general: anti-enumeración con aviso al titular, verificación de correo también para empresas, refresh token en cookie `HttpOnly` con rotación y `revoke-all`, guard de rutas por rol y `/logout` real, búsqueda solo para empresas verificadas con consentimiento y apellido enmascarado, `Apply` solo a vacantes activas y el NU1903 resuelto. Compila y pasan las 122 pruebas unitarias.

## Bloqueantes (ya corregidos en `dsiezar-fix-iluna-sec`)

### 1. Las migraciones borraban en producción los documentos de selección — `9b5c05d`

`VacancyReferenceCode`, `CandidateVisibilityConsent` y `CandidateRequest` llevaban el ruido de los seeds (`DeleteData`/`InsertData`/`UpdateData`) y, en `VacancyReferenceCode`, `DELETE FROM SY_DocumentTypes` y `DELETE FROM SY_WizardSteps`.

- `SY_DocumentTypes` → `PT_RecruitmentDocuments` es **ON DELETE CASCADE**: el `DELETE` borraba todos los documentos de selección. En producción la migración corre sola al desplegar.
- Los `UpdateData` de `PT_Plans` pisaban los planes configurados desde el admin.
- El motivo del `DELETE` (los seeds regeneran GUIDs y la reinserción chocaba con el índice único por nombre) desaparece al no reinsertar nada.

Ahora solo quedan columnas, tablas, índices y el backfill de `ReferenceCode`. Probado en local: se aplican las 4 migraciones y no cambia el número de tipos de documento (10), documentos (4) ni planes (6).

**Para la próxima migración:** recortar siempre el ruido de seeds (lo provoca `Guid.NewGuid()` en `HasData`; por eso `has-pending-model-changes` nunca queda limpio).

### 2. La casilla de visibilidad salía premarcada y el consentimiento se registraba solo — `356e22a`

`IsProfilePublic` vale `true` por defecto (también `HasDefaultValue(true)` en la BD) y el botón **Guardar** del perfil lo enviaba siempre. Cualquier candidato que editara su teléfono quedaba registrado con `VisibilityConsentAt` sin haber marcado nada: una casilla premarcada no es consentimiento válido (RGPD art. 4.11 y 7; caso Planet49).

- Nueva `PTCandidate.IsVisibleToCompanies` = `IsProfilePublic` + consentimiento no retirado (misma condición que ya usabas en la búsqueda y los matches).
- El perfil (portal y admin) muestra esa visibilidad real: sin consentimiento la casilla sale desmarcada.
- Guardar compara con la visibilidad real: solo marcar registra el consentimiento y solo desmarcar algo consentido registra la retirada.
- No cambié el default de la columna: con `HasDefaultValue(true)` en EF, poner `false` en C# no tiene efecto sin migración.

Probado contra la API: guardar sin tocar → sin consentimiento; marcar → registrado (`rgpd-v1`); desmarcar → retirado.

## Pendiente (no bloquea, para ti)

1. **Score y métricas en la búsqueda.** `CandidateSearchResultDto` sigue enviando `OverallScore` y los cuatro índices a las empresas. El informe (BE-19 paso 6, H-27) pide no mostrarlos hasta tener la EIPD y la base legal; el texto del consentimiento ya menciona la "puntuación". Decisión de Darwin + asesoría.
2. **Redirección abierta (`/\`).** Corregida en `Login` y `VerifyEmail`; falta en `Register.razor` (línea ~247) y `GoogleAuth.razor` (`SafeReturnUrl`).
3. **Refresh token también en el cuerpo.** El login y el refresh lo siguen devolviendo en el JSON además de en la cookie; el portal ya no lo guarda, pero conviene vaciarlo en la respuesta cuando va en cookie.
4. **`/cookies` pública.** El mapa de `/vacancies` envía IP (y búsquedas) a OpenFreeMap y Nominatim; está en `docs/COOKIES_Y_PRIVACIDAD.md` pero la página sigue diciendo que el navegador no descarga nada de otros sitios.
5. **Completitud del perfil.** `AlertService` sigue contando `IsProfilePublic` como campo completado (línea ~103): conviene no ligar el % de perfil al consentimiento (presión para aceptar) y, si se mantiene, usar la visibilidad real.
6. **Pequeños:** `IAuthService.RevokeAllTokensAsync` sin sangría; BOM añadido a `es/profile.json`; captcha programado pero apagado (`Recaptcha:Enforced=false`) hasta configurar el widget.
7. **Pruebas de integración:** no las ejecuté (46); con las migraciones ya recortadas se pueden lanzar en local sin riesgo.
