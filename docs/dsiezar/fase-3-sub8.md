# Fase 3, Sub-fase 3.8: UI — Integracion en los 3 portales

**IA:** Dsiezar
**Fecha:** 2026-09-01
**Estado:** Completada - la sub-fase mas grande de las 8, cierra Fase 3 completa.

---

## Respuestas a las preguntas de la sub-fase 3.8

**1. ¿Los graficos de indices son SVG circulares o barras horizontales?**

Ninguno de los dos literalmente: se reutiliza el mismo patron ya usado en el donut de
completitud de perfil de `Dashboard.razor` (`.dash-chart-ring`, un `div` con
`background: conic-gradient(...)` controlado por variables CSS `--deg`/`--progress`, no un
`<svg>`). Se crearon variantes mas chicas (`.dash-score-mini-ring`, 64px) para los 4 indices +
una version grande (`.dash-score-ring--overall`, 110px) para el `OverallScore` - mismo lenguaje
visual que "el donut existente" al que se refiere la pregunta, sin introducir una libreria SVG
nueva.

**2. ¿El candidato puede ver el desglose de que penalizo su `StabilityIndex`?**

El backend no persiste "razones" por indice - solo los 4 numeros finales
(`PTCandidateScore`). Lo que si existe es el array `issues` que ya guarda
`ValidationService.VerifyCvCoherenceAsync` en el JSON `Result` de la verificacion CvCoherence
(ej. "Gap de 15 mes(es)...", "Salto laboral..."). La seccion Verificaciones del dashboard
muestra esa lista de verificaciones con su estado; el desglose textual real disponible es el de
CvCoherence, no un mecanismo separado por cada uno de los 4 indices que no existe en el
esquema.

**3. ¿La cola de shortlist del admin tiene workflow de aprobacion?**

No en este MVP. `PTJobMatchScore` no tiene un campo de estado de aprobacion (diferido
explicitamente en `fase-3-sub4.md` pregunta 9). La cola en `Vacancies.razor` (admin) es una
vista de solo lectura/ranking con boton "Calcular matches" - no hay
pendiente→aprobado→enviado.

**4. ¿El scorecard configurable de la empresa es sliders o inputs numericos?**

Inputs numericos (0-100, interpretados como %) - mas simple de construir/validar que sumen
100 que un componente de sliders, consistente con que `WeightsConfig` ya se guarda como JSON de
numeros simples.

**5. ¿El banco de retos del admin tiene preview del reto antes de publicarlo?**

Si - un boton "Vista previa" en `SkillTests.razor` (admin) muestra el titulo, categoria,
tiempo y cada pregunta con sus opciones, resaltando en verde la opcion marcada como correcta -
sin guardar el reto todavia. No es una vista previa interactiva tipo "tomar el examen", solo
revision visual del contenido cargado.

## Que se implemento

### Backend previo necesario (antes de la UI)

Dos endpoints de **solo lectura** que faltaban (todo lo anterior en Fase 3 solo tenia
recalculo/ejecucion, que hubieran sido muy costosos de disparar en cada carga de pantalla):
- `IScoringService.GetScoreAsync` + `GET api/candidates/{id}/score` - lee el ultimo score
  persistido sin recalcular (todo en 0 si nunca se calculo, sin crear una fila).
- `IValidationService.GetVerificationsAsync` + `GET api/candidates/{id}/verifications` - lee
  `PT_Verifications` sin disparar HTTP.

**Verificaciones manuales** (item de Fase 4 desbloqueado desde `fase-3-sub1.md`):
`IValidationService.SetVerificationStatusAsync` - override de admin (aprobar=Verified,
rechazar=Failed), expuesto en `OpenToWork.AdminAPI` via `PUT
api/admin/candidates/{candidateId}/verifications/{type}` + `GET/POST
.../score[/recalculate]`. Se agrego `AdminUserProfileDto.CandidateId` (no existia - la ruta
admin solo tenia el `SCUserId`) para poder llamar estos endpoints desde `CandidateProfile.razor`.

