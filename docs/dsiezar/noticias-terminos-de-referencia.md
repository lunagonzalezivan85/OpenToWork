# Noticias — Términos de referencia

**IA:** Dsiezar · **Fecha:** 2026-10-05 · **Rama:** `dsiezar-noticias` · **Estado:** decisiones cerradas (sección 9), listo para construir.

## 1. Contexto

El menú del portal ya tiene el enlace **Noticias** (`/news`, en `MainLayout.razor`), pero no hay ninguna página detrás:
hoy lleva a "no encontrado". Tampoco existe nada en el administrador ni en la base de datos.

Se quiere que el equipo de Trato Directo publique desde el administrador contenido propio —artículos, vídeos,
anuncios— con una foto y un texto, y que aparezca en la sección Noticias del portal.

## 2. Objetivo

- Dar vida a la sección Noticias con contenido útil para candidatos y empresas (consejos, novedades del sector,
  eventos, novedades de la plataforma).
- Que publicar sea tan sencillo como rellenar un formulario: título, foto, texto y "Publicar". Sin tocar código ni
  pedir ayuda técnica.
- Mantener la promesa del portal: sin terceros ni cookies que no avisemos.

## 3. Alcance

### Entra (primera versión)

**Administrador (`admin.tratodirecto.es`), nuevo menú "Noticias":**
- Listado de publicaciones con estado, tipo, fecha de publicación y autor; filtro por estado y buscador por título.
- Crear y editar una publicación (ver campos en la sección 4).
- Subir **una foto de portada** por publicación, con vista previa y recorte automático al formato de la tarjeta.
- Guardar como **borrador**, **publicar**, **despublicar** (vuelve a borrador) y **archivar**. Borrar solo borradores o archivadas (una publicada hay que despublicarla o archivarla antes).
- **Destacar** una publicación: sale primera y más grande en el portal (máximo una destacada a la vez).
- Botón "Ver como en el portal" para revisar antes de publicar.
- Cada acción (crear, editar, publicar, despublicar, archivar, borrar) queda en el **registro de auditoría** que ya
  tiene el admin.

**Portal (`tratodirecto.es`):**
- `/news`: listado de publicaciones publicadas, de la más nueva a la más antigua, en tarjetas (foto, tipo, título,
  resumen, fecha). La destacada arriba y más grande. Filtro por tipo. Paginación de 9 en 9 ("Ver más").
- `/news/{slug}`: la publicación completa (foto, título, fecha, texto y, si es vídeo, el vídeo).
- Visible **sin iniciar sesión**, para que sirva también de escaparate a quien aún no tiene cuenta.
- Estado vacío digno si todavía no hay nada publicado (en lugar de una página en blanco).

### No entra (posibles fases siguientes)

- Comentarios, "me gusta" o compartir en redes.
- Envío de las noticias por correo (boletín). Ya guardamos el consentimiento comercial (`SC_Users.MarketingConsent`),
  así que sería un paso natural, pero aparte.
- Publicación programada (fecha futura que se publica sola).
- Galería de varias fotos dentro de una misma publicación.
- Contenido en varios idiomas (el texto se escribe en español; solo la interfaz se traduce).
- Bloque "Últimas noticias" en la portada (`/`). Fácil de añadir después, una vez haya contenido.

## 4. Contenido de una publicación

| Campo | Obligatorio | Notas |
|---|---|---|
| Tipo | Sí | **Artículo**, **Vídeo** o **Anuncio** (aviso corto: evento, novedad de la plataforma). |
| Título | Sí | Hasta 120 caracteres. |
| Resumen | Sí | Hasta 280 caracteres. Sale en la tarjeta del listado. |
| Foto de portada | Sí | JPG, PNG o WebP. Ver sección 5. |
| Texto alternativo de la foto | Sí | Describe la imagen para lectores de pantalla (accesibilidad). |
| Cuerpo | Sí en Artículo | Texto con formato básico: párrafos, subtítulos, negrita, cursiva, listas y enlaces. |
| Enlace del vídeo | Sí en Vídeo | YouTube o Vimeo. Ver sección 6. |
| Slug (dirección) | Automático | Se genera del título (`/news/consejos-para-tu-primera-entrevista`); editable. Único. |
| Fecha de publicación | Automática | Se pone al publicar; editable para ordenar contenido antiguo. |
| Destacada | No | Casilla. Al marcar una, se desmarca la anterior. |

