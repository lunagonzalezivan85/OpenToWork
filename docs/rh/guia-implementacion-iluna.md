# Guía de Implementación: Ciclo Completo de Reclutamiento

> **Para:** Iluna  
> **De:** RH  
> **Fecha:** 06-Sep-2026  
> **Objetivo:** Completar los 5 items de prioridad ALTA para cerrar el ciclo de reclutamiento antes de iniciar búsquedas activas.

---

## Item #1: Scorecard de Competencias Estructurada (1-5)

### Problema
No existe una rubrica con escalas 1-5 por competencia técnica y blanda antes de publicar la vacante. La evaluación es inconsistente.

### Qué crear

#### Entidades nuevas

**`PTScorecard`** — plantilla de scorecard por vacante
```
Id (Guid, PK)
PT_VacancyId (Guid, FK → PT_Vacancy)
Title (string)
Description (string?)
IsActive (bool)
+ BaseEntity (CreatedAt, UpdatedAt, etc.)
```

**`PTScorecardCompetency`** — competencias dentro de la scorecard
```
Id (Guid, PK)
PT_ScorecardId (Guid, FK → PT_Scorecard)
Name (string) — ej: "React", "Comunicación asertiva"
Type (enum: Technical=0, Soft=1)
IsRequired (bool) — no negociable vs deseable
Weight (int) — peso porcentual en el score total (suma = 100)
Scale (int) — escala máxima, default 5
+ BaseEntity
```

**`PTScorecardEvaluation`** — evaluación de un candidato contra la scorecard
```
Id (Guid, PK)
PT_ScorecardId (Guid, FK)
PT_CandidateId (Guid, FK)
PT_RecruitmentId (Guid, FK → PTCandidateRecruitment)
EvaluatedByUserId (Guid, FK → SC_User)
OverallScore (decimal) — calculado: suma(score * weight) / suma(weights)
Notes (string?)
+ BaseEntity
```

**`PTScorecardScore`** — score individual por competencia
```
Id (Guid, PK)
PT_ScorecardEvaluationId (Guid, FK)
PT_ScorecardCompetencyId (Guid, FK)
Score (int) — 1 a 5
Notes (string?)
+ BaseEntity
```

#### Enum nuevo
```csharp
public enum CompetencyType
{
    Technical = 0,
    Soft = 1
}
```

#### API endpoints
```
GET    /api/admin/recruitment/{recruitmentId}/scorecard
POST   /api/admin/recruitment/{recruitmentId}/scorecard
PUT    /api/admin/scorecards/{id}
DELETE /api/admin/scorecards/{id}
POST   /api/admin/scorecards/{id}/evaluate   — evaluar candidato
PUT    /api/admin/scorecard-evaluations/{id}  — actualizar evaluación
```

#### UI en AdminWEB
- En `Candidates/PipelineDetail.razor`, agregar una sección en la etapa de Evaluación Técnica (stage 2) que muestre:
  - Lista de competencias de la scorecard de la vacante vinculada
  - Sliders o selects 1-5 por cada competencia
  - Score total calculado automáticamente
  - Notas por competencia

#### Migración
```bash
dotnet ef migrations add Scorecard --project src/OpenToWork.Models --startup-project src/OpenToWork.AdminAPI
```

#### Archivos a crear/modificar
| Archivo | Acción |
|---------|--------|
| `OpenToWork.Models/Entities/PTScorecard.cs` | Crear |
| `OpenToWork.Models/Entities/PTScorecardCompetency.cs` | Crear |
| `OpenToWork.Models/Entities/PTScorecardEvaluation.cs` | Crear |
| `OpenToWork.Models/Entities/PTScorecardScore.cs` | Crear |
| `OpenToWork.Shared/Enums/CompetencyType.cs` | Crear |
| `OpenToWork.Shared/DTOs/ScorecardDtos.cs` | Crear |
| `OpenToWork.Models/Context/AppDbContext.cs` | Agregar DbSets + configuración |
| `OpenToWork.Core/Interfaces/IRecruitmentService.cs` | Agregar métodos scorecard |
| `OpenToWork.Core/Services/RecruitmentService.cs` | Implementar métodos |
| `OpenToWork.AdminAPI/Controllers/RecruitmentController.cs` | Agregar endpoints |
| `OpenToWork.AdminWEB/Services/AdminAuthApiService.cs` | Agregar métodos cliente |
| `OpenToWork.AdminWEB/Components/Pages/Candidates/PipelineDetail.razor` | Agregar UI scorecard |
| `wwwroot/config/language/es/admin.json` | Agregar traducciones |
| `wwwroot/config/language/en/admin.json` | Agregar traducciones |

