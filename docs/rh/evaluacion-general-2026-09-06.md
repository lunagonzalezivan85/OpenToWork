# Análisis RH: Evaluación del Proyecto Trato Directo

> **Fecha:** 06-Sep-2026  
> **Agente:** RH (Reclutamiento y Selección)  
> **Proceso:** Evaluación general del proyecto contra el modelo operativo de reclutamiento

---

## 1. Mapeo del Perfil y Arquitectura de Habilidades

### Lo que YA existe:
- **Scorecard automática** (`PTCandidateScore`): 4 indices calculados automáticamente (Estabilidad, Confiabilidad, Evidencia, Compatibilidad) con `OverallScore` 0-100.
- **Job Match Score** (`PTJobMatchScore`): matching candidato-vacante con `MatchPercentage`, `SkillsMatch`, `ExperienceMatch`, `EducationMatch`.
- **Scorecard configurable** por vacante: `PTVacancy.WeightsConfig` permite ajustar pesos del matching.
- **Verificaciones automáticas** (`PTVerification`): LinkedIn, portafolio, coherencia CV, identidad.

### Lo que FALTA:
- **Scorecard de competencias blandas (1-5)**: No existe una rubrica estructurada con escalas 1-5 por competencia técnica y blanda antes de publicar la vacante. El score actual es automático pero no permite evaluación manual estructurada con competencias definidas.
- **Definición previa de competencias no negociables**: No hay entidad que permita al reclutador definir qué competencias son obligatorias vs deseables antes de iniciar la búsqueda.
- **Plantillas de scorecard por rol/industria**: No hay templates reutilizables de scorecards por tipo de puesto.

**Prioridad:** ALTA — Es el paso 1 del flujo operativo. Sin scorecard estructurada, la evaluación es inconsistente.

---

## 2. Sourcing Multicanal e Inbound Talent

### Lo que YA existe:
- **Búsqueda de candidatos** (`SearchLinkedin.razor`): página de búsqueda en el portal admin.
- **Portal de candidatos con vacantes públicas**: los candidatos pueden autorregistrarse y postularse.
- **Pipeline de captación de empresas** (CRM): nuevo pipeline comercial para captar empresas cliente.

### Lo que FALTA:
- **Sourcing booleano avanzado**: No hay búsqueda con operadores lógicos (AND, OR, NOT) sobre skills, experiencia, ubicación simultáneamente.
- **Atracción pasiva**: No hay integración con LinkedIn Recruiter, GitHub, ni comunidades de nicho.
- **Inbound recruiting**: No hay landing pages de vacantes optimizadas para SEO ni campañas de employer branding.
- **Base de talent pool**: No hay un pool de candidatos pasivos/previos para recontactar en futuras búsquedas.

**Prioridad:** MEDIA — El sourcing manual funciona pero no escala. Priorizar después de tener el core del reclutamiento operativo.

---

## 3. Criba Curricular y Entrevista de Diagnóstico Inicial

### Lo que YA existe:
- **Pipeline de reclutamiento** (`RecruitmentStage`): 5 etapas (Postulación → Investigación → Evaluación Técnica → Entrevista Cultural → Listo a Entregar).
- **Checklist de investigación** (`PTInvestigationChecklist`): 5 pasos (Llamar candidato, Llamar referencias, Validar LinkedIn, Validar portafolio, Validar certificaciones) + validaciones custom.
- **Referencias laborales** (`PTReferenceCheck`): registro de contactos, estado (pendiente, llamada, verificada, fallida), notas.
- **Preferencias del candidato** (`PTCandidateRecruitmentPreferences`): expectativas salariales, disponibilidad, info de migración.
- **Descarte con motivo** (`PTRecruitmentDismissal`): razones (Técnico, Referencias, Salario, No mostró, Otro) + notas.
- **Detección automática de red flags** (`ValidationService.DetectRedFlagsAsync`): saltos laborales <3 meses, cambios de sector frecuentes, gaps inexplicables.

### Lo que FALTA:
- **Criba automática por expectativas salariales**: No hay filtro automático que descarte o marque candidatos cuya expectativa salarial excede el rango de la vacante.
- **Detección de motivos de rotación histórica**: El `StabilityIndex` calcula estabilidad pero no diferencia entre rotación involuntaria (recortes/proyectos) y voluntaria (falta de compromiso). Falta un campo "motivo de salida" en `PTCandidateExperience`.
- **Entrevista de diagnóstico inicial estructurada**: No hay template de preguntas de criba inicial (15-20 min) con scoring. La etapa 0 (Postulación) no tiene estructura de entrevista.

**Prioridad:** ALTA — La criba es donde se pierde más tiempo. Automatizar filtros y estructurar la primera entrevista es crítico.

---

## 4. Evaluación Práctica y Técnica Estandarizada

