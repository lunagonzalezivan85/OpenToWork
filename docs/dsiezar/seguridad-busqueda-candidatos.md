# Búsqueda de candidatos: contención (H-39) y registro del incidente

**IA:** Dsiezar · **Fecha:** 2026-10-09 · **Rama:** `dsiezar-seguridad-busqueda` · **Origen:** auditoría externa del 8-Oct-2026 (`tratodirecto-audit`, hallazgo H-39, tareas BE-19 fase A y DSO-15).

## 1. Qué pasaba

- `GET api/candidates/search` solo exigía sesión: **cualquier usuario autenticado** (empresa sin verificar, e incluso un candidato) listaba candidatos con nombre completo, puesto, ciudad, país, score y las cuatro métricas (Estabilidad, Confiabilidad, Evidencia, Compatibilidad).
- Todos los candidatos aparecían por defecto (`PTCandidate.IsProfilePublic = true`) y el portal no ofrece al candidato ningún ajuste para ocultarse.
- `GET api/candidates/{id}/score` y `/verification-status` devolvían el score y el estado de verificación de **cualquier** candidato a cualquier usuario con sesión (los Id salen de la búsqueda).
- La página `/candidate-search` no estaba en el menú, pero se abría por URL. Ocultar no es control de acceso.
- El registro de empresa es libre (sin captcha ni verificación del NIF real), así que en la práctica cualquiera podía obtener esos datos.

## 2. Contención aplicada (este cambio)

| Qué | Antes | Ahora |
|---|---|---|
| `api/candidates/search` y `search/skills` | Cualquier usuario con sesión | **403 para todos** (cerrada) |
| `api/candidates/{id}/score` | Cualquier usuario con sesión | Solo el propio candidato o una empresa a la que TD se lo entregó; si no, 404 |
| `api/candidates/{id}/verification-status` | Igual | Igual que el score |
| `/candidate-search` (portal) | Formulario de búsqueda | Aviso "no disponible" |

La regla "propio candidato o empresa a la que TD se lo entregó" es la misma que ya protegía el perfil y el CV (`ProfileService.CanViewCandidateAsync` / `IsDeliveredToViewerAsync`). El panel del candidato (su propio score) sigue funcionando. `ICandidateSearchService` se conserva para el rediseño.

Comprobado en local (API real): candidata → búsqueda 403, su score 200, score de otro 404; empresa → búsqueda 403, score de una candidata no entregada 404; sin sesión → 401.

## 3. Registro del incidente (completar — DSO-15)

> Plazo legal: si es una brecha notificable, la AEPD debe recibirla en **72 h desde que el responsable tiene constancia** (RGPD art. 33). Documentar la decisión **en cualquier caso** (art. 33.5).

- [ ] **Constancia:** fecha y hora en que Trato Directo conoció el problema (informe recibido por Darwin el 9-Oct-2026; el auditor lo detectó el 8-Oct).
- [ ] **Contención en producción:** fecha y hora del despliegue de este cambio.
- [ ] **Conservar registros antes de que roten:** IIS y API (`/api/candidates/search`, `/api/candidates/*/score`, `/api/candidates/*/verification-status`, `/api/profile/candidate/*`) y Cloudflare, desde que existe la función. Copiarlos fuera del servidor y anotar quién y cuándo.
- [ ] **Quién accedió:** cuentas que llamaron a la búsqueda. Excluir la cuenta del auditor (`patroclosystems+empresa@gmail.com`, empresa "Patroclo Pruebas SL", 8-Oct). Revisar empresas sospechosas (NIF raro, registros en serie) y suspenderlas.
- [ ] **Qué se expuso:** cuántos candidatos y qué campos (nombre, puesto, ciudad, país, score y métricas; no documento, teléfono, correo ni CV).
- [ ] **Decisión legal** con asesoría/DPO: ¿notificar a la AEPD (art. 33)? ¿comunicar a los candidatos (art. 34)? Escribir aquí la decisión y el motivo.
- [ ] **Cuentas de prueba del auditor:** marcarlas como prueba o eliminarlas (`patroclosystems@gmail.com` y `+empresa`).

## 4. Para reabrir la búsqueda (fase B — decisiones de Darwin)

1. **Verificación de empresas:** estado Pendiente/Verificada/Rechazada revisado por el staff. Solo las verificadas buscan.
2. **Opt-in del candidato:** `IsProfilePublic` pasa a **falso por defecto**, con un ajuste en el perfil ("Permitir que empresas verificadas me encuentren") y explicación clara. Los perfiles existentes quedan ocultos salvo consentimiento demostrable.
3. **Datos mínimos en la búsqueda:** por ejemplo nombre de pila o iniciales, ciudad, puesto y años de experiencia. Nombre completo, CV y contacto solo tras postularse o con aceptación del candidato.
4. **Score y métricas:** no se muestran a empresas hasta tener la evaluación de impacto (EIPD), la base legal y la información al candidato (RGPD arts. 13/14/22).
5. **Registro de cada búsqueda y vista** (empresa, criterios, resultados), límite por empresa y alerta ante volúmenes raros.
