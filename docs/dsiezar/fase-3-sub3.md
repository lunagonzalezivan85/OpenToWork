# Fase 3, Sub-fase 3.3: ScoringService — Indices Automaticos

**IA:** Dsiezar
**Fecha:** 2026-09-01
**Estado:** Completada

---

## Respuestas a las preguntas de la sub-fase 3.3

**1. ¿Que pesos tiene cada indice en el `OverallScore`?**

Estabilidad 30% / Confiabilidad 25% / Evidencia 25% / Compatibilidad 20% - los mismos pesos que
trae de ejemplo la propia pregunta del plan de Iluna. Compatibilidad pesa menos porque puede no
tener datos reales todavia si no hay vacantes activas con skills demandados en el sistema.

**2. ¿La duracion promedio en empleos como se pondera?**

Lineal con techo: 0 a 60 meses (5 anos) escala 0→100, 60+ meses = 100.

**3. ¿Cuantos cambios de empleo por ano se consideran "frecuentes"? ¿Como escala la penalizacion?**

Mas de 1 cambio por ano (`cantidadExperiencias / anosDeSpan`). Penalizacion:
`min(30, (cambiosPorAño - 1) * 15)`.

**4. ¿Un gap de 6 meses se penaliza igual que uno de 2 anos?**

No, es proporcional - se reutiliza exactamente la misma formula ya construida en
`ValidationService.VerifyCvCoherenceAsync` (sub-fase 3.2, pregunta 3): `min(30, 10 +
mesesDeGap/2) * (esReciente ? 1.5 : 1.0)`, donde "reciente" es que el gap haya terminado hace
menos de 2 anos. Se reutiliza la misma formula para no tener dos criterios distintos de
"coherencia cronologica" conviviendo en el sistema.

**5. ¿La "progresion logica" como se detecta automaticamente?**

**No se implementa.** Detectar ascensos via texto libre en `JobTitle` (buscar palabras como
"senior"/"lead"/"junior") daria falsos positivos/negativos arbitrarios sin datos estructurados
de seniority o industria - mismo tipo de limitacion ya documentada en `fase-3-sub2.md` pregunta
9 (cambios de sector). El bonus se omite del `ReliabilityIndex`, documentado como limitacion
explicita en vez de simular una deteccion poco confiable.

**6. ¿El `CompatibilityIndex` se calcula contra todas las vacantes activas, o solo las de la
industria del candidato?**

Contra todas las vacantes activas (`Status = Active`) - no existe un campo de industria en
`PTCandidate` ni `PTVacancy` para poder filtrar (mismo gap de dato ya documentado en 3.2).

**7. ¿Si no hay vacantes activas, el `CompatibilityIndex` es 0, 50, o se omite?**

50 (neutral) - tambien aplica si hay vacantes activas pero ninguna tiene skills demandados
cargados (`PTVacancySkill`), verificado en pruebas reales (1 vacante activa sin skills → 50).
No castiga al candidato por un problema de oferta/datos incompletos del sistema, no de su
perfil.

**8. ¿El recalculo en lote (`RecalculateAllAsync`) via job programado o manual?**

Manual/bajo demanda - no hay Hangfire/Quartz instalado (mismo gap ya documentado en
`fase-3-sub2.md` pregunta 6). El metodo existe y queda listo para que un futuro job lo invoque
sin cambios; no se expone un endpoint HTTP para el en esta sub-fase (el plan de 3.3 solo pide
el endpoint singular `POST score/recalculate`).

**9. ¿Cada cuanto se recalcula el score automaticamente?**

Se dispara automaticamente en cada edicion de perfil/experiencia/educacion/CV: `ProfileService`
llama a `IScoringService.RecalculateAsync` al final de `UpdateProfileAsync`,
`AddExperienceAsync`, `UpdateExperienceAsync`, `DeleteExperienceAsync`, `AddEducationAsync`,
`UpdateEducationAsync`, `DeleteEducationAsync` y `ApplyCvDataAsync`. No hay cron
diario/semanal/mensual (misma razon de infraestructura que la pregunta 8). Esto es seguro
porque los 4 `Calculate*Index` son lecturas puras de base de datos, sin llamadas HTTP salientes
(esas viven solo en `ValidationService`, bajo demanda) - confirmado en las pruebas: agregar una
experiencia incremento `Version` automaticamente sin llamar al endpoint de recalculo.

**10. ¿El score anterior se guarda para comparar, o se sobrescribe?**

Se sobrescribe (upsert en `PTCandidateScore`, mismo patron que `PTVerification` en 3.2).
`Version` se incrementa en cada recalculo como contador, sin una tabla de historico separada
(no esta en el esquema de la sub-fase 3.1; agregarla seria un cambio de alcance no pedido, fast
follow posible).

**11. ¿El candidato puede ver el desglose de cada indice, o solo el `OverallScore`?**

Puede ver el desglose - el endpoint `POST .../score/recalculate` devuelve los 4 indices
individuales (`StabilityIndex`, `ReliabilityIndex`, `EvidenceIndex`, `CompatibilityIndex`) mas
el `OverallScore`, no solo el total. La UI para mostrarlo es sub-fase 3.8.

