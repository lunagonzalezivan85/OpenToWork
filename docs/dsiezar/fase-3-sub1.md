# Fase 3, Sub-fase 3.1: Entidades de Scoring + Migracion

**IA:** Dsiezar
**Fecha:** 2026-09-01
**Estado:** Completada

---

## Contexto

Sigue el plan obligatorio para Fase 3 documentado en `README.md` (commit `a340397` de Iluna,
"plan obligatorio Fase 3 — Motor de Scoring Automatico con 8 sub-fases y preguntas por
sub-fase para Darwin"). Antes de esta sub-fase existia un MVP propio (rama
`dsiezar-fase-3-mvp-descartado`, sin mergear) que no seguia ni el esquema (`SCUserId` en vez
de `PT_CandidateId`, sin `PTJobMatchScore`/`PTSkillTest`) ni el proceso de preguntas del plan.
A pedido de Darwin, se descarto ese trabajo y se reinicia Fase 3 desde el `main` actualizado,
siguiendo el plan al pie de la letra.

## Respuestas a las preguntas de la sub-fase 3.1

**1. ¿El `OverallScore` se almacena como un campo calculado en la tabla, o se calcula
on-the-fly cada vez que se consulta?**

Se almacena (snapshot persistido en `PT_CandidateScores.OverallScore`). Evita recalcular en
cada lectura del dashboard/admin, y `CalculatedAt`/`Version` solo tienen sentido si el valor
es un snapshot congelado en el tiempo, no algo derivado on-the-fly.

**2. ¿Que estrategia se usa para el versionado (`Version`)?**

Incremental: se suma 1 en cada recalculo (`ScoringService.RecalculateAsync`), no timestamp.
Mas simple de comparar/ordenar y evita ambiguedad si dos recalculos caen en el mismo
milisegundo.

**3. ¿`PTJobMatchScore.WeightsConfig` que formato JSON debe tener?**

```json
{ "skills": 40, "experience": 30, "education": 20, "location": 10 }
```

Enteros que representan porcentaje, deben sumar 100. Se valida en el servicio antes de
persistir (fuera de alcance de esta sub-fase - se implementa en 3.4, `CompatibilityService`).

**4. ¿Las verificaciones (`PTVerification`) se insertan automaticamente al crear un candidato,
o se disparan bajo demanda?**

Bajo demanda: se crea una fila de `PT_Verifications` (una por `Type`) solo cuando esa
verificacion especifica corre por primera vez. Pre-crear las 6 filas vacias por cada
candidato registrado generaria ruido para perfiles que nunca se completan.

**5. ¿`PTCandidateReference` tiene soft delete o se elimina fisicamente?**

Soft delete. Es la convencion obligatoria del proyecto para todas las tablas (`BaseEntity`
con `IsDeleted`/`DeletedAt`/`DeletedBy`, ver seccion "Convenciones" del README) - no hay
motivo para que esta entidad sea la excepcion.

**6. ¿`PTSkillTest.Questions` que estructura JSON debe tener? ¿Multiple choice, codigo, o
ambos?**

Solo multiple choice en esta primera version:

```json
[{ "question": "...", "options": ["...", "..."], "correctIndex": 0 }]
```

Retos de codigo ejecutable requieren un judge/sandbox externo (Judge0, Piston, o propio) -
alcance mucho mayor que se deja fuera de esta sub-fase (revisar en sub-fase 3.6).

**7. ¿Se necesita una entidad `PTScoreWeight` configurable por el admin, o los pesos van
hardcodeados en el `ScoringService`?**

Hardcodeados en `ScoringService` por ahora. El `WeightsConfig` de la pregunta 3 ya cubre lo
configurable para el Job Match Score (por vacante, por la empresa); una entidad separada para
los pesos globales del Candidate Score (Estabilidad/Confiabilidad/Evidencia/Compatibilidad)
queda como mejora futura si se necesita ajustarlos sin redeploy.

## Entidades creadas

Todas heredan `BaseEntity`, FK a `PT_CandidateId` (no `SCUserId`) siguiendo el mismo patron
que `PTCandidateExperience`/`PTCandidateEducation`/`PTCandidateCertification`.

| Entidad | Tabla | Descripcion |
|---|---|---|
| `PTCandidateScore` | `PT_CandidateScores` | Score intrinseco (1:1 con candidato, indice unico) |
| `PTJobMatchScore` | `PT_JobMatchScores` | Score por par candidato-vacante (indice unico compuesto) |
| `PTVerification` | `PT_Verifications` | Una fila por `Type` de verificacion (indice unico `PT_CandidateId+Type`) |
| `PTCandidateReference` | `PT_CandidateReferences` | Referencias que aporta el candidato (distinta de `PTReferenceCheck` de Iluna) |
| `PTSkillTest` | `PT_SkillTests` | Banco de retos (sin FK a candidato - es catalogo) |
| `PTCandidateTestResult` | `PT_CandidateTestResults` | Resultado de un candidato en un `PTSkillTest` |

5 enums nuevos en `OpenToWork.Shared.Enums`: `VerificationType`, `VerificationCheckStatus`,
`ReferenceRelationship`, `ReferenceStatus`, `SkillTestDifficulty`.

## Migracion

`ScoringEngine` (`20260901144449_ScoringEngine`), aplicada contra MySQL real. Se repitio la
misma limpieza que en el intento anterior: el scaffold automatico intento borrar+reinsertar
las 10 filas de `SY_WizardSteps` con GUIDs nuevos (bug preexistente del proyecto - `Guid.NewGuid()`
no determinista en `SeedWizardSteps`, ver nota en Sub-fase 3.1 del MVP descartado). Se quito ese
ruido de la migracion antes de aplicarla.

**Nota operativa:** la base de datos local ya tenia aplicada la migracion `ScoringEngine` del
MVP descartado (mismo nombre, esquema distinto). Se elimino manualmente (`DROP TABLE` de las 3
tablas viejas + borrar la fila de `__EFMigrationsHistory`) antes de aplicar la nueva version,
ya que esa rama nunca se mergeo a `main` ni se publico.

**Verificado:** `SHOW TABLES` confirma las 6 tablas nuevas creadas correctamente;
`SY_WizardSteps` sigue con sus 10 filas originales sin tocar.

## Build

`dotnet build` sin errores en `OpenToWork.Models` y `OpenToWork.API` (que arrastra `Core` y
`Models`) tras registrar las entidades en `AppDbContext`.