---

## Item #2: Entrevista STAR/CAR con Rúbrica

### Problema
Las entrevistas culturales son texto libre. No hay metodología STAR/CAR estructurada ni rubrica de competencias evaluadas.

### Qué crear

#### Entidades nuevas

**`PTInterviewTemplate`** — plantilla de entrevista reutilizable
```
Id (Guid, PK)
Name (string) — ej: "Entrevista Cultural Estándar", "Entrevista Técnica Senior"
Type (enum: Cultural=0, Technical=1, Initial=2)
Description (string?)
IsActive (bool)
+ BaseEntity
```

**`PTInterviewQuestion`** — preguntas dentro del template
```
Id (Guid, PK)
PT_InterviewTemplateId (Guid, FK)
Question (string) — ej: "Cuéntame de una vez que tuviste un conflicto con un compañero"
Methodology (enum: STAR=0, CAR=1, Free=2)
Competency (string) — ej: "Resolución de conflictos", "Liderazgo"
Order (int)
+ BaseEntity
```

**`PTInterviewSession`** — sesión de entrevista con un candidato
```
Id (Guid, PK)
PT_InterviewTemplateId (Guid, FK)
PT_RecruitmentId (Guid, FK → PTCandidateRecruitment)
InterviewerUserId (Guid, FK → SC_User)
ScheduledAt (DateTime?)
CompletedAt (DateTime?)
OverallScore (decimal) — promedio de respuestas
OverallRecommendation (enum: StrongNo=0, No=1, Maybe=2, Yes=3, StrongYes=4)
Notes (string?)
+ BaseEntity
```

**`PTInterviewAnswer`** — respuesta del candidato a cada pregunta
```
Id (Guid, PK)
PT_InterviewSessionId (Guid, FK)
PT_InterviewQuestionId (Guid, FK)
Situation (string?) — campo S del STAR
Task (string?) — campo T
Action (string?) — campo A
Result (string?) — campo R
Score (int) — 1 a 5
Notes (string?)
+ BaseEntity
```

#### Enum nuevo
```csharp
public enum InterviewMethodology
{
    STAR = 0,
    CAR = 1,
    Free = 2
}

public enum InterviewRecommendation
{
    StrongNo = 0,
    No = 1,
    Maybe = 2,
    Yes = 3,
    StrongYes = 4
}
```

#### API endpoints
```
GET    /api/admin/interview-templates                    — listar templates
POST   /api/admin/interview-templates                    — crear template
PUT    /api/admin/interview-templates/{id}               — editar template
DELETE /api/admin/interview-templates/{id}               — eliminar template
GET    /api/admin/recruitment/{id}/interviews            — listar sesiones del candidato
POST   /api/admin/recruitment/{id}/interviews            — crear sesión
PUT    /api/admin/interviews/{id}                        — actualizar sesión (respuestas + score)
GET    /api/admin/interviews/{id}                        — detalle de sesión
```

#### UI en AdminWEB
- Nueva página `Candidates/InterviewTemplates.razor` — CRUD de templates
- En `Candidates/PipelineDetail.razor`, etapa de Entrevista Cultural (stage 3):
  - Selector de template de entrevista
  - Formulario con preguntas del template
  - Campos STAR (Situación, Tarea, Acción, Resultado) por pregunta
  - Score 1-5 por pregunta
  - Score total y recomendación final
  - Posibilidad de múltiples sesiones (ej: 2 entrevistadores distintos)

#### Seed data
Crear 1 template default "Entrevista Cultural Estándar" con 8-10 preguntas STAR:
1. "Cuéntame de una vez que tuviste un conflicto con un compañero de trabajo" (Resolución de conflictos)
2. "Describe una situación donde tomaste la iniciativa en un proyecto" (Iniciativa)
3. "Háblame de una vez que recibiste feedback negativo" (Receptividad)
4. "Cuéntame de un proyecto que fracasó y qué aprendiste" (Resiliencia)
5. "Describe cómo manejaste una situación de alta presión" (Manejo de estrés)
6. "Háblame de una vez que tuviste que adaptarte a un cambio importante" (Adaptabilidad)
7. "Cuéntame de una vez que lideraste un equipo" (Liderazgo)
8. "Describe una situación donde fuiste más allá de lo esperado" (Compromiso)