**Scorecard de empresa** - correccion real de una limitacion documentada en `fase-3-sub4.md`
pregunta 1: `PTVacancy` no tenia ningun campo propio donde la empresa pudiera configurar pesos
(el `WeightsConfig` de esa sub-fase vivia solo en `PTJobMatchScore`, el resultado de un calculo
ya hecho). Se agrego una migracion (`VacancyWeightsConfig`) con `PTVacancy.WeightsConfig`, y
`CompatibilityService.ParseWeights` ahora prioriza ese campo sobre el de un match previo.
Nuevos endpoints en `PermanentVacanciesController` (`OpenToWork.API`, no Admin - la empresa usa
el API principal): `GET {id}/matches` (shortlist, solo lectura, la empresa no dispara el
calculo - eso sigue siendo admin/TD), `GET/PUT {id}/scorecard`, todos con verificacion de
ownership (`PT_CompanyId` del caller debe ser dueno de la vacante).

### Portal del Candidato (`OpenToWork.WEB`)

- **`Dashboard.razor`**: nueva seccion "Tu Evaluacion" con anillo grande de `OverallScore` +
  badge "★ Verificado TD" (o el estado textual actual) + boton "Recalcular", 4 anillos chicos
  (Estabilidad/Confiabilidad/Evidencia/Compatibilidad), seccion "Verificaciones" (lista con
  pill de estado + boton "Ejecutar verificaciones"), y 2 quick-action cards nuevas
  ("Mis Referencias", "Retos Tecnicos").
- **`References.razor`** (nueva pagina, `/references`): formulario de alta + lista con estado,
  aviso de empresa duplicada, boton "Enviar solicitud" que muestra el link publico a compartir
  (copiar al portapapeles), rating/feedback visible una vez verificada.
- **`SkillTests.razor`** (nueva pagina, `/skill-tests`): lista de retos disponibles por
  categoria (visible siempre) + boton "Comenzar" (bloqueado si el wizard no esta completo);
  modo de examen con timer real (`PeriodicTimer`, cuenta regresiva en vivo, auto-envia al
  llegar a 0), deteccion de cambio de pestana via un pequeno script inline
  (`document.visibilitychange`) leido al enviar; pantalla de resultado; historial de resultados
  propios.
- `ApiAuthService.cs`: ~15 metodos cliente nuevos para todo lo anterior.

**Bug real encontrado y corregido durante la verificacion en navegador**: `References.razor` y
`SkillTests.razor` redirigian a `/dashboard` si `GetCandidateProfileAsync()` devolvia null - pero
durante el **prerender estatico** de Blazor Server (antes de que el circuito interactivo
arranque) no hay JS/localStorage disponible todavia, asi que el token nunca esta y esto
**siempre** da null en esa primera pasada. Un `NavigateTo` ahi dispara una redireccion HTTP real
antes de que la segunda pasada (interactiva, con datos reales) pueda ejecutarse - toda
navegacion directa a esas rutas rebotaba a `/dashboard`. Se corrigio no redirigiendo en null
(mismo criterio tolerante que ya usaba `Dashboard.razor`, que nunca tuvo este problema
justamente por eso).

### Portal Admin (`OpenToWork.AdminWEB`)

- **`Candidates/CandidateProfile.razor`**: nueva seccion "Score del Candidato" (5 valores +
  boton Recalcular) y "Verificaciones" (lista con botones Aprobar/Rechazar por verificacion
  existente - "verificaciones manuales").
- **`SkillTests.razor`** (nueva pagina, `/skill-tests`): CRUD completo de `PTSkillTest`
  (categoria, dificultad, tiempo, preguntas dinamicas con N opciones y radio de respuesta
  correcta) + Vista previa (pregunta 5) + tabla con Editar/Eliminar.
- **`Vacancies.razor`**: boton "Cola de Shortlist" por vacante activa, expande una fila con
  boton "Calcular matches" + tabla rankeada (candidato, match%, desglose skills/exp/ubicacion) -
  vista simple sin workflow (pregunta 3).
- `AdminAuthApiService.cs`: ~9 metodos cliente nuevos.
- Se agrego la clase CSS `.admin-result-badge--danger` que faltaba (ya se usaba en el archivo
  para el badge de "Descartado" sin tener una regla definida - se corrigio de paso).

