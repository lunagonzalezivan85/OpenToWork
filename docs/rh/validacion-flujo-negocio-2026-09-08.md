# Análisis RH: Validación del Flujo de Negocio

> **Fecha:** 08-Sep-2026  
> **Agente:** RH  
> **Proceso:** Validación del flujo empresa-postulante-admin contra el modelo de negocio de Trato Directo

---

## Flujo de negocio definido (por Iluna)

1. Las empresas publican vacantes.
2. Los postulantes aplican a las vacantes.
3. Las empresas ven **cuántas** personas se postulan, pero **NO ven quiénes** se postulan.
4. Nosotros (Trato Directo, desde el admin) escogemos quiénes se entregan como **personal ya verificado**.
5. La empresa puede ver el **personal ya verificado** (solo lo que le entregamos).

---

## Veredicto: NO CUMPLE (3 brechas críticas)

### Brecha 1: La empresa ve QUIÉNES se postulan (violación del modelo)

**Evidencia:**
- `src/OpenToWork.WEB/Components/Pages/VerifiedApplicants.razor` — la página "Postulantes verificados" del portal empresa muestra la lista completa de postulantes con **nombre completo, avatar, título, fecha y % de perfil**. Además permite abrir el **perfil completo** del candidato (`GoToApplicantProfile`).
- `src/OpenToWork.Core/Services/ApplicationService.cs` (`GetApplicationsByVacancyAsync`, líneas 52-72) — el endpoint `GET api/applications/vacancy/{vacancyId}` devuelve **todas** las aplicaciones de la vacante con datos completos del candidato. Solo valida que el usuario sea dueño de la vacante; **no filtra por estado de entrega/verificación**.

**Lo que debería pasar:** la empresa solo ve el **conteo** de postulantes. La identidad se revela únicamente cuando Trato Directo entrega al candidato verificado.

### Brecha 2: No existe el mecanismo de "entrega" de personal verificado

**Evidencia:**
- El pipeline de reclutamiento tiene la etapa "Listo a Entregar" (`RecruitmentStage.ReadyToDeliver = 4`), pero **no hay ningún flujo, entidad ni endpoint que conecte esa etapa con la visibilidad de la empresa**.
- No existe una entidad tipo `PTDelivery` / `PTCandidateDelivery` que registre qué candidatos fueron presentados a qué empresa para qué vacante.
- El admin no tiene una acción "Entregar a empresa" en el pipeline.

**Lo que debería pasar:** al marcar un candidato como "Listo a Entregar" (o mediante una acción explícita de entrega), el admin selecciona la empresa/vacante destino y a partir de ese momento la empresa ve ese candidato en su portal.

### Brecha 3: La empresa puede aceptar/rechazar postulantes directamente

**Evidencia:**
- `src/OpenToWork.API/Controllers/ApplicationsController.cs` (`UpdateStatus`, líneas 67-75) — el endpoint `PUT api/applications/{id}/status` permite a la **empresa** cambiar el estado de la postulación (aceptar/rechazar) directamente.
- `VerifiedApplicants.razor` muestra labels "En revision", "Rechazado", "Aceptado" gestionados por la empresa.

**Lo que debería pasar:** la decisión de avanzar/descartar postulantes es de **Trato Directo** (admin), no de la empresa. La empresa solo decide sobre el personal **ya entregado y verificado** (contratar/no contratar lo entregado).

---

## Lo que SÍ cumple

- **Empresas publican vacantes:** `Vacancies.razor`, `VacancyManage.razor`, `MyVacancies.razor` en el portal empresa. OK.
- **Postulantes aplican a vacantes:** `POST api/applications` desde el portal candidato. OK.
- **Conteo de postulantes por vacante:** `VerifiedApplicants.razor` muestra el número de postulantes por vacante (aunque también muestra la lista, que es la brecha 1).
- **Pipeline de verificación interno:** el admin tiene el pipeline completo (investigación, evaluación técnica, entrevista cultural, "Listo a Entregar") y el estado "Verificado TD" (sub-fase 3.7). La infraestructura de verificación existe; falta el puente de entrega.

---

## Diseño propuesto (refinado con Iluna, 08-Sep)

### Flujo final validado