---

## Item #4: Flujo de Oferta al Candidato

### Problema
No existe carta de oferta ni flujo de negociación. El proceso termina en "Listo a Entregar".

### Qué crear

#### Entidad nueva

**`PTCandidateOffer`** — oferta formal al candidato
```
Id (Guid, PK)
PT_RecruitmentId (Guid, FK → PTCandidateRecruitment)
PT_VacancyId (Guid, FK → PT_Vacancy)
Salary (decimal)
Currency (string) — "EUR", "USD"
Benefits (string?) — texto libre o JSON
StartDate (DateTime?)
WorkMode (enum: Onsite=0, Hybrid=1, Remote=2)
ContractType (string?) — "Indefinido", "Temporal", "Freelance"
ProbationPeriod (string?) — ej: "3 meses"
Status (enum: Draft=0, Sent=1, UnderReview=2, Accepted=3, Rejected=4, Expired=5)
ValidUntil (DateTime?)
SentAt (DateTime?)
RespondedAt (DateTime?)
Notes (string?)
+ BaseEntity
```

#### Enum nuevo
```csharp
public enum OfferStatus
{
    Draft = 0,
    Sent = 1,
    UnderReview = 2,
    Accepted = 3,
    Rejected = 4,
    Expired = 5
}

public enum WorkMode
{
    Onsite = 0,
    Hybrid = 1,
    Remote = 2
}
```

#### API endpoints
```
GET    /api/admin/recruitment/{id}/offer          — obtener oferta del candidato
POST   /api/admin/recruitment/{id}/offer          — crear oferta
PUT    /api/admin/offers/{id}                      — actualizar oferta
POST   /api/admin/offers/{id}/send                 — marcar como enviada
PUT    /api/admin/offers/{id}/status               — actualizar estado (aceptada/rechazada)
```

#### UI en AdminWEB
- En `Candidates/PipelineDetail.razor`, agregar sección en etapa "Listo a Entregar" (stage 4):
  - Formulario de oferta: salario, moneda, beneficios, fecha inicio, modalidad, tipo contrato
  - Botón "Enviar oferta" — cambia status a Sent
  - Selector de status: Borrador → Enviada → En Revisión → Aceptada/Rechazada
  - Generador de carta de oferta en HTML imprimible (similar al contract-generator.js)
  - Historial de cambios de estado

#### Carta de oferta (HTML template)
Crear `wwwroot/js/offer-generator.js` similar a `contract-generator.js` pero con:
- Datos de la empresa, vacante, salario, beneficios
- Fecha de inicio, modalidad, tipo de contrato
- Espacio para firma del candidato

---

## Item #5: Onboarding Guiado (30/60/90 días)

### Problema
No hay seguimiento post-contratación. Sin esto no hay Quality of Hire ni métricas de deserción temprana.

### Qué crear

#### Entidad nueva

**`PTOnboarding`** — seguimiento post-contratación
```
Id (Guid, PK)
PT_RecruitmentId (Guid, FK → PTCandidateRecruitment)
PT_CandidateId (Guid, FK)
PT_VacancyId (Guid, FK)
HiredAt (DateTime)
Status (enum: Active=0, Completed=1, EarlyExit=2)
ExitReason (string?)
ExitAt (DateTime?)
+ BaseEntity
```

**`PTOnboardingCheckin`** — check-in programado
```
Id (Guid, PK)
PT_OnboardingId (Guid, FK)
Type (enum: Day30=0, Day60=1, Day90=2)
ScheduledAt (DateTime)
CompletedAt (DateTime?)
PerformanceScore (int) — 1 a 5
CultureFitScore (int) — 1 a 5
RetentionRisk (enum: Low=0, Medium=1, High=2)
Notes (string?)
ActionItems (string?) — próximos pasos si hay riesgo
+ BaseEntity
```

#### Enum nuevo
```csharp
public enum OnboardingStatus
{
    Active = 0,
    Completed = 1,
    EarlyExit = 2
}

public enum CheckinType
{
    Day30 = 0,
    Day60 = 1,
    Day90 = 2
}

public enum RetentionRisk
{
    Low = 0,
    Medium = 1,
    High = 2
}
```