**12. ¿Que pasa si un candidato no tiene experiencias cargadas?**

`StabilityIndex = 0` - no hay evidencia de estabilidad laboral que evaluar, no es un caso
neutral. Para `ReliabilityIndex` el criterio es distinto: con 0 o 1 experiencia no hay
cronologia entre dos puntos que pueda ser incoherente, asi que vale 100 por definicion (la
propia pregunta 4 del plan dice "sin gaps ni superposiciones = 100", y eso se cumple
trivialmente sin al menos 2 experiencias).

## Que se implemento

`IScoringService`/`ScoringService` (`OpenToWork.Core`), sin dependencias HTTP:

- `CalculateStabilityIndex(candidateId)`, `CalculateReliabilityIndex(candidateId)` - separados
  para no penalizar dos veces el mismo problema: Estabilidad cubre duracion/frecuencia/saltos
  laborales/bonus de antiguedad; Confiabilidad cubre solo gaps y solapamientos en la
  cronologia (sin el bonus de progresion logica, pregunta 5).
- `CalculateEvidenceIndex(candidateId)` - lee los `PT_Verifications` ya persistidos por
  `ValidationService` (LinkedIn/Portfolio/CvCoherence/Identity, +25 cada uno si `Verified`) sin
  volver a disparar las llamadas HTTP - esas siguen viviendo solo en `POST verifications/run`.
- `CalculateCompatibilityIndex(candidateId)` - matching de `PT_CandidateSkills` vs
  `PT_VacancySkills` de vacantes activas, con penalizacion (hasta 20 pts) por skills del
  candidato que ninguna vacante demanda.
- `CalculateOverallScore(...)` - promedio ponderado 30/25/25/20.
- `RecalculateAsync(candidateId)` - orquesta los 4 calculos + upsert en `PTCandidateScore`.
- `RecalculateAllAsync()` - recalculo en lote, manual/bajo demanda (pregunta 8).

Endpoint: `POST /api/candidates/{id}/score/recalculate` en `CandidatesController`, mismo guard
de ownership que `/verifications/run` (3.2).

`ProfileService` ahora inyecta `IScoringService` y llama `RecalculateAsync` al final de cada
metodo que cambia datos relevantes para el score (perfil, experiencias, educaciones, CV
parseado) - confirmado en pruebas reales que dispara automaticamente sin intervencion manual.

## Verificacion

Contra MySQL real y la API corriendo, con el candidato de prueba (`donald@gmail.com`,
`PTCandidate.Id = 906e514d-adec-4b8a-b408-83ccd2c78f41`):

1. **Baseline sin experiencias**: `Stability=0`, `Reliability=100`, `Evidence=50` (LinkedIn +
   Portfolio verificados de pruebas previas), `Compatibility=50` (1 vacante activa sin skills
   demandados) → `OverallScore=48`. Formula verificada a mano: `0*.3+100*.25+50*.25+50*.2=47.5→48`.
2. **Compatibilidad con match exacto**: se agrego 1 skill demandado por la vacante activa +
   la misma skill al candidato → `CompatibilityIndex=100`, `OverallScore=58` (coincide con la
   formula).
3. **Compatibilidad con penalizacion**: se agrego una 2da skill al candidato que nadie demanda
   → `CompatibilityIndex=90` (100 - 0.5*20, coincide exactamente).
4. **Auto-recalculo al editar**: se agrego una experiencia via `POST /api/profile/experience`
   **sin llamar al endpoint de recalculo** → `Version` en `PT_CandidateScores` subio solo,
   confirmando que `ProfileService` dispara `RecalculateAsync` automaticamente.
5. **Estabilidad/Confiabilidad con datos reales**: 2 experiencias (StartupX 1.5 meses = salto
   laboral; Acme Corp actual, gap de ~15.7 meses no reciente entre ambas, antiguedad actual >
   12 meses = bonus). Resultado exacto: `StabilityIndex=28` (duracion 33.33 - salto 15 + bonus
   10 = 28.33→28), `ReliabilityIndex=82` (100 - 17.85→82), `OverallScore=59` - ambos coinciden
   con el calculo manual.
6. **Consistencia con ValidationService**: se corrio `POST verifications/run` sobre los mismos
   datos - `CvCoherence` dio `Score=68` (100 - 15 salto - 17 gap, incluye ambas penalizaciones
   porque esa verificacion evalua "es el CV coherente" en general) mientras que
   `ReliabilityIndex` dio 82 (solo gap, sin el salto laboral que ya penaliza Estabilidad). Es
   una diferencia esperada y documentada, no un bug: son dos indices con alcance distinto que
   comparten la misma formula de gap mas no el mismo conjunto de penalizaciones.
7. **Ownership guard**: `POST` contra un `candidateId` que no es el propio del usuario
   autenticado devuelve `403 Forbidden`.
8. Datos de prueba (2 experiencias, 2 `PTCandidateSkill`, 1 `PTVacancySkill`, 1 `PTSkill`
   sintetico) borrados despues de verificar; el score volvio exactamente al baseline original.

`dotnet build` sin errores en `OpenToWork.API` (arrastra `Core`/`Models`).
