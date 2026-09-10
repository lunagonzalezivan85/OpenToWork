# RH — Alineación con Trato Directo y Arranque del Proceso de Reclutamiento

> **Soy RH.** Análisis solicitado por Iluna (`README.md`, sección "Observaciones para Darwin / Dsiezar", punto 3, 06-Sep-2026): *"Consultar a la IA y al agente de Recursos Humanos si lo llevado hasta ahora cumple con lo alineado a Trato Directo. Evaluar: qué falta, por qué no hemos iniciado el proceso de reclutamiento e investigación, y qué pasos siguen."*
>
> Fecha: 2026-09-07
> Proceso: Evaluación de estado — no es sobre una vacante puntual, es sobre si la plataforma ya sostiene el negocio real de Trato Directo.
> Fuentes: `docs/rh/analisis-portafolio-candidatos.md` (15-Ago), `docs/dsiezar/respuesta-rh.md` (15-Ago, decisiones de negocio de Darwin), código actual en `main`, y datos reales de la base MySQL al momento de escribir esto.

---

## 1. Respuesta corta a las 3 preguntas de Iluna

1. **¿Está alineado con Trato Directo?** Sí, en lo estructural. El pivote a hostelería (categorías reales, vacantes sembradas), el motor de scoring/verificación, el modelo standalone sin HRIS y el flujo de cierre de negociación siguen la estrategia que Darwin fijó en `respuesta-rh.md`. Pero **una decisión de negocio clave de esa misma respuesta no está implementada en el código**: ver punto 2.4 abajo.
2. **¿Qué falta?** Nada bloqueante en código para operar un piloto chico — falta lo que le da al producto su razón de ser (el gate de "solo candidatos evaluados"), y falta la capa operativa (comunicación por email, cobro real, planes con precios reales).
3. **¿Por qué no arrancó el reclutamiento real?** Porque hasta hoy el esfuerzo fue 100% construir la herramienta. Es una respuesta de negocio, no técnica — ver sección 4.

---

## 2. Estado real de la plataforma (no lo que dice el código, lo que dice la base)

| Métrica | Valor real hoy | Lectura de RH |
|---|---|---|
| Candidatos totales | 10 | Todos de prueba (`qatest_...`, `newcandidate@...`, `testcandidate@...`) — **cero candidatos reales** |
| Candidatos con score calculado | 3 de 10 | El motor de scoring (Fase 3) casi no se ha corrido sobre la base real |
| Candidatos con alguna verificación | 1 de 10 | — |
| Candidatos con verificación **aprobada** | **0** | Ningún candidato tiene hoy el distintivo "Verificado" que `respuesta-rh.md` #4 define como *"la señal de confianza central del producto"* |
| Empresas totales | 4 | 3 de prueba + 1 creada hoy en QA (Hostal Costa Brava) — **cero empresas reales** |
| Empresas en el CRM de captación | 2 | El CRM de Iluna (06-Sep) recién se está probando, no se usó todavía para captar una empresa real |
| Vacantes activas | 20 | Todas sembradas por script SQL, no publicadas por una empresa real |
| Postulaciones totales | 2 | — |
| Negociaciones (presentar candidato → cerrar vacante) | 1 creada, 1 cerrada | La probé yo mismo hoy en QA para validar que el flujo funciona — no es un cierre real de negocio |
| Personal administrativo | 4 cuentas | Todas cuentas de prueba (`admin@opentowork.com`, `reclutador1@...`, `comercial1@...`) — **nadie las usa como su trabajo del día a día todavía** |
| Candidatos en el Pipeline de Reclutamiento manual (Iluna) | 5 | Con actividad de prueba, no de un proceso real en curso |

**Conclusión de esta tabla: la plataforma tiene el motor completo, pero está en punto muerto (0 combustible real) — no le falta construcción, le falta arranque.**

---

## 3. ¿Está alineado con Trato Directo? — Revisión punto por punto contra `respuesta-rh.md`

