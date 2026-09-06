# Fase 3, Sub-fase 3.4: CompatibilityService — Job Match Score

**IA:** Dsiezar
**Fecha:** 2026-09-01
**Estado:** Completada (3 de 5 dimensiones - Idioma y Educacion quedan fuera, ver pregunta 5)

---

## Respuestas a las preguntas de la sub-fase 3.4

**1. ¿Los pesos son fijos o configurables por la empresa?**

`PTVacancy` no tiene un campo propio para que la empresa configure pesos (no esta en el
esquema de la sub-fase 3.1). Por ahora son defaults hardcodeados: **Skills 50% / Experiencia
30% / Ubicacion 20%** (solo 3 dimensiones reales, ver pregunta 5). El campo `WeightsConfig` de
`PTJobMatchScore` (creado en 3.1) se usa para **registrar** los pesos usados en cada calculo -
si una fila ya tiene un `WeightsConfig` seteado (a mano o por una futura feature de admin), se
respeta en cada recalculo siguiente en vez de sobrescribirlo con los defaults. Verificado en
pruebas: un `WeightsConfig` custom seteado directamente en la fila persistio correctamente a
traves de un recalculo posterior.

**2. ¿El matching de skills es binario o ponderado por `ProficiencyLevel`?**

Ponderado cuando ambos lados (`PTCandidateSkill.ProficiencyLevel` y
`PTVacancySkill.MinProficiencyLevel`) estan cargados; si falta cualquiera de los dos, cae a
binario (tiene/no tiene). Las skills `IsRequired=true` pesan el doble que las opcionales en la
suma.

**3. ¿Si la vacante requiere 5 anos y el candidato tiene 3, es 60%, 0%, o hay curva?**

Curva proporcional bajo el minimo, tal como sugiere el ejemplo de la propia pregunta:
`candidateYears / minimoDelBucket * 100`. `PTVacancy.ExperienceLevel` es un bucket
(`Entry/Junior/Mid/Senior/Lead`), no anos literales, asi que se mapea a un minimo de anos:
Entry=0, Junior=1, Mid=3, Senior=5, Lead=8. Si el candidato iguala o supera el minimo, 100%.
Sin requisito en la vacante (`ExperienceLevel == null`), 100% neutral.

**4. ¿La ubicacion geografica como se compara?**

Si `WorkMode = Remote`, 100% siempre (la ubicacion no importa). Si es Hibrido/Presencial, se
compara texto: 100% si `City` o `Country` del candidato aparece como substring (case-
insensitive) en el campo libre `PTVacancy.Location`, 0% si no hay coincidencia, 100% neutral si
la vacante no tiene `Location` cargado. Es aproximado porque `Location` es texto libre, no hay
campos estructurados de pais/ciudad en la vacante.

**5. ¿El nivel de ingles se valida contra un campo del candidato o se infiere de experiencias?**

**Ninguno de los dos - no se implementa.** `PTCandidate` no tiene ningun campo de nivel de
idioma (solo `PTVacancy.EnglishLevel` existe del lado de la demanda). Inferirlo de las
experiencias seria una heuristica de texto sin base confiable, mismo criterio ya aplicado a la
"progresion logica" en `fase-3-sub3.md` pregunta 5. Por el mismo tipo de limitacion, tampoco se
implementa **Educacion** como dimension: `PTVacancy.Requirements` es texto libre sin un campo
estructurado de titulo/carrera requerida para comparar contra `PTCandidateEducation`
(`PTJobMatchScore.EducationMatch` queda siempre en 0, reservado para cuando exista ese campo).
Los pesos de la pregunta 1 quedan redistribuidos entre las 3 dimensiones que si tienen datos
estructurados confiables: Skills/Experiencia/Ubicacion.

**6. ¿El shortlist se genera automaticamente al crear la vacante, o lo dispara el admin/TD?**

Lo dispara el admin/TD manualmente via `POST matches/calculate` - generarlo automaticamente al
crear la vacante implicaria recorrer todos los candidatos elegibles en cada creacion, un costo
innecesario si la vacante todavia no esta lista para recibir candidatos. `GenerateShortlist` es
solo lectura/ranking sobre los `PTJobMatchScore` ya calculados y persistidos.

**7. ¿Cuantos candidatos aparecen en el shortlist por defecto? ¿Es configurable?**

20 por defecto (mismo default que `AdminVacancyService.GetVacanciesAsync`), configurable via
`?limit=N` tal como especifica el propio endpoint del plan.

**8. ¿El `MatchPercentage` se recalcula si el candidato actualiza su perfil despues del match?**

