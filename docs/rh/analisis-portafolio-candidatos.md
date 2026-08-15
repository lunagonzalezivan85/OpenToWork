# RH — Análisis de Portafolio de Candidatos

> **Soy RH.** Análisis del proyecto OpenToWork desde la perspectiva de Reclutamiento y Selección.
> Fecha: 2026-08-15
> Proceso: Evaluación de módulos faltantes para un portafolio de candidatos de calidad.

---

## 1. Estado Actual del Proyecto

### Lo que YA existe

| Módulo | Estado | Observación RH |
|--------|--------|----------------|
| Registro de candidato (Wizard 10 pasos) | ✅ | Buen punto de partida, pero incompleto para evaluación real |
| Perfil con datos básicos | ✅ | Nombre, título, resumen, teléfono, ubicación |
| Experiencia laboral | ✅ | Empresa, puesto, fechas, descripción, ubicación |
| Educación | ✅ | Institución, grado, campo de estudio, fechas |
| Certificaciones | ✅ | Nombre, emisor, fechas, credential ID/URL |
| Skills | ✅ | Lista de habilidades con nivel de proficiency |
| LinkedIn / Portafolio / CV | ✅ | URLs almacenadas, pero sin verificación real |
| Vacantes y postulaciones | ✅ | CRUD básico de vacantes y aplicaciones |
| Dashboard admin con métricas | ✅ | Conteos de perfiles evaluados, pendientes, etc. |
| Vista de perfil en admin | ✅ | Perfil completo en modo lectura con donut de completitud |
| Mensajería | ✅ (mock) | Conversaciones entre candidato y empresa |

### Lo que FALTA (crítico para reclutamiento real)

---

## 2. Módulos Faltantes — Priorizados por Impacto

### 🔴 Prioridad 1 — Sin esto no hay portafolio de calidad

#### 2.1 Scorecard de Competencias
**Estado actual:** No existe. Los skills son una lista plana sin evaluación.

**Lo que falta:**
- Escala de 1 a 5 por cada competencia técnica y blanda
- Rubrica objetiva con definiciones claras por nivel
- Posibilidad de que la empresa defina su propio scorecard por vacante
- Comparación automática: scorecard del candidato vs. scorecard requerido por la vacante

**Preguntas para el equipo:**
- ¿Quién define el scorecard: la plataforma, la empresa, o ambos?
- ¿Las competencias blandas se evalúan automáticamente o manualmente?
- ¿El scorecard es visible para el candidato o solo para la empresa?

---

#### 2.2 Evaluación Práctica (Reto Técnico)
**Estado actual:** No existe. No hay forma de evaluar skills reales.

**Lo que falta:**
- Banco de retos técnicos por categoría (dev, design, marketing, etc.)
- Retos basados en escenarios reales del puesto
- Timer, anti-copia, validación de plagio
- Resultado con puntaje y feedback automático
- Integración del resultado en el score del candidato

**Preguntas para el equipo:**
- ¿Los retos los crea la plataforma o cada empresa sube los suyos?
- ¿Se evalúan con IA, tests unitarios, o revisión manual?
- ¿El candidato puede ver su resultado y mejorarlo?

---

#### 2.3 Sistema de Verificaciones Real
**Estado actual:** Solo se guarda la URL de LinkedIn/portafolio/CV. No hay verificación.

**Lo que falta:**
- Verificación de identidad (documento oficial, video selfie, OAuth de LinkedIn)
- Verificación de experiencia (referencias laborales, contacto a empleadores anteriores)
- Verificación de educación (contacto a instituciones, API de verificación)
- Badge de "Verificado" con nivel de confianza (oro, plata, bronce)
- Historial de verificaciones visible para la empresa

**Preguntas para el equipo:**
- ¿La verificación de identidad la hace un tercero (Onfido, Jumio) o manual?
- ¿Las referencias laborales las contacta la plataforma o la empresa?
- ¿Qué nivel de verificación es obligatorio vs. opcional?

---

#### 2.4 Índices de Scoring (Estabilidad, Confiabilidad, Evidencia)
**Estado actual:** Documentado en el plan (Fase 3) pero no implementado.