## 5. Fotos

- Formatos JPG, PNG o WebP; se comprueba el tipo real del archivo, no solo la extensión (como ya hacen las fotos
  de perfil en `ProfilePhotoStorage`).
- **Se reducen en el navegador antes de subir** (máximo 1600 px de ancho, WebP o JPEG de buena calidad), igual que
  hoy se comprime el vídeo de presentación. Así una foto de móvil de 6 MB queda en unos 200-300 KB, sin añadir
  librerías al servidor. Límite en el servidor: 2 MB tras la reducción.
- Se guardan en la carpeta privada compartida por las dos APIs (`storage/news`), fuera de `wwwroot` y del repositorio.
- Las fotos de **publicaciones publicadas** son públicas (las sirve la API del portal). Las de borradores solo se
  ven desde el admin.
- Al borrar una publicación se borran también sus fotos. Archivar no las borra (se puede volver a publicar).
- **Derechos y personas:** solo fotos propias, con licencia o de bancos libres; si aparecen personas reconocibles,
  con su consentimiento. Lo recordará un aviso junto al botón de subir.

## 6. Vídeos

Insertar un vídeo de YouTube o Vimeo carga contenido de Google o Vimeo, que pone sus propias cookies. Hoy la
política de cookies dice que el portal no carga nada de terceros, así que se propone un **reproductor de dos pasos**:

1. En la página se ve la miniatura con un botón de reproducir. La miniatura se descarga en el servidor al publicar
   y se sirve desde nuestro dominio, así que no se llama a YouTube ni a Vimeo.
2. Solo al pulsar "Reproducir" se carga el vídeo, usando el modo "sin cookies" de YouTube (`youtube-nocookie.com`)
   o el modo `dnt` de Vimeo, con un aviso breve ("Al reproducir, el vídeo se carga desde YouTube").

Hay que añadir esos dominios a la política de seguridad (CSP: `frame-src`) y actualizar `/cookies` y
`docs/COOKIES_Y_PRIVACIDAD.md`.

Alternativa más simple: la tarjeta del vídeo **enlaza** a YouTube/Vimeo en una pestaña nueva, sin reproductor
dentro del portal. No requiere cambios legales, pero se sale del portal. Ver decisión D3.

## 7. Permisos

Decidido (D1): los tres roles del staff pueden hacer todo.

| Rol del staff | Ver | Crear y editar borradores | Publicar / despublicar / archivar / borrar |
|---|---|---|---|
| SuperAdmin | Sí | Sí | Sí |
| Comercial | Sí | Sí | Sí |
| Reclutador | Sí | Sí | Sí |

Se aplica en la API con `[RequireStaffRole]`, como el resto del admin; el menú solo enseña lo que cada rol puede hacer.

## 8. Diseño técnico (resumen)

**Tabla nueva `PT_NewsPosts`:** `Id`, `Type`, `Title`, `Slug` (único), `Summary`, `Body`, `CoverImagePath`,
`CoverImageAlt`, `VideoUrl`, `VideoThumbnailPath`, `Status` (Borrador / Publicada / Archivada), `IsFeatured`,
`PublishedAt`, `CreatedBy`, `UpdatedBy`, `CreatedAt`, `UpdatedAt`, `IsDeleted`. Una migración, sin ruido de seeds,
que en local se aplica a mano (`dotnet ef database update ...`) y en producción la aplica el despliegue.