#### API endpoints
```
GET    /api/admin/recruitment/{id}/onboarding          — obtener onboarding
POST   /api/admin/recruitment/{id}/onboarding          — iniciar onboarding (al aceptar oferta)
GET    /api/admin/onboarding/{id}/checkins             — listar check-ins
POST   /api/admin/onboarding/{id}/checkins             — crear check-in
PUT    /api/admin/checkins/{id}                         — completar check-in
PUT    /api/admin/onboarding/{id}/exit                  — registrar salida temprana
```

#### UI en AdminWEB
- En `Candidates/PipelineDetail.razor`, agregar sección después de "Listo a Entregar":
  - Solo visible si el candidato tiene oferta aceptada
  - Timeline de 3 check-ins (30, 60, 90 días) con estado pendiente/completado
  - Formulario por check-in: performance, culture fit, riesgo de retención, notas, action items
  - Si riesgo = Alto, mostrar alerta visual
  - Al completar los 3 check-ins, marcar onboarding como Completed
  - Botón "Registrar salida temprana" con motivo

---

## Item #6: Comunicación Reclutador-Candidato

### Problema
No hay canal de comunicación directo entre el reclutador (admin) y el candidato dentro del pipeline.

### Qué crear

#### Entidad nueva

**`PTRecruitmentMessage`** — mensajes entre reclutador y candidato
```
Id (Guid, PK)
PT_RecruitmentId (Guid, FK → PTCandidateRecruitment)
FromUserId (Guid, FK → SC_User)
ToUserId (Guid, FK → SC_User)
Content (string)
IsRead (bool)
ReadAt (DateTime?)
+ BaseEntity
```

#### API endpoints (AdminAPI)
```
GET    /api/admin/recruitment/{id}/messages          — listar mensajes
POST   /api/admin/recruitment/{id}/messages          — enviar mensaje
PUT    /api/admin/messages/{id}/read                  — marcar como leído
```

#### API endpoints (API candidatos)
```
GET    /api/recruitment/messages                      — mis mensajes
POST   /api/recruitment/messages/{id}/read            — marcar como leído
```

#### UI en AdminWEB
- En `Candidates/PipelineDetail.razor`, agregar tab o sección "Mensajes":
  - Lista de mensajes (chat estilo burbujas)
  - Input de texto + botón enviar
  - Indicador de leído/no leído

#### UI en portal de candidatos (OpenToWork.WEB)
- Nueva página `/messages` o sección en dashboard:
  - Lista de conversaciones con reclutadores
  - Chat con burbujas
  - Notificación de mensajes no leídos

#### Notificaciones automáticas (bonus)
Al cambiar de etapa en el pipeline, enviar mensaje automático:
- Stage 0 → 1: "Hola {nombre}, hemos recibido tu postulación y comenzaremos el proceso de investigación."
- Stage 1 → 2: "¡Buenas noticias! Has pasado a evaluación técnica. Te contactaremos pronto."
- Stage 2 → 3: "Has avanzado a entrevista cultural. Agendaremos una fecha contigo."
- Stage 3 → 4: "¡Felicidades! Estás listo para ser presentado a la empresa."
- Descarte: "Te agradecemos por tu interés. {feedback opcional}"

---

## Orden de implementación recomendado

| Orden | Item | Dependencia | Esfuerzo |
|-------|------|-------------|----------|
| 1º | **#1 Scorecard** | Ninguna | Medio (4 entidades + UI) |
| 2º | **#2 Entrevista STAR** | Scorecard (opcional, puede usar competencias de scorecard) | Medio (4 entidades + UI + seed) |
| 3º | **#4 Oferta al candidato** | Ninguna | Medio (1 entidad + UI + generator) |
| 4º | **#5 Onboarding** | Oferta aceptada | Medio (2 entidades + UI) |
| 5º | **#6 Mensajería** | Ninguna | Alto (1 entidad + UI en 2 portales + notificaciones) |

## Notas importantes

- **Diseño:** Todo cambio visual debe ser autorizado por Iluna (regla del proyecto). Esta guía es técnica, no visual.
- **Estilo:** Usar `admin-input` en todos los inputs, paleta existente (#0066FF, #0B132B, #3A506B, #778DA9, etc.).
- **i18n:** Todas las claves nuevas deben agregarse en `es/admin.json` y `en/admin.json`.
- **Migraciones:** Una migración por feature, no mezclar entidades de distintos items.
- **Testing:** Después de cada item, ejecutar AdminAPI + AdminWEB y verificar en navegador.
- **Commits:** Un commit por item con mensaje descriptivo.
