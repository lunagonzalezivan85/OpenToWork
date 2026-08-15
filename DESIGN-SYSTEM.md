# OpenToWork — Design System & Guidelines

## Regla General de Autorización

> **IMPORTANTE:** Todo cambio visual o de diseño en el portal requiere autorización explícita de **Iluna**.  
> Los cambios visuales solo pueden ser realizados por **Iluna**.  
> En caso de procesos continuos, **Darwin** supervisa la ejecución.  
> No se permitirá ningún cambio de diseño sin cumplir este protocolo.

---

## Principios de Arquitectura

### Componentes Reutilizables
- **Crear componentes** donde el código sea reutilizable para evitar duplicación.
- Los colores, tipografías y espaciados ya están definidos en el sistema — no inventar nuevos.
- Toda nueva UI debe consumir los tokens existentes (CSS variables del tema).

---

## Paleta de Colores — Portal del Postulante

| Token / Uso              | Hex       | Descripción                          |
|--------------------------|-----------|--------------------------------------|
| Fondo de tarjeta         | `#FFFFFF` | Blanco puro                          |
| Banner suave institucional | `#F0F7FF` | Azul navy muy claro                 |
| Azul royal (acento)      | `#0066FF` | Color primario de marca              |
| Texto principal oscuro   | `#0B132B` | Navy profundo para títulos           |
| Gris azulado intermedio  | `#3A506B` | Roles, subtítulos, chips             |
| Gris neutro discreto     | `#778DA9` | Email, texto secundario              |
| Fondo azul tenue (pills) | `#E8F1FF` | Modalidad, filtros activos           |
| Fondo gris claro (chips) | `#F1F5F9` | Skills, tags neutros                 |
| Azul royal hover         | `#0052CC` | Hover de botones                     |

---

## Tipografía

| Elemento         | Familia              | Peso        | Tamaño     | Color       |
|------------------|----------------------|-------------|------------|-------------|
| Nombre/Título    | Plus Jakarta Sans    | 800 (extrabold) | 1.2rem | `#0B132B`   |
| Rol/Posición     | Plus Jakarta Sans    | 600         | 0.8rem     | `#3A506B`   |
| Email            | Plus Jakarta Sans    | 400         | 0.78rem    | `#778DA9`   |
| Sección          | Plus Jakarta Sans    | 700         | 0.72rem    | `text-muted`|
| Tags/Chips       | Plus Jakarta Sans    | 600         | 0.78rem    | varía       |

---

## Componentes UI — Especificaciones

### Avatar Squircle (One UI)
- `border-radius: 20px` (no circular)
- Fondo: `#0066FF`
- Texto: `#FFFFFF`, `font-weight: 800`
- Borde blanco: `4px solid #FFFFFF` (cuando overlapping)
- Sombra: `0 4px 12px rgba(0, 102, 255, 0.2)`

### Pills / Badges
- **Pill de rol**: `border-radius: 999px`, fondo `#F1F5F9`, texto `#3A506B`
- **Pill de modalidad**: fondo `#E8F1FF`, texto `#0066FF`
- **Chips de skills**: fondo `#F1F5F9`, texto `#3A506B`, `border-radius: 999px`
- **Filtros activos**: fondo `#E8F1FF`, texto `#0066FF`, borde transparente

### Tarjetas (Cards)
- `background: #FFFFFF`
- `border-radius: 20px` o `24px` (según contexto)
- `border: 1px solid var(--border-color)`
- `box-shadow: var(--shadow-sm)`

### Bubbles de Chat
- Propias: `#0066FF`, texto blanco, `border-bottom-right-radius: 4px`
- Ajenas: `#F1F5F9`, texto `#0B132B`, `border-bottom-left-radius: 4px`

### Botón de Envío (Chat)
- Squircle: `border-radius: 14px`
- Fondo: `#0066FF`, hover: `#0052CC`

---

## Navegación Móvil (One UI)

### Top App Bar
- **Izquierda**: Oculto en móvil (sin logo)
- **Centro-Izquierda**: Título dinámico de pantalla actual (bold, alineado a izquierda)
- **Derecha**: Campana de notificaciones + Avatar (44px mínimo táctil)
- Settings (idioma/tema) movidos al dropdown del avatar

### Bottom Navigation Bar
- Fija, 64px de altura
- 4 pestañas: Panel, Vacantes, Postulaciones, Mensajes
- Icono + texto pequeño (0.68rem)
- Item activo: `color: var(--accent-primary)`
- `env(safe-area-inset-bottom)` para notch

### Messages (Móvil)
- Vista default: Solo lista de conversaciones
- Al seleccionar: Se oculta lista, se muestra chat full-screen
- Botón flecha ← en header del chat para regresar
- Solo visible en móvil (`display: none` en desktop)

---

## PWA

### Icono
- SVG: maletín blanco con siglas "OTW" en azul royal sobre fondo `#0066FF`
- Esquinas redondeadas: `rx="112"` (formato 512x512)

### Manifest
- `name`: OpenToWork
- `short_name`: OTW
- `display`: standalone
- `theme_color`: `#0066FF`
- `background_color`: `#FFFFFF`
- `orientation`: portrait

### Service Worker
- Cache de assets estáticos (CSS, JS, iconos)
- Estrategia cache-first para recursos estáticos
- Network-first para navegación dinámica

---

## Archivos Clave del Sistema de Diseño

| Archivo                        | Responsabilidad                          |
|--------------------------------|------------------------------------------|
| `wwwroot/css/base.css`         | Variables CSS, reset, tipografía base    |
| `wwwroot/css/components.css`   | Componentes UI reutilizables             |
| `wwwroot/css/portal-nav.css`   | Navegación superior e inferior, notifs   |
| `wwwroot/css/wizard-profile.css`| Wizard de perfil, sidebar de resumen    |
| `wwwroot/css/responsive.css`   | Breakpoints y ajustes móviles            |
| `wwwroot/themes/navy/theme.css`| Variables del tema navy (colores)        |
| `wwwroot/manifest.json`        | Configuración PWA                        |
| `wwwroot/sw.js`                | Service worker                           |
| `wwwroot/icon.svg`             | Icono PWA (maletín OTW)                  |

---

## Reglas para Nuevos Componentes

1. **Reutilizar antes que duplicar**: Si existe un componente similar, extenderlo.
2. **Usar variables CSS**: Nunca hardcodear colores que ya existen como tokens.
3. **Mobile-first**: Diseñar primero para móvil, luego escalar a desktop.
4. **Touch-friendly**: Mínimo 44px de área táctil en elementos interactivos.
5. **One UI**: Border-radius generosos (14-24px), sombras suaves, espaciados amplios.
6. **Autorización**: Cualquier nuevo componente visual debe ser aprobado por Iluna.