### Portal de Empresa (dentro de `OpenToWork.WEB`)

- **`VacancyManage.razor`** (pantalla de gestion de una vacante propia de la empresa): nueva
  seccion "Ranking por Compatibilidad (Match Score)" con la lista rankeada (solo lectura) y
  boton "Configurar Scorecard" que abre un formulario de 3 inputs numericos
  (Skills/Experiencia/Ubicacion) para ajustar los pesos de esa vacante puntual.

## Verificacion

Contra MySQL real y los 3 portales corriendo en simultaneo (candidato `donald@gmail.com`, admin
`admin@opentowork.com`, empresa `testcompany@opentowork.com`), navegado en el browser real
(no solo curl):

1. **Candidato - Dashboard**: los 4 anillos + `OverallScore` + estado "Evaluado" se renderizaron
   con los valores reales de MySQL (`48`, `0/100/50/50`); la lista de Verificaciones mostro
   Identidad Pendiente / LinkedIn, Portafolio, CvCoherence Verificadas.
2. **Candidato - Referencias**: se agrego una referencia real via el formulario, se genero y
   copio el link de "Enviar solicitud", se vio el aviso de empresa duplicada al agregar una
   segunda con el mismo `CompanyName`.
3. **Candidato - Retos Tecnicos**: se creo un reto real desde el admin, se tomo desde el
   candidato con el timer corriendo en vivo (60s → 40s entre pasos), se enviaron las respuestas
   correctas y se obtuvo `100%` - confirmado en "Mis resultados". Se encontro y corrigio el bug
   de redireccion en prerender durante esta prueba.
4. **Admin - Score y Verificaciones manuales**: se vio el score real de Donald (52/0/100/70/50 en
   ese momento del testing) y se aprobo manualmente la verificacion de Identidad (Pendiente →
   Verificada) - revertido despues para no dejar un dato fabricado.
5. **Admin - Banco de Retos**: se creo un reto completo desde cero via la UI (categoria, titulo,
   pregunta con 2 opciones, radio de respuesta correcta), se uso "Vista previa" (confirmado el
   resaltado verde de la opcion correcta), se guardo y aparecio en la tabla.
6. **Admin - Cola de Shortlist**: se expandio la fila de la vacante activa, se calcularon
   matches ("2 candidatos evaluados") y se vio la tabla rankeada con Donald y Juan Perez al
   100%.
7. **Empresa - Match Score y Scorecard**: la pantalla de gestion de la vacante mostro el mismo
   ranking calculado por el admin; se abrio "Configurar Scorecard", se cambio el peso de Skills
   a 70 y se guardo - confirmado en la base de datos que `PT_Vacancies.WeightsConfig` se
   actualizo correctamente (`{"skills":0.7,...}`).
8. Datos de prueba (reto de skill test, referencia, override manual de Identidad, WeightsConfig
   custom de la vacante) limpiados/revertidos despues de cada verificacion.

`dotnet build` sin errores en los 5 proyectos host (`OpenToWork.API`, `OpenToWork.AdminAPI`,
`OpenToWork.WEB`, `OpenToWork.AdminWEB`, mas `OpenToWork.Models`/`Core`/`Shared` que arrastran).

---

## Cierre de Fase 3

Con esta sub-fase se completan las 8 sub-fases del plan obligatorio de Iluna (3.1 → 3.8): 6
entidades nuevas + 2 columnas agregadas sobre la marcha (`PTJobMatchScore.LocationMatch`,
`PTVacancy.WeightsConfig`) + 3 campos de tracking (`PTCandidateReference.Token*`,
`PTCandidateTestResult.StartedAt/Answers`), 7 servicios nuevos en `OpenToWork.Core`
(`ValidationService`, `ScoringService`, `CompatibilityService`, `ReferenceService`,
`SkillTestService`, `VerificationStatusService`, mas los overrides de admin), y UI real
funcionando en los 3 portales. El Motor de Evaluacion y Scoring Automatico del checklist
original de Fase 3 queda implementado de punta a punta.
