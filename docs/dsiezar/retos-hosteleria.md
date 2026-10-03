# Retos de hostelería ("Demuestra tus habilidades")

**IA:** Dsiezar · **Fechas:** 2026-10-03 → 2026-10-04 · **Rama:** `dsiezar-retos` · **Estado:** MVP completo, pendiente de validar en navegador.

Retos prácticos por puesto (camarero/a, cocina, gerente, eventos...). El candidato practica sin límite o hace una
evaluación que cuenta como evidencia. Parte se corrige sola en el servidor y parte la revisa una persona con una
rúbrica de 0 a 4. Sustituye a la tarjeta "Retos Técnicos" del dashboard; el módulo viejo `PT_SkillTests` se conserva
(tenía 0 contenido).

## Modelo

| Tabla | Para qué |
|---|---|
| `PT_Competencies` | Competencias evaluables. No se borran: se desactivan. `Slug` solo para el contenido inicial. |
| `PT_JobTypeCompetencies` | Competencias de cada puesto del catálogo existente (`PT_JobTypes`, que gana `Description`). |
| `PT_Challenges` | Reto: `DraftJson` (borrador editable), estado Draft/Published/Archived, última versión. |
| `PT_ChallengeJobTypes` | Un reto puede servir a varios puestos sin duplicar contenido. |
| `PT_ChallengeVersions` | Copia **inmutable** del borrador al publicar (`ContentJson`). |
| `PT_ChallengeAttempts` | Intento atado a su versión. Modo Practice/Evaluation, `RowVersion` contra doble entrega. |
| `PT_ChallengeAnswers` | Una fila por actividad e intento (índice único): reenviar no duplica. |
| `PT_ChallengeReviews` | Revisión humana: puntuaciones por criterio (JSON) y comentario general. |

El contenido de un reto es un JSON tipado (`Shared/Challenges/ChallengeDefinition.cs`). **Plantilla** (cómo se ve) y
**tipo de respuesta** (cómo se corrige) van separados; `ChallengeRules.CompatibleResponses` dice qué combinaciones valen:

| Plantilla | Respuestas |
|---|---|
| Tarjetas | selección única, múltiple, ordenar, acciones + justificación |
| Documento | detectar errores (se marcan filas), selección única, múltiple |
| Ficha de cálculo | cálculo numérico con tolerancia |
| Conversación | selección única, respuesta breve, acciones + justificación |

Migración: `20261003212223_HospitalityChallenges` (sin ruido de seeds). **Hay que aplicarla a mano en local**
(`dotnet ef database update --project src/OpenToWork.Models --startup-project src/OpenToWork.API`); en producción
la aplica el deploy.

## Corrección y resultado

Todo se corrige en el servidor (`ChallengeScoring`); el cliente nunca recibe claves, rúbricas ni explicaciones
de evaluación (`ActivityViewDto.From`). En ordenación las opciones se barajan de forma estable para que el orden
de presentación nunca sea la solución.

- **Selección única / acciones (una):** valor de la opción elegida (0-1; admite opciones "defendibles" a 0,5).
- **Múltiple / detección de errores:** exacta (todo o nada) o parcial = (aciertos − fallos) / correctas, mínimo 0.
  En parcial se avisa al candidato de que los fallos restan.
- **Ordenar:** fracción de relaciones "A antes que B" cumplidas, o 1/0 contra una lista de secuencias aceptadas.
- **Cálculo:** cada campo puntúa si está dentro de la tolerancia (absoluta o %), ponderado por su peso. Acepta
  `12,5`, `12.5`, `1.234,50 €`; no acepta fórmulas.
- **Rúbrica:** cada criterio 0-4 → /4.

**Agregación** (por competencia y total): media ponderada de los elementos ya corregidos. Los criterios humanos
pendientes **no cuentan como 0**: se excluyen y la competencia queda incompleta. El resultado completo solo existe
cuando no falta ninguna revisión. Una actividad entregada sin responder cuenta 0 (y una abierta sin texto no se
manda a revisar). Fortalezas ≥ 75 %, a practicar < 50 % (textos editoriales por competencia en cada reto).
Las notas de retos distintos **no son comparables** (dificultad distinta); la interfaz lo dice.

## Reglas de intentos

- Práctica: ilimitada, con feedback y explicación por actividad, privada (no la ve nadie del equipo ni empresas,
  nunca va a revisión).
- Evaluación: el candidato acepta las condiciones; máx. 1-5 entregas por reto y 0-90 días de espera entre ellas
  (configurables por reto). Un solo intento en curso por modalidad: volver a entrar retoma el mismo.
