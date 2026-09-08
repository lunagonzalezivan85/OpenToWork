# Validación: Embudo Ciego (Entrega de Personal Verificado)

> **Agente:** RH
> **Fecha:** 08-Sep-2026
> **Guía de referencia:** `docs/rh/guia-embudo-ciego-iluna.md`
> **Estado:** IMPLEMENTADO — cumple el flujo descrito, con 2 brechas menores pendientes

---

## Veredicto

El flujo del embudo ciego descrito en la guía **está implementado y cumple** el modelo de negocio: la empresa ve conteos de postulantes (nunca identidad) y solo accede al personal verificado que Trato Directo le entrega. Quedan 2 brechas de endurecimiento (no bloquean el flujo, se detallan abajo).

---

## Checklist de validación (Paso 7 de la guía)

### A) Confidencialidad — ✅ CUMPLE

| Punto | Estado | Evidencia |
|---|---|---|
| Empresa no ve nombres/perfiles de postulantes en ninguna vista | ✅ | `VacancyManage.razor` muestra 3 conteos (postulantes / en verificación / entregados); `CompanyDashboard.razor` muestra personal entregado; `VerifiedApplicants.razor` solo candidatos con delivery |
| Endpoint de aplicaciones no expone identidad | ✅ | `GET api/applications/vacancy/{id}` devuelve `VacancyApplicantSummaryDto` (solo Total/InVerification/Delivered) |
| Empresa no cambia estados de postulaciones | ✅ | `PUT applications/{id}/status` eliminado de `ApplicationsController` |
| Búsqueda avanzada fuera del menú empresa | ✅ | Link removido de `MainLayout.razor` (página y endpoint intactos por decisión de producto) |

### B) Entrega desde admin — ✅ CUMPLE

| Punto | Estado | Detalle |
|---|---|---|
| Stage 4 + vacante requeridos | ✅ | `DeliveryService.DeliverCandidateAsync` valida `CurrentStage == ReadyToDeliver` y rechaza si no |
| Verificado TD requerido | ✅ | Valida `IVerificationStatusService.IsVerifiedTD`; error "El candidato debe estar Verificado TD antes de ser entregado" (backend; el checklist acepta "error claro") |
| Duplicados rechazados | ✅ | Valida delivery activo existente para misma recruitment+vacancy |
| Auditoría | ✅ | `_auditLog.LogAsync("Recruitment.Deliver", ...)` con IP |
| UI admin | ✅ | `PipelineDetail.razor`: botón "Entregar a empresa" (habilitado con vacante vinculada), modal con nota de presentación, sección "Entregas" con estado y feedback de la empresa |

### C) Vista empresa — ✅ CUMPLE

| Punto | Estado | Detalle |
|---|---|---|
| Empresa ve solo entregados | ✅ | `GET api/deliveries/my` filtra por `Company.SCUserId == userId` del token; sin delivery no hay identidad |
| Badge Verificado TD + score + % perfil | ✅ | `DeliveryService.MapToDtoAsync` calcula con `IVerificationStatusService` |
| Acciones empresa | ✅ | `PUT api/deliveries/{id}/respond` permite solo 2=Interesado, 3=Contratado, 4=No encaja; setea `ViewedAt`/`RespondedAt` |
| Feedback visible para admin | ✅ | Sección Entregas del pipeline muestra estado y feedback |

### D) Conteos — ✅ CUMPLE

- `GetVacancySummaryAsync`: Total = postulaciones activas; Delivered = deliveries activos; InVerification = Total − Delivered (postulados sin delivery).
- Valida propiedad de la vacante (`vacancy.Company.SCUserId == companyUserId`), si no → null/404.

### E) Regresión — ✅ CUMPLE

- `GET api/applications/my` intacto (candidato sigue aplicando y viendo sus postulaciones).
- Pipeline admin sin cambios (stages, investigación, evaluaciones).
- Migración `20260908113350_CandidateDeliveries` aplicada con índices y FKs.
- Build sin errores; portales corriendo (API 5000, WEB 5147, AdminAPI/AdminWEB).

---

## Brechas pendientes (no bloquean, recomendadas)

1. **`GET api/permanentvacancies/{id}/matches` sigue exponiendo `CandidateName`** de postulantes a la empresa (shortlist por match score). La UI ya no lo consume, pero el endpoint responde identidad si se llama directo. *Recomendación:* restringir a rol admin o filtrar solo candidatos con delivery activo.
2. **`/candidate-search` accesible por URL directa** en el portal empresa (fuera del menú por decisión temporal). Cuando se reactive, filtrar solo entregados o moverlo a rol admin.

## Datos de prueba para validar el flujo

Ver `docs/credenciales-ejemplo.md`. Flujo sugerido:
1. Admin: candidato en stage 4 con Verificado TD → "Entregar a empresa" (vacante de Hotel Sol Caribe)
2. Empresa (`rrhh@hotelsolcaribe.com`): "Postulantes verificados" → candidato entregado → "Me interesa"
3. Admin: ver el cambio de estado y feedback en la sección Entregas del pipeline
4. Verificar que `GET api/applications/vacancy/{id}` como empresa devuelve solo `{total, inVerification, delivered}`