### Lo que YA existe:
- **Evaluaciones técnicas manuales** (`PTTechnicalEvaluation`): nombre, descripción, score, evidencia, notas, tipo, recomendación. CRUD completo.
- **Banco de retos técnicos** (`PTSkillTest`): categoría, dificultad, título, preguntas JSON, tiempo límite.
- **Resultados de retos** (`PTCandidateTestResult`): score, tiempo, anti-cheat flags.
- **Retos técnicos con timer** en el portal del candidato (`/skill-tests`).
- **Job Match Score** automático: compara skills, experiencia, educación vs vacante.

### Lo que FALTA:
- **Evaluación basada en escenarios reales del puesto**: Los retos técnicos existen pero no están vinculados a una vacante específica. No hay forma de asignar un reto específico a un candidato en el contexto de un pipeline de reclutamiento.
- **Pruebas situacionales**: No hay retos de tipo "caso real" donde el candidato resuelve un problema del día a día del puesto (no solo multiple choice o código).
- **Anti-copia más robusto**: Solo hay `AntiCheatFlags` (int). Falta detección de tab switching, copiar/pegar, webcam proctoring.
- **Evaluación por pares**: No hay forma de que múltiples evaluadores califiquen al mismo candidato y se promedie.

**Prioridad:** MEDIA — El banco de retos existe pero necesita vinculación al pipeline de reclutamiento.

---

## 5. Entrevista por Competencias y Fit Cultural

### Lo que YA existe:
- **Entrevistas culturales** (`PTTechnicalEvaluation` con `Type` diferenciado): registradas en `CulturalInterviews` dentro del detalle de reclutamiento.
- **Score y recomendación**: cada entrevista tiene score decimal y recomendación (enum).
- **Notas y evidencia**: se puede adjuntar URL de evidencia y notas libres.

### Lo que FALTA:
- **Metodología STAR/CAR estructurada**: No hay template de preguntas STAR (Situación, Tarea, Acción, Resultado). Las entrevistas se registran como texto libre, no hay rubrica de competencias evaluadas.
- **Culture Add vs Culture Fit**: No hay evaluación estructurada de qué aporta el candidato en diversidad de pensamiento. Solo hay un score numérico.
- **Entrevistas estructuradas con rubrica**: Falta una entidad `PTInterviewRubric` con competencias definidas y escala 1-5 por cada una.
- **Feedback estructurado por entrevistador**: No hay múltiples entrevistadores con evaluaciones independientes.

**Prioridad:** ALTA — Sin estructura de entrevista, la evaluación cultural es subjetiva y no comparable entre candidatos.

---

## 6. Oferta, Cierre y Onboarding

### Lo que YA existe:
- **CRM de captación de empresas** con generación de contratos: el contrato de prestación de servicios se genera al cerrar el pipeline de empresas (no de candidatos).
- **Etapa "Listo a Entregar"** (stage 4): el candidato está listo para ser presentado a la empresa.
- **Vinculación candidato-vacante**: `LinkVacancyDto` permite asociar el candidato a una vacante específica.

### Lo que FALTA:
- **Generación de oferta al candidato**: No hay entidad ni flujo para generar una carta de oferta al candidato (salario, beneficios, fecha de inicio, condiciones). Solo existe el contrato empresa-Trato Directo.
- **Negociación salarial estructurada**: Las preferencias del candidato registran expectativas pero no hay un flujo de contraoferta/negociación.
- **Onboarding guiado**: No hay entidad ni flujo de onboarding. No hay seguimiento de primeras semanas, check-ins, ni métricas de deserción temprana (30/60/90 días).
- **Comunicación candidato-reclutador**: No hay mensajería directa entre el reclutador (admin) y el candidato dentro del pipeline. Solo hay mensajería empresa-candidato.
- **Notificaciones al candidato**: No hay notificaciones automáticas al candidato cuando avanza de etapa, se le asigna una entrevista, o se le hace una oferta.

**Prioridad:** ALTA — El onboarding es donde se valida la calidad de la contratación. Sin seguimiento post-contratación, no hay Quality of Hire.

---

## 7. People Analytics y Métricas

### Lo que YA existe:
- **Dashboard admin con métricas**: métricas generales del sistema (usuarios, vacantes, aplicaciones).
- **CountByStage en el pipeline**: conteo de candidatos por etapa del pipeline de reclutamiento.
- **Scores automáticos**: `PTCandidateScore` con 4 indices + overall.

### Lo que FALTA:
- **Time-to-Hire**: No se calcula ni se muestra el tiempo promedio desde postulación hasta "Listo a Entregar" o contratación.
- **Quality of Hire**: No hay métrica post-contratación que evalúe si el candidato fue una buena contratación (performance a 30/60/90 días).
- **Costo por contratación**: No hay tracking de costos del proceso de reclutamiento.
- **Tasa de rotación temprana**: No hay seguimiento de cuántos candidatos abandonan en los primeros 90 días.
- **Funnel de conversión por etapa**: No hay visualización de tasas de conversión entre etapas del pipeline (cuántos pasan de Investigación a Evaluación Técnica, etc.).
- **Source of hire**: No hay tracking de de dónde vienen los candidatos (LinkedIn, directo, referencia, etc.).