**Lo que falta:**
- **Índice de Estabilidad:** Análisis de duración en empleos anteriores, frecuencia de cambios, motivos de salida
- **Índice de Confiabilidad:** Verificaciones completadas, consistencia de información, referencias positivas
- **Índice de Evidencia:** Skills validadas con retos, portafolio verificado, certificaciones confirmadas
- **Índice de Compatibilidad:** Match entre perfil del candidato y requisitos de la vacante
- Dashboard visual con los 4 índices en el perfil del candidato
- Comparación de índices entre candidatos para una misma vacante

**Preguntas para el equipo:**
- ¿El scoring es automático (algoritmo) o hay componente humano?
- ¿El candidato puede ver sus propios índices o son solo para empresas?
- ¿Los índices se recalculan automáticamente al actualizar el perfil?

---

#### 2.5 Pipeline de Reclutamiento (ATS)
**Estado actual:** Las postulaciones tienen un estado (Applied, Reviewing, Accepted, Rejected) pero no hay pipeline visual.

**Lo que falta:**
- Pipeline visual estilo Kanban: Applied → Screening → Interview → Offer → Hired
- Drag-and-drop entre estados
- Notas internas por candidato en cada etapa
- Log de actividad (quién movió a quién, cuándo, por qué)
- Filtros por estado, score, fecha, empresa
- Recordatorios y alertas (ej: "Sin respuesta en 5 días")
- Template de pipeline configurable por empresa

**Preguntas para el equipo:**
- ¿El pipeline es único para todas las empresas o configurable?
- ¿Las empresas pueden agregar etapas personalizadas?
- ¿Se envían notificaciones automáticas al candidato al cambiar de estado?

---

### 🟡 Prioridad 2 — Diferenciador competitivo

#### 2.6 Video Pitch del Candidato
**Estado actual:** No existe. Inspirado en Cazvid.

**Lo que falta:**
- Grabación de video pitch de 30-60 segundos desde el portal del candidato
- Almacenamiento en cloud (Azure Blob, AWS S3)
- Reproducción en el perfil visible para empresas
- Compresión y optimización automática
- Moderación admin (aprobar/rechazar videos antes de publicar)

**Preguntas para el equipo:**
- ¿El video es obligatorio para completar el perfil?
- ¿Hay límite de duración o número de intentos?
- ¿Se puede regrabar o es una sola toma?

---

#### 2.7 Referencias Laborales
**Estado actual:** No existe.

**Lo que falta:**
- Candidato agrega 2-3 referencias (nombre, cargo, empresa, email, teléfono, relación)
- Sistema envía solicitud de referencia al contacto
- Referencia responde un formulario breve (recomienda sí/no, comentarios)
- Resultado visible en el perfil con badge de "Referencias verificadas"
- Opción de referencia confidencial (el candidato no ve el contenido)

**Preguntas para el equipo:**
- ¿Cuántas referencias son obligatorias?
- ¿Las referencias tienen fecha de vencimiento?
- ¿Qué pasa si una referencia es negativa?

---

#### 2.8 People Analytics (Métricas de Reclutamiento)
**Estado actual:** Dashboard admin con conteos básicos.

**Lo que falta:**
- **Time-to-Hire:** Tiempo desde publicación de vacante hasta contratación
- **Quality of Hire:** Score del candidato contratado vs. desempeño a 90 días
- **Costo por Contratación:** Basado en tiempo de reclutador y recursos
- **Tasa de Rotación Temprana:** Candidatos que abandonan antes de 90 días
- **Funnel de Conversión:** Postulaciones → Screening → Entrevista → Oferta → Contratado
- **Source of Hire:** De dónde vienen los mejores candidatos
- Gráficos de tendencia mensual/trimestral

**Preguntas para el equipo:**
- ¿Estas métricas son para el admin, para la empresa, o ambos?
- ¿Se necesita integración con un HRIS para medir Quality of Hire post-contratación?
- ¿Qué métricas son visible en el dashboard público vs. privado?

---

#### 2.9 Búsqueda Avanzada de Candidatos (para empresas)
**Estado actual:** No existe. El portal corporativo está pendiente.

**Lo que falta:**
- Búsqueda booleana (AND, OR, NOT) por skills, ubicación, años de experiencia
- Filtros múltiples: score mínimo, verificaciones, disponibilidad, salario esperado
- Ranking de candidatos por relevancia (match score)
- Guardar búsquedas y recibir alertas de nuevos candidatos
- Lista de favoritos / shortlist
- Exportar resultados a CSV/PDF

