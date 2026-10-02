# agent.md — Skills de diseño en uso

Registro de las skills de diseño que usa el agente (Claude Code) en este proyecto, y por qué.
Las skills viven en `.claude/skills/` (Emil Kowalski, MIT — ver `.claude/skills/README.md`).

> Recordatorio: según `DESIGN-SYSTEM.md`, todo cambio visual necesita autorización de **Iluna**.
> Las skills de análisis (solo lectura) se pueden usar libremente; aplicar cambios requiere ese visto bueno.

---

## Skill en uso ahora

**`emil-design-eng`** — criterio de pulido de UI y animación (Emil Kowalski).
Usada el 02-Oct-2026 para el análisis inicial de diseño del proyecto (WEB + AdminWEB, Blazor + CSS).

### Resultado del análisis (02-Oct-2026)

Revisados ~18.900 líneas de CSS (`wwwroot/css/*.css` de WEB y AdminWEB) y estilos inline en `.razor`.

| Antes | Después | Por qué |
| --- | --- | --- |
| `transition: all ...` (93 casos; 27 en `WEB/components.css`, 19 en `AdminWEB/components.css`, 16 en `admin.css`) | Propiedades exactas: `transition: background-color 150ms, transform 160ms var(--ease-out)` | `all` anima cosas que no deben (layout, sombras) y cuesta rendimiento |
| Tokens `--transition: 0.25s cubic-bezier(0.4,0,0.2,1)` y `--transition-fast: 0.15s ease` (`base.css`) | Añadir `--ease-out: cubic-bezier(0.23,1,0.32,1)` y `--ease-in-out: cubic-bezier(0.77,0,0.175,1)` | Las curvas por defecto son flojas; las custom dan respuesta inmediata |
| `--transition-slow: 0.4s` y `transition: width 0.6s` (barras de progreso en `admin.css:980,1044`) | ≤ 300 ms; barras con `transform: scaleX()` + `transform-origin: left` | UI por encima de 300 ms se siente lenta; `width` provoca layout |
| `transition: max-height` / `margin-left` (sidebar, `admin.css:158,208`) | `transform: translateX()` para el sidebar; `grid-template-rows: 0fr→1fr` para colapsables | Solo `transform` y `opacity` van por GPU |
| Sin estado `:active` en botones (solo 4 ficheros lo tienen) | `.btn:active { transform: scale(0.97) }` con `transition: transform 160ms ease-out` | El botón debe confirmar que "escuchó" el clic |
| `:hover` sin media query (~180 reglas) | Envolver los hover con movimiento en `@media (hover: hover) and (pointer: fine)` | En móvil el hover se queda "pegado" tras el tap |
| No existe `prefers-reduced-motion` en todo el proyecto | Bloque global que quite traslaciones/escalas y deje fundidos cortos | Accesibilidad: el movimiento puede marear |
| `@keyframes chip-pop` con rebote a `scale(1.05)` (`wizard-profile.css:327`) | Transición `scale(0.95)→1` + opacidad, sin rebote | Los chips se añaden rápido; los keyframes no se pueden interrumpir y el rebote no encaja con un portal profesional |
| `fab-toast-in` / `cvSlideUp` con keyframes (`components.css:4622`, `wizard-profile.css:891`) | Transición con `@starting-style` y `--ease-out` | Interrumpible, sin reinicios si se dispara dos veces |
| Tokens duplicados en `WEB/base.css` y `AdminWEB/base.css` | Moverlos a `OpenToWork.SharedUI` | Una sola fuente de verdad para motion y tema |

Puntos bien resueltos: no hay `scale(0)` ni `ease-in` en la UI; el skeleton-shimmer y el spinner usan keyframes (correcto para movimiento continuo).

---

## Skills propuestas (orden de uso)

| # | Skill | Para qué en este proyecto | Toca código |
|---|---|---|---|
| 1 | `emil-design-eng` | Criterio base y revisión de cada cambio de UI (formato tabla Antes/Después) | Guía |
| 2 | `improve-animations` | Auditoría priorizada de las animaciones existentes + planes ejecutables (tokens de easing, `transition: all`, barras `width`) | No |
| 3 | `find-animation-opportunities` | Dónde falta feedback: `:active` en botones, entrada de modales/toasts, wizard de perfil, bienvenida del candidato | No |
| 4 | `break-ui` | Estrés con datos extremos en pantallas críticas: tarjetas de candidato, pipeline de empresa, perfil v2, ofertas | Solo demo |
| 5 | `review-animations` | Revisión final (manual: `/review-animations`) del diff antes de cada commit visual | No |
| 6 | `animation-vocabulary` | Bajo demanda, para nombrar efectos al hablar con Iluna | No |

No se propone `pick-ui-library`: está orientada a React y el proyecto es Blazor + CSS propio.

### Flujo sugerido
1. `improve-animations` → plan de tokens y limpieza de `transition: all` (WEB primero, luego AdminWEB).
2. `find-animation-opportunities` → lista de huecos de feedback.
3. Autorización de Iluna sobre ambos planes.
4. Implementar por partes (un commit por parte), revisando con `emil-design-eng`.
5. `break-ui` sobre las pantallas tocadas y `/review-animations` sobre el diff.

---

## Historial

| Fecha | Skill | Uso |
|---|---|---|
| 2026-10-02 | `emil-design-eng` | Análisis inicial de diseño y motion del proyecto |
| 2026-10-02 | `improve-animations` | Auditoría de animaciones (10 hallazgos + 3 oportunidades); pendiente elegir cuáles pasan a plan en `plans/` |