**Prioridad:** MEDIA — Las métricas son importantes para optimización continua pero requieren que el core del reclutamiento esté completo primero.

---

## 8. CRM de Captación de Empresas (Nuevo, 06-Sep)

### Lo que YA existe:
- **Pipeline comercial 6 etapas**: Lead → Contactado → Reunión → Propuesta Enviada → Negociación → Cerrado Ganado.
- **Wizard progresivo** con inputs unificados por etapa.
- **Mapa interactivo** (Leaflet + OpenStreetMap) para país/ciudad.
- **Planes** (Basic/Premium/Platinum) con cards de selección.
- **Generación de contratos** de prestación de servicios imprimible.
- **Descarte/restauración** de empresas con motivo.
- **Historial** de cambios de etapa.

### Evaluación RH:
El CRM de empresas está bien diseñado para la captación comercial. **Cumple su propósito**. Las observaciones para Darwin/Dsiezar (CRUD de planes, tabla de configuración, templates de contrato) son correctas y necesarias para que el sistema sea administrable.

---

## Resumen Ejecutivo

### ¿Cumple el proyecto con el modelo de una reclutadora?

**Parcialmente (60%).** El proyecto tiene una base sólida de evaluación automática (scoring, verificaciones, matching) y un pipeline de reclutamiento manual funcional. Sin embargo, **faltan componentes críticos** para que el flujo de reclutamiento sea completo y profesional.

### Lo que está bien:
- Pipeline de reclutamiento con 5 etapas claras
- Evaluación técnica y entrevista cultural con score
- Checklist de investigación con referencias
- Scoring automático con 4 indices
- Matching candidato-vacante configurable
- CRM de captación de empresas con contratos

### Lo CRÍTICO que falta (prioridad ALTA):
1. **Scorecard de competencias estructurada (1-5)** — sin esto la evaluación es inconsistente
2. **Entrevista STAR/CAR con rubrica** — sin esto la evaluación cultural es subjetiva
3. **Criba automática por expectativas salariales** — ahorra tiempo del reclutador
4. **Flujo de oferta al candidato** — no hay carta de oferta ni negociación
5. **Onboarding guiado con seguimiento 30/60/90** — sin esto no hay Quality of Hire
6. **Comunicación reclutador-candidato** — no hay canal directo dentro del pipeline
7. **Notificaciones automáticas al candidato** — candidate experience deficiente

### Lo IMPORTANTE que falta (prioridad MEDIA):
8. Sourcing booleano avanzado y talent pool
9. Vinculación de retos técnicos al pipeline de reclutamiento
10. People Analytics (Time-to-Hire, funnel de conversión, source of hire)
11. Motivo de salida en experiencia laboral (rotación voluntaria vs involuntaria)
12. Múltiples entrevistadores con evaluaciones independientes

### ¿Por qué no se ha iniciado el proceso de reclutamiento e investigación?

**Porque el sistema está listo para gestionar candidatos que ya están en el pipeline, pero no está listo para iniciar búsquedas activas desde cero.** Faltan:

1. **Definición de la vacante con scorecard** — no se puede iniciar una búsqueda sin definir primero qué competencias se evalúan y con qué rubrica.
2. **Sourcing** — no hay herramientas para atraer candidatos pasivos ni buscar en bases externas.
3. **Comunicación con candidatos** — no hay forma de contactar al candidato dentro del sistema durante el proceso.
4. **Onboarding** — el proceso termina en "Listo a Entregar" pero no hay seguimiento post-entrega.

**Recomendación:** Antes de iniciar reclutamiento activo, completar los items de prioridad ALTA #1, #2, #4, #5 y #6. Estos son los que cierran el ciclo completo de reclutamiento.

---

## Próximos pasos sugeridos (para FS / Darwin):

| Orden | Tarea | Esfuerzo estimado |
|-------|-------|-------------------|
| 1 | Crear entidad `PTScorecard` con competencias y escala 1-5 | Medio |
| 2 | Crear entidad `PTInterviewRubric` con preguntas STAR/CAR | Medio |
| 3 | Agregar filtro automático de expectativa salarial vs rango vacante | Bajo |
| 4 | Crear entidad `PTCandidateOffer` (carta de oferta) | Medio |
| 5 | Crear entidad `PTOnboarding` con check-ins 30/60/90 | Medio |
| 6 | Mensajería reclutador-candidato en el pipeline | Alto |
| 7 | Notificaciones automáticas por cambio de etapa | Medio |
| 8 | People Analytics: Time-to-Hire y funnel de conversión | Bajo |