No automaticamente. Un Job Match es por par candidato-vacante: recalcular en cada edicion de
perfil implicaria recorrer todas las vacantes activas en cada guardado (a diferencia del
Candidate Score de la sub-fase 3.3, que es 1:1 con el candidato). Se recalcula solo bajo
demanda via `POST matches/calculate` - mismo criterio de "sin job programado" ya usado en 3.2
y 3.3 (no hay Hangfire/Quartz instalado).

**9. ¿Se necesita un endpoint para que TD apruebe/rechace matches antes de llegar a la empresa?**

**Fuera de alcance de esta sub-fase.** Requeriria un campo de estado de aprobacion en
`PTJobMatchScore` que no esta en el esquema de la sub-fase 3.1 (solo tiene
`MatchPercentage`/`SkillsMatch`/`ExperienceMatch`/`EducationMatch`/`CalculatedAt`/
`WeightsConfig`). Se documenta como diferido a una sub-fase futura (probablemente 3.8, cuando
se construya la UI de Admin) en vez de improvisar un campo no pedido en el plan.

**10. ¿La empresa puede ver el desglose del match o solo el porcentaje total?**

El desglose - `GET matches` devuelve `SkillsMatch`/`ExperienceMatch`/`LocationMatch` (mas
`EducationMatch=0`, reservado) ademas del `MatchPercentage` total. La UI para mostrarlo en el
portal de empresa es sub-fase 3.8.

## Que se implemento

`ICompatibilityService`/`CompatibilityService` (`OpenToWork.Core`), sin dependencias HTTP:

- `CalculateJobMatch(candidateId, vacancyId)` - calcula y persiste (upsert) un
  `PTJobMatchScore`.
- `CalculateMatchesForVacancyAsync(vacancyId)` - corre `CalculateJobMatch` contra todos los
  candidatos elegibles (`IsProfilePublic && WizardCompleted`, mismo criterio que `AlertService`
  para candidatos visibles).
- `GenerateShortlist(vacancyId, limit)` - lee `PT_JobMatchScores` ordenado por
  `MatchPercentage` descendente, `limit` default 20.

Se agrego una migracion (`JobMatchLocationMatch`) que suma la columna `LocationMatch` a
`PTJobMatchScore` (el esquema de 3.1 no la incluia porque Ubicacion no estaba resuelta como
dimension todavia) - mismo problema de `Guid.NewGuid()` en el seed de `SY_WizardSteps` ya
documentado en `fase-3-sub1.md`, limpiado de la migracion generada de la misma forma.

Endpoints en **`OpenToWork.AdminAPI`** (no en `OpenToWork.API`), en el `VacanciesController`
existente bajo `api/admin/vacancies`, protegidos con `[Authorize(Roles="Admin")]` (mismo patron
que el resto del Admin API) - se decidio esta ubicacion porque el disparo es admin/TD (pregunta
6) y no hay autenticacion de empresa propia en ningun API todavia:
- `POST api/admin/vacancies/{id}/matches/calculate`
- `GET api/admin/vacancies/{id}/matches?limit=N`

## Verificacion

Contra MySQL real y la AdminAPI corriendo (login con `admin@opentowork.com`), vacante real
"Desarrollador Backend Senior" y 2 candidatos reales (`donald@gmail.com`, `Juan Perez`):

1. **Baseline** (sin requisitos: sin skills demandados, sin `ExperienceLevel`, `WorkMode=
   Remote`): ambos candidatos dieron `MatchPercentage=100` (100% en las 3 dimensiones, nada que
   fallar).
2. **Con requisitos reales**: se seteo `ExperienceLevel=Senior` (minimo 5 anos), `WorkMode=
   OnSite`, `Location='Buenos Aires, Argentina'`; Donald con 3 anos + `City=Cordoba,
   Country=Argentina` (coincide por pais) dio `Skills=100, Experience=60, Location=100 →
   MatchPercentage=88` (100*.5+60*.3+100*.2=88, exacto); Juan Perez sin anos de experiencia ni
   ciudad/pais cargados dio `Experience=0, Location=0 → MatchPercentage=50` (100*.5+0+0=50,
   exacto).
3. **Shortlist rankeado**: la lista devolvio a Donald (88) antes que a Juan (50), orden
   descendente confirmado; `?limit=1` devolvio solo a Donald.
4. **Persistencia de `WeightsConfig` custom**: se seteo manualmente `{"skills":0.1,
   "experience":0.8,"location":0.1}` en la fila de Donald y se volvio a calcular - el resultado
   fue `MatchPercentage=68` (100*.1+60*.8+100*.1=68, exacto), confirmando que un peso
   personalizado ya guardado sobrevive a un recalculo en vez de perderse con los defaults.
5. Datos de prueba (campos de vacante/candidato modificados, filas de `PT_JobMatchScores`)
   revertidos/borrados despues de verificar.

`dotnet build` sin errores en `OpenToWork.API` y `OpenToWork.AdminAPI` (ambos arrastran
`Core`/`Models`).