**Admin API (`/api/admin/news`):** listar, obtener, crear, editar, subir foto, publicar, despublicar, archivar, borrar.

**API del portal (`/api/news`, sin login):** listado paginado (solo publicadas, filtro por tipo), detalle por slug,
y la foto o miniatura de una publicación publicada.

**Texto con formato:** se guarda como Markdown sencillo y se convierte a HTML **en el servidor**, sin permitir HTML
escrito a mano ni scripts. Los enlaces externos se abren en pestaña nueva con `rel="noopener nofollow"`. En el admin,
una barra mínima de botones (negrita, cursiva, subtítulo, lista, enlace) y vista previa; sin editores pesados de
terceros. Ver decisión D2.

**Portal:** páginas `News.razor` (`/news`) y `NewsDetail.razor` (`/news/{slug}`), con el mismo estilo plano del
resto (tarjetas, tema claro/oscuro, móvil). Imágenes con carga diferida (`loading="lazy"`).

**Interruptor:** opción "Mostrar Noticias en el portal" en Configuración del admin, como ya se hace con los vídeos
de presentación. Apagado, el enlace del menú desaparece; así no se ve una sección vacía mientras se carga contenido.

## 9. Decisiones (cerradas por Darwin el 2026-10-05)

| # | Decisión | Resultado |
|---|---|---|
| D1 | ¿Quién puede **publicar**? | **SuperAdmin, Comercial y Reclutador** pueden crear, publicar, despublicar, archivar y borrar. |
| D2 | Formato del texto | Markdown sencillo con barra de botones (seguro y sin dependencias). |
| D3 | Vídeos | Reproductor de dos pasos dentro del portal (sección 6). |
| D4 | ¿Para quién son las noticias? | Para todos, también sin cuenta. |
| D5 | ¿Subir vídeos propios (archivo MP4)? | No en la primera versión. |
| D6 | Categorías además del tipo | No en la primera versión; el tipo basta para filtrar. |

## 10. Criterios de aceptación

- [ ] Un SuperAdmin crea un artículo con foto y texto, lo publica y aparece en `/news` sin recargar el admin.
- [ ] La foto de un móvil (6-8 MB) se sube sin errores y en el portal pesa menos de 400 KB.
- [ ] Un borrador **no** se ve en el portal, ni por su dirección directa ni su foto.
- [ ] Despublicar o archivar la quita del portal al momento.
- [ ] Un vídeo no carga nada de YouTube/Vimeo hasta que se pulsa "Reproducir" (comprobado en la pestaña Red).
- [ ] Un texto con `<script>` o HTML a mano se muestra como texto, nunca se ejecuta.
- [ ] Cada rol solo puede hacer lo de la tabla de permisos (comprobado contra la API, no solo ocultando botones).
- [ ] `/news` y el detalle se ven bien en móvil (375 px) y en tema oscuro; las imágenes tienen texto alternativo.
- [ ] Con el interruptor apagado, el enlace Noticias no aparece en el portal.
- [ ] Cada acción del admin queda en el registro de auditoría con autor y fecha.
- [ ] `/cookies` y `docs/COOKIES_Y_PRIVACIDAD.md` actualizados si se elige el reproductor de vídeo dentro del portal.

## 11. Entregas propuestas

Una rama, un commit por parte, como en los retos:

1. **Modelo y API:** tabla, migración, endpoints del admin y del portal, almacenamiento de fotos y permisos.
2. **Admin:** menú Noticias, listado, formulario con subida de foto, vista previa y acciones.
3. **Portal:** `/news`, `/news/{slug}`, interruptor y estado vacío.
4. **Vídeo y textos legales:** reproductor de dos pasos (o enlace, según D3), CSP y `/cookies`.

Al cerrar: probar en local de punta a punta (crear, publicar, ver en el portal, despublicar) antes de fusionar con `main`.