- Autoguardado (1,5 s tras cada cambio y al cambiar de actividad). Entregar es idempotente; después las respuestas
  quedan bloqueadas.
- Editar un reto publicado no cambia nada para los candidatos hasta publicar una versión nueva; los intentos ya
  iniciados terminan con su versión. Archivar impide intentos nuevos pero deja terminar los iniciados.
- Solo se pueden eliminar borradores nunca publicados ni usados.

## Permisos (comprobados en servidor)

| Acción | Quién |
|---|---|
| Crear/editar retos, competencias, puestos, vista previa | SuperAdmin, Reclutador |
| Publicar, archivar, restaurar, cargar contenido inicial | Solo SuperAdmin |
| Revisar | SuperAdmin (todo); Reclutador solo candidatos que tiene asignados (`PT_CandidateRecruitments.AssignedToUserId`) |
| Ver resultados de un candidato (perfil admin) | Todo el equipo (incluye Comercial) |
| Empresa | Solo resultados de evaluaciones de candidatos que TD le ha entregado (`PT_CandidateDeliveries`, misma regla que el CV). Nunca respuestas ni comentarios del revisor. |

## Endpoints

- Portal (`OpenToWork.API`, `ChallengesController`): `GET api/challenges/job-types`, `GET api/challenges/job-types/{id}`,
  `GET api/challenges/{id}`, `POST api/challenges/{id}/attempts`, `GET api/challenge-attempts/{id}`,
  `PUT api/challenge-attempts/{id}/answers/{activityKey}`, `POST api/challenge-attempts/{id}/submit`,
  `GET api/challenge-attempts/{id}/result`, `GET api/challenge-attempts/history`,
  `GET api/company/candidates/{candidateId}/challenge-results`.
- Admin (`OpenToWork.AdminAPI`, `api/admin/challenges`): competencias, puestos, CRUD de retos, `publish`,
  `archive`, `restore`, `duplicate`, `preview`, `seed`, `reviews`, `candidates/{userId}/results`.

## Pantallas

- Portal: `/skills` (catálogo por puesto + historial), `/skills/{id}` (presentación y reintentos),
  `/skills/attempt/{id}` (intento), `/skills/result/{id}`. La tarjeta del dashboard lleva aquí.
  Empresa: sección en `/applicant-profile/{id}`.
- Admin: `/challenges` (retos, competencias, puestos), `/challenges/{id}` (editor con vista previa),
  `/challenges/reviews` y `/challenges/reviews/{attemptId}`. Sección en el perfil del candidato.
- El reproductor (`SharedUI/Components/ChallengeActivity.razor` + `challenges.css`) es el mismo en portal y en la
  vista previa del admin. Accesible con teclado (ordenar con botones Subir/Bajar y anuncio `aria-live`),
  respeta `prefers-reduced-motion`.

## Contenido inicial

`OpenToWork.Core/Challenges/Seed/challenges-hosteleria.json`: 18 competencias, 5 puestos, 10 retos × 3 actividades
con rúbricas y explicaciones. Se carga desde el admin ("Cargar contenido inicial", SuperAdmin), **no al arrancar**.
Es idempotente: no duplica ni sobrescribe ediciones (por slug/nombre); publica la v1 de cada reto que valida.

> **Debe validarlo una persona del sector** antes de usarlo como evidencia real: importes, tolerancias, órdenes
> de prioridad, protocolos de alérgenos y APPCC, y la redacción de las rúbricas. Lo escribió una IA a partir de
> buenas prácticas generales, no de la operativa de Trato Directo.

## Cómo probar (local)

1. Aplicar la migración (ver arriba) y arrancar MySQL de XAMPP, API (5000), AdminAPI (5001), WEB (5100) y AdminWEB (5101).
2. Admin como SuperAdmin → `http://localhost:5101/challenges` → "Cargar contenido inicial".
3. Portal como candidato → `http://localhost:5100/skills` → elegir puesto → practicar y hacer una evaluación con
   respuesta abierta.
4. Admin → `http://localhost:5101/challenges/reviews` → puntuar y completar → el candidato ve el resultado completo.
5. Tests: `dotnet test src/OpenToWork.Tests --filter "FullyQualifiedName~Challenge"` (31, sin BD real).

## Pendiente / fuera del MVP

- Arrastrar y soltar en ordenación (hoy botones Subir/Bajar, accesibles por teclado).
- Avisos por correo al candidato cuando se completa la revisión (el SMTP está, falta el disparador).
- Validación del contenido inicial por profesionales (ver arriba).
- Retirar el módulo viejo `PT_SkillTests` (`/skill-tests`) cuando se confirme que no se usa.