```
Empresa publica vacante
        │
Postulantes aplican ──► Empresa ve SOLO conteos en la vacante:
        │                 • "12 postulantes" (total)
        │                 • "3 en verificación" (en proceso por TD)
        │
Admin (Trato Directo) revisa pipeline:
  Investigación → Evaluación Técnica → Entrevista Cultural → Listo a Entregar
        │
Admin ENTREGA candidatos verificados a la empresa
        │
NUEVA VISTA empresa: "Personal Verificado"
  └─ Solo candidatos entregados por TD (nombre, badge Verificado TD, score, perfil)
        │
Empresa decide: Interesado / Contratado / No contratado
```

### 1. Vacantes: solo conteos, sin identidad

- La card de vacante en el portal empresa muestra:
  - **Total de postulantes** (todos los que aplicaron)
  - **"X en verificación"** (postulantes que el admin tiene en proceso en el pipeline)
- Nuevo endpoint: `GET api/applications/vacancy/{vacancyId}/summary` — devuelve `{ total, inVerification, delivered }`.
- Modificar `GetApplicationsByVacancyAsync` o deprecarlo para el rol empresa: **sin entrega, la empresa no recibe nombres ni perfiles**.
- En `VerifiedApplicants.razor`: la vista por vacante ya no lista postulantes; muestra los conteos y un CTA "Ver personal verificado".

### 2. Entidad de entrega (puente admin → empresa)

```
PTCandidateDelivery
- Id (Guid, PK)
- PT_RecruitmentId (FK → PTCandidateRecruitment)
- PT_CandidateId (FK)
- PT_VacancyId (FK)
- PT_CompanyId (FK)
- DeliveredByUserId (FK → SC_User, admin que entrega)
- DeliveredAt (DateTime)
- Status (enum: Delivered=0, ViewedByCompany=1, Interested=2, Hired=3, RejectedByCompany=4)
- CompanyFeedback (string?)
```

### 3. Acción de entrega en el admin

- En `Candidates/PipelineDetail.razor`, etapa "Listo a Entregar" (stage 4): botón "Entregar a empresa" → seleccionar vacante → confirma → crea `PTCandidateDelivery`.
- Requisito: candidato con `IsVerifiedTD = true` (o verificación aprobada).
- Opcional: nota del admin para la empresa al entregar (presentación del candidato).

### 4. Nueva vista empresa: "Personal Verificado"

- Nuevo endpoint: `GET api/deliveries/my` — candidatos entregados a las vacantes de la empresa autenticada.
- Nueva página (o reemplazo de `VerifiedApplicants.razor`): **solo** candidatos con delivery activo:
  - Nombre, badge "Verificado TD", score general, % perfil
  - Perfil completo en modo lectura
  - Acciones de la empresa sobre el entregado: Marcar interés / Contratado / No encaja (con feedback)
- El conteo por vacante se mantiene; la identidad solo aparece aquí (brecha 1 resuelta).

### 5. Permisos

- Empresa: NO puede cambiar status de aplicaciones. Solo acciona sobre **candidatos entregados** (tabla delivery).
- Admin: único con poder de avanzar etapas y entregar candidatos.
- Postulante: ve su estado en el pipeline (notificaciones por cambio de etapa).

---

## Tareas para el equipo (FS/Dsiezar/Iluna)

| # | Tarea | Prioridad |
|---|-------|-----------|
| 1 | Endpoint `GET api/applications/vacancy/{vacancyId}/summary` (total, inVerification, delivered) + ocultar identidad en `GetApplicationsByVacancyAsync` para rol empresa | ALTA |
| 2 | Entidad `PTCandidateDelivery` + migración | ALTA |
| 3 | Acción "Entregar a empresa" en PipelineDetail (stage 4) con requisito Verificado TD | ALTA |
| 4 | Endpoint `GET api/deliveries/my` + nueva vista "Personal Verificado" en el portal empresa (reemplaza lista en `VerifiedApplicants.razor`) | ALTA |
| 5 | Quitar a la empresa el `PUT applications/{id}/status`; reemplazar por acciones sobre delivery (Interesado/Contratado/No encaja) | ALTA |
| 6 | i18n de las nuevas pantallas (es/en) | MEDIA |
| 7 | Notificación al candidato cuando es entregado a una empresa | MEDIA |

---

## Conclusión

El modelo de negocio "embudo ciego" (la empresa ve cantidad, Trato Directo filtra y entrega verificados) **es la propuesta de valor central de Trato Directo** y hoy el sistema la viola: la empresa ve todo y decide todo. Las 5 tareas de prioridad ALTA de arriba cierran el ciclo y alinean el producto al modelo. Sin esto, no hay diferenciación frente a una bolsa de empleo tradicional.