**Preguntas para el equipo:**
- ¿La búsqueda es gratuita o requiere suscripción?
- ¿Los candidatos pueden optar por no aparecer en búsquedas?
- ¿Hay límite de resultados por plan de suscripción?

---

#### 2.10 Match Inteligente Candidato-Vacante
**Estado actual:** No existe.

**Lo que falta:**
- Algoritmo de matching basado en skills, experiencia, ubicación, salario
- Score de compatibilidad (0-100%) entre candidato y vacante
- Recomendaciones automáticas: "Estos candidatos encajan en tu vacante"
- Recomendaciones al candidato: "Estas vacantes coinciden con tu perfil"
- Notificaciones push/email cuando hay un match alto

**Preguntas para el equipo:**
- ¿El matching usa reglas simples o machine learning?
- ¿El candidato recibe recomendaciones de vacantes automáticamente?
- ¿Se puede ajustar el peso de cada criterio en el match?

---

### 🟢 Prioridad 3 — Optimización y experiencia

#### 2.11 Candidate Experience (Experiencia del Candidato)
**Estado actual:** Wizard de 10 pasos, pero sin feedback en el proceso.

**Lo que falta:**
- Notificaciones automáticas al cambiar estado de postulación
- Feedback estructurado tras rechazo (motivo, áreas a mejorar)
- Timeline visible del proceso: "Tu postulación pasó a entrevista"
- Tiempo estimado de respuesta por empresa
- Rating de la experiencia del candidato con la empresa (NPS)

**Preguntas para el equipo:**
- ¿El feedback de rechazo es obligatorio para la empresa?
- ¿El NPS del candidato es público o privado?
- ¿Se puede desactivar el feedback automático?

---

#### 2.12 Entrevistas Integradas
**Estado actual:** No existe.

**Lo que falta:**
- Agendar entrevistas desde la plataforma (calendario integrado)
- Videoentrevistas integradas (Zoom, Google Meet, Teams)
- Plantillas de preguntas por competencias (STAR/CAR)
- Grabación de entrevistas con consentimiento
- Evaluación estructurada post-entrevista (rubrica por competencias)
- Múltiples entrevistores con evaluaciones independientes

**Preguntas para el equipo:**
- ¿Se integra con Calendly/Google Calendar o se construye calendario propio?
- ¿Las entrevistas se graban? ¿Quién tiene acceso?
- ¿Las plantillas de preguntas son por defecto o configurables?

---

#### 2.13 Gestión de Ofertas y Onboarding
**Estado actual:** No existe.

**Lo que falta:**
- Generación de carta de oferta desde la plataforma
- Negociación salarial dentro del sistema (propuesta/contrapropuesta)
- Firma digital de la oferta
- Checklist de onboarding (documentos, accesos, inducción)
- Seguimiento de primeros 30/60/90 días
- Encuesta de onboarding al candidato

**Preguntas para el equipo:**
- ¿La firma digital usa un tercero (DocuSign) o se construye?
- ¿El onboarding es responsabilidad de la plataforma o de la empresa?
- ¿Se integra con el HRIS de la empresa?

---

#### 2.14 Detección de Red Flags
**Estado actual:** No existe.

**Lo que falta:**
- Análisis automático de trayectoria: saltos laborales frecuentes (< 6 meses)
- Detección de incongruencias: fechas solapadas, gaps sin explicar
- Alerta de expectativas salariales fuera de mercado
- Score de riesgo de rotación temprana
- Notas internas de reclutadores compartidas entre empresas (opt-in)

**Preguntas para el equipo:**
- ¿Las red flags son visibles para el candidato o solo para empresas?
- ¿Hay riesgo legal al compartir notas negativas entre empresas?
- ¿El candidato puede explicar los gaps antes de que sean marcados?

---

#### 2.15 Employer Branding y Perfil de Empresa
**Estado actual:** Datos básicos de empresa (nombre, descripción, web, logo).

**Lo que falta:**
- Página de perfil de empresa completa (cultura, valores, beneficios, fotos)
- Reseñas de empleados actuales y anteriores
- Rating de employer experience por candidatos
- Estadísticas públicas: tiempo promedio de respuesta, tasa de contratación
- Verificación de empresa (RUT, registro mercantil)
- Showcase de proyectos y cultura