| Decisión de negocio (respuesta-rh.md, 15-Ago) | Estado en código hoy |
|---|---|
| Candidatos gratis, ingreso del lado empresa | ✅ Sin cambios, se mantiene |
| Standalone, sin integración a HRIS de terceros | ✅ Confirmado — ninguna fase la agregó |
| Planes escalonados (Free/Basic/Pro/Enterprise) para empresas | 🟡 Existen 3 planes (Basic/Premium/Platinum) en el CRM de Iluna, pero son **placeholders sin CRUD ni precios reales** (tarea abierta #1 de Iluna en el README) |
| **Badge/estrella de "Verificado" visible en perfil y listas** | 🟡 Existe el checkmark "Verificado TD" (sub-fase 3.7/3.8), pero **0 candidatos lo tienen hoy** porque nadie corrió el proceso de verificación real |
| **Gate de visibilidad: candidatos no evaluados no aparecen a empresas** | 🔴 **No implementado.** Revisé `CandidateSearchService.cs:27` — el filtro que decide qué candidato ve una empresa es `!IsDeleted && IsProfilePublic && WizardCompleted`. El nivel de verificación (`MinVerificationStatus`) es un **filtro opcional** que la empresa puede aplicar, no un piso obligatorio. Hoy, una empresa que busca sin filtros ve candidatos con wizard completo aunque tengan **cero verificaciones aprobadas**. Esto contradice directamente la respuesta #10 de `respuesta-rh.md`: *"la base de datos de candidatos que ve una empresa debe estar compuesta únicamente por personal ya evaluado"* |
| Sin mecanismo de apelación de score | ✅ Correcto, no se construyó (como se decidió) |
| Recalculo de score inmediato + mensual | 🟡 El recalculo inmediato existe (botón "Recalcular" manual); no encontré un job periódico mensual automático — no es crítico para un piloto chico |
| Scorecard configurable por vacante (peso por índice) | ✅ Existe (`PT_Vacancy.WeightsConfig`, sub-fase 3.8) |

**El hallazgo más importante de esta revisión es el gate de visibilidad faltante.** No es un bug menor: es la pieza que hace que Trato Directo sea distinto de "una bolsa de empleo más" (la comparación que la propia `respuesta-rh.md` #12 usa contra LinkedIn/Computrabajo/Bumeran). Sin ese gate, hoy técnicamente una empresa podría ver un candidato sin ninguna verificación, exactamente el escenario que se quiso evitar.

---

## 4. ¿Por qué no hemos iniciado el proceso de reclutamiento e investigación?

Esta es la pregunta que de verdad importa, y la respuesta no está en el código — está en que **el proyecto hasta ahora ha sido 100% modo construcción, cero modo operación**:

1. **No hay una persona real operando el rol de Reclutador o Comercial.** Las 4 cuentas de staff son de prueba. El Pipeline de Reclutamiento (Iluna) y el CRM de Captación de Empresas (Iluna) son herramientas terminadas y probadas — pero herramientas sin operador. Un ATS sin reclutador no recluta solo.
2. **No hay canal de adquisición.** Cero tráfico real, cero campaña de outreach a empresas de hostelería, cero publicación en canales donde estén candidatos reales del rubro (camareros, cocineros, etc.). El CRM de captación asume que alguien va a cargar leads — hoy nadie lo está alimentando.
3. **Falta la capa de comunicación real.** Ya documentado en el proyecto: no hay SMTP configurado. Ningún candidato ni empresa real puede recibir un email de la plataforma hoy (confirmación de cuenta, cambio de estado de postulación, notificación de negociación). Esto no es un detalle — es un bloqueante operativo real para correr aunque sea un piloto de 5 empresas, porque en cualquier punto del flujo (`respuesta-rh.md` no lo dice explícito, pero el análisis original de RH sí, sección "Candidate Experience") se necesita poder avisarle a alguien que algo pasó.
4. **El gate de visibilidad de la sección 3 no está.** Aunque hubiera candidatos reales hoy, el producto no está protegiendo todavía la promesa central ("solo te mostramos gente evaluada") — lanzar un piloto sin esto genera el riesgo de que la primera empresa real vea candidatos sin verificar y pierda confianza en el diferenciador.

En una palabra: **el motor está armado y probado, pero nadie lo arrancó con combustible real, y todavía le falta una pieza de seguridad (el gate) antes de salir a la calle.**

---

## 5. Qué pasos siguen — recomendación priorizada de RH

### Antes de reclutar a la primera persona real (bloqueante, esta semana)
1. **Implementar el gate de visibilidad** en `CandidateSearchService.GetSearchableSkillsAsync`/búsqueda de candidatos: exigir al menos 1 verificación aprobada (o un score mínimo) como piso obligatorio, no como filtro opcional. Es el cambio de código más importante de este documento — cierra la brecha de la sección 3.
2. **Resolver el envío de emails real** (elegir proveedor SMTP — SendGrid, Amazon SES, o similar). Sin esto no se puede operar con personas reales del otro lado.
3. **CRUD de Planes + precios reales** (tarea #1 de Iluna) — no se le puede ofrecer un plan "Basic $49" a una empresa piloto real sin saber si ese precio es el definitivo.

### Para arrancar el piloto (una vez resuelto lo anterior)
4. **Asignar una persona real** (puede ser el propio Darwin o Iluna al principio, no hace falta contratar) al rol Comercial, y usar el CRM ya construido para captar 5-10 empresas piloto reales del rubro hostelería — idealmente contactos directos/conocidos, no leads fríos, para la primera vuelta.
5. **Asignar una persona real** al rol Reclutador, y hacer sourcing activo de 20-30 candidatos reales (camareros, cocineros, recepcionistas, etc.) en el mercado piloto elegido — el Pipeline de Reclutamiento (Iluna) ya soporta este flujo.
6. **Correr un ciclo completo con datos reales**: 1 empresa real, varios candidatos reales evaluados/verificados de verdad, 1 vacante real cerrada a través del flujo completo (postulación → shortlist → negociación → cierre). Esto valida el producto contra la realidad antes de escalar sourcing, y da el primer caso de éxito real para mostrar a la siguiente empresa.

### No bloqueante, pero antes de escalar el piloto
7. Job periódico de recalculo mensual de score (decisión ya tomada en `respuesta-rh.md` #11, no implementada).
8. Cobertura de pruebas automatizadas sobre RBAC/negociaciones/CRM (hoy en cero, según el QA de esta misma sesión) — para no repetir a mano esta validación cada vez que algo cambie.

---

## 6. Nota de coordinación

- **FS (Dsiezar/Iluna):** el punto 1 (gate de visibilidad) es el único cambio de código de esta lista que yo, como RH, marco como bloqueante antes de mostrarle la plataforma a una empresa real.
- **PM:** los puntos 4-5 no son de desarrollo, son de decidir quién ocupa esos roles y cuándo arranca — es una decisión de negocio, no una tarea de sprint.
- **SEC:** una vez haya candidatos/empresas reales, revisar que el manejo de datos personales (ya decidido en `respuesta-rh.md` #4-7) esté implementado tal cual se definió, no solo documentado.

---

*Documento generado por RH — Agente de Reclutamiento y Selección.*
*Actualiza y complementa `docs/rh/analisis-portafolio-candidatos.md` (15-Ago-2026) a la luz de todo lo construido en Fase 3, Fase 4, RBAC/Negociaciones y CRM de Empresas.*