**Preguntas para el equipo:**
- ¿Las reseñas de empleados son anónimas?
- ¿La empresa puede responder a reseñas negativas?
- ¿Qué pasa si una empresa tiene un rating bajo?

---

## 3. Resumen de Módulos Faltantes

| # | Módulo | Prioridad | Fase sugerida |
|---|--------|-----------|---------------|
| 1 | Scorecard de Competencias | 🔴 Crítica | Fase 3 |
| 2 | Evaluación Práctica (Retos) | 🔴 Crítica | Fase 3 |
| 3 | Verificaciones Reales | 🔴 Crítica | Fase 3 |
| 4 | Índices de Scoring | 🔴 Crítica | Fase 3 |
| 5 | Pipeline ATS (Kanban) | 🔴 Crítica | Fase 4 |
| 6 | Video Pitch | 🟡 Alta | Fase 4 |
| 7 | Referencias Laborales | 🟡 Alta | Fase 3 |
| 8 | People Analytics | 🟡 Alta | Fase 4 |
| 9 | Búsqueda Avanzada | 🟡 Alta | Fase 5 |
| 10 | Match Inteligente | 🟡 Alta | Fase 5 |
| 11 | Candidate Experience | 🟢 Media | Fase 4 |
| 12 | Entrevistas Integradas | 🟢 Media | Fase 5 |
| 13 | Ofertas y Onboarding | 🟢 Media | Fase 5 |
| 14 | Detección de Red Flags | 🟢 Media | Fase 3 |
| 15 | Employer Branding | 🟢 Media | Fase 5 |

---

## 4. Preguntas Estratégicas para el Equipo

> Estas preguntas deben ser respondidas por PM, FS, SEC e Iluna antes de avanzar.

### 4.1 Modelo de Negocio
1. ¿El portafolio de candidatos es gratuito para candidatos y pago para empresas?
2. ¿Qué módulos son del plan gratuito y cuáles del plan premium?
3. ¿Se cobra por candidato contratado o por suscripción mensual?

### 4.2 Datos y Privacidad
4. ¿Quién es el dueño de los datos del candidato: la plataforma o la empresa?
5. ¿El candidato puede eliminar su perfil y todos sus datos? (GDPR/Ley 25.326)
6. ¿Las notas internas de reclutadores son accesibles al candidato?
7. ¿Cuánto tiempo se conservan los datos de un candidato no contratado?

### 4.3 Evaluación y Scoring
8. ¿El scoring es transparente para el candidato (sabe cómo se calcula)?
9. ¿El candidato puede apelar un score bajo?
10. ¿Las empresas confían en el score de la plataforma o hacen su propia evaluación?
11. ¿Con qué frecuencia se recalcula el score?

### 4.4 Competencia y Diferenciación
12. ¿Qué nos diferencia de LinkedIn, Computrabajo, Bumeran?
13. ¿El video pitch es nuestro diferenciador principal o es el scoring?
14. ¿Las empresas pueden usar la plataforma sin integrar su HRIS?

### 4.5 Escalabilidad
15. ¿Cuántos candidatos y empresas soporta la plataforma en el lanzamiento?
16. ¿El matching con IA escala linealmente o necesita infraestructura dedicada?
17. ¿Se soportan múltiples idiomas además de ES/EN?

---

## 5. Recomendación de RH

> **OpenToWork tiene una base sólida de datos del candidato, pero le falta la capa de evaluación y confianza que justifica su propuesta de valor: "perfiles validados con índices de confiabilidad".**
>
> Sin scoring, sin verificaciones reales y sin evaluación práctica, la plataforma es una bolsa de empleo más. La Fase 3 (Motor de Evaluación) es el bloque crítico que convierte los datos en decisiones de contratación confiables.
>
> **Prioridad inmediata:** Implementar los 4 índices de scoring + verificaciones reales + scorecard de competencias. Esto es lo que diferencia a OpenToWork de cualquier ATS genérico.

---

*Documento generado por RH — Agente de Reclutamiento y Selección.*
*Coordina con PM para priorización, con FS para factibilidad técnica, y con SEC para cumplimiento de protección de datos.*
