# OPENTOWORK

Plataforma de **evaluacion, validacion y scoring de talento** que funciona como la capa de confianza para decisiones de contratacion. Construida con **.NET 10, Blazor y MySQL**.

> Ver `docs/BUSINESS_PROPOSAL.md` para la propuesta de negocio completa.
> Ver `docs/PLAN_DE_PROYECTO.md` para el plan detallado de fases y 3 portales.

---

## Introduccion

OpenToWork no es una bolsa de empleo mas. Es una plataforma que genera **perfiles profesionales validados** con indices de confiabilidad, estabilidad y evidencia, permitiendo a las empresas identificar rapidamente a los candidatos mas confiables y mejor preparados.

El proyecto se compone de **3 portales independientes**:

| Portal | Descripcion | Estado |
|--------|-------------|--------|
| **Portal de Candidatos** | Registro, perfil, wizard, busqueda de vacantes, postulaciones | 80% Completado |
| **Portal Administrativo** | Verificaciones manuales, moderacion, gestion de usuarios, auditoria, pipeline de reclutamiento, CRM de captacion de empresas | 95% Completado + Pipeline de Reclutamiento (21-Ago) + CRM de Empresas (06-Sep) |
| **Portal Corporativo** | Dashboard, vacantes, postulantes, perfil de candidato, mensajeria | 70% Completado (estructura base en OpenToWork.WEB) — pendiente scoring/suscripciones (Fase 3) |

### Caracteristicas principales

- **Autenticacion JWT** con registro, login, refresh tokens y device fingerprinting
- **Wizard de registro** multi-paso (10 pasos) para completar el perfil del candidato
- **Busqueda de vacantes** con filtros (texto, ubicacion, tipo de contrato, salario)
- **Dashboard con Bento Grid** estilo Samsung One UI
- **Sistema de temas** dinamicos (navy, dark, light) con CSS variables
- **Internacionalizacion (i18n)** con Espanol e Ingles, archivos JSON, sin texto hardcoded
- **Soft delete** en todas las tablas (auditoria completa: CreatedAt, UpdatedAt, IsDeleted, etc.)
- **Motor de evaluacion** con 4 indices: Estabilidad, Confiabilidad, Evidencia, Compatibilidad (Fase 3)
- **Sistema de verificaciones** con checkmarks: identidad, LinkedIn, experiencia, portafolio, referencias (Fase 3)
- **CRM de Captacion de Empresas** — pipeline comercial con 6 etapas, wizard progresivo, mapa interactivo para ubicacion, planes (Basic/Premium/Platinum), generacion de contratos
- **Embudo Ciego** — la empresa nunca ve candidatos ni identidades, solo el conteo de postulantes por vacante; Trato Directo investiga/evalua/entrevista en el pipeline y luego *entrega* personal verificado a una vacante concreta (`PTCandidateDelivery`); la empresa responde Interesado/Contratado/No encaja sobre lo entregado

---

## Diagramas y Documentacion Visual

- **[Del Lead al Contrato](https://claude.ai/code/artifact/ccf43c05-56ad-4df3-a6f7-dff549eb7f96)** (Dsiezar, 13-Sep) — infografia del ciclo administrativo completo: pipeline comercial de empresas (Lead → Cerrado Ganado), pipeline de reclutamiento y verificacion de candidatos ("Verificado TD"), y como convergen ambos flujos al entregar un candidato y cerrar la negociacion de una vacante. Util para Iluna como referencia visual de como su CRM de Empresas y su Pipeline de Reclutamiento se conectan con el flujo de Negociaciones.
- **[Puntos Ciegos de Trato Directo](https://claude.ai/code/artifact/8ca52a52-4ea4-41f3-9102-4fac81b54d93)** (Dsiezar, 13-Sep) — version visual del gap-analysis de la seccion de abajo ("Auditoria del Ciclo Comercial Completo"): compara el flujo de negocio oficial del dueno (22 pasos, de la captacion del cliente a la garantia de reposicion) contra lo que el sistema realmente rastrea hoy.

---

## Auditoria del Ciclo Comercial Completo (Gap Analysis)

> **Contexto:** Darwin comparto el flujo de negocio OFICIAL de Trato Directo (22 pasos, 4 fases: Captacion del Cliente → Reclutamiento → Contratacion y Pago → Seguimiento y Garantia) para auditar que tanto de eso ya construyo el sistema vs. que sigue siendo 100% manual. Version visual con notas: **[Puntos Ciegos de Trato Directo](https://claude.ai/code/artifact/8ca52a52-4ea4-41f3-9102-4fac81b54d93)**.
>
> **Como usar esto:** cada item marcado `[ ]` es una pieza real de negocio que hoy NO tiene ningun soporte en el codigo (no es una tarea tecnica generica, es un paso que el dueno del negocio necesita que el sistema sepa que paso). A medida que se construya cada uno, marcarlo `[x]` aqui y actualizar/republicar el artifact de arriba para que Iluna y Darwin vean el avance real.
>
> **Estado al 18-Sep-2026:** 25 construidos / 0 parciales / 0 faltantes (de los 22 pasos + 3 sub-pasos de reposicion) — **Auditoria del Ciclo Comercial completa**. De las "Politicas del documento oficial", tambien se cerro el ultimo bullet abierto: Validacion y Consolidacion quedan a criterio manual del admin, decision explicita de Darwin (no se automatiza su disparo). Ultima actualizacion: paso 7 re-evaluado (ya estaba resuelto por `CompatibilityService`, la evaluacion anterior miraba el servicio equivocado) y fecha de contratacion candidato-empresa (paso 16, alcance recortado por decision de Darwin — TD no formaliza el contrato laboral, solo registra la fecha).

### A. Captacion y Contratacion del Cliente

- [x] 1. Inicio: Gestion Comercial — pipeline comercial (`Lead → Contactado → Reunion → Propuesta → Negociacion → Cerrado Ganado`) con historial de etapas
- [x] 2. Presentacion y Diagnostico — etapa "Contactado" + notas de la empresa
- [x] 3. Propuesta de Servicio y Condiciones — etapa "Propuesta Enviada"; split 30/50/20 ya modelado en `PTVacancyContract`. **Refinado 17-Sep:** los precios ofrecidos en esa etapa ahora salen del catalogo real de Precios y Niveles de Precio (antes eran 3 planes fijos de `PT_Plans` sin relacion con los precios reales de los contratos) — ver Bitacora
- [x] 4. Firma Contrato de Servicio — estados Draft/Sent/Accepted/Rejected. **Refinado 17-Sep:** "Generar Contrato" en la etapa "Cerrado Ganado" del pipeline ahora lleva directo a crear el contrato (`/vacancies/{id}/contract`) cuando todavia no existe uno, en vez de solo mostrar un error — ver Bitacora
- [x] 5. Cliente paga primer 30% (gate "¿pago activado?") — **construido 13-Sep**: entidad `PTContractPayment` (tramos Apertura/Validacion/Consolidacion, auto-generados al aceptar el contrato); `PermanentVacancyService.PublishVacancyAsync` bloquea la publicacion de la vacante (con `InvalidOperationException` mostrado al cliente) hasta que un admin marca la Apertura como pagada en `/vacancies/{id}/contract`
- [x] 6. Briefing y Perfil — cubierto por los campos de la vacante (requisitos, horario, salario). **Refinado 17-Sep:** ahora es un paso obligatorio del pipeline (gate al avanzar de "Reunion Agendada" a "Propuesta Enviada"), antes existia pero estaba desconectado del flujo de ventas — ver Bitacora

### B. Reclutamiento y Seleccion

- [x] 7. Consulta Base de Datos Prevalidada (filtrada por la vacante) — **re-evaluado 18-Sep, ya estaba construido**: la evaluacion anterior miraba el servicio equivocado (`CandidateSearchService`, generico, portal de empresa). El que realmente resuelve este paso es `CompatibilityService.CalculateMatchesForVacancyAsync`/`GetNonApplicantMatchesAsync` — consulta exactamente la base prevalidada (`IsProfilePublic && WizardCompleted`) filtrada por los requisitos puntuales de la vacante (skills requeridas/opcionales, experiencia, ubicacion), en el admin, pestaña "Cumplen sin postularse" de la ficha de vacante (`/vacancies/{id}`), con filtro de % minimo de match y boton para postular directamente a quien califique. Verificado en vivo: 2 candidatos encontrados con desglose real de skills/experiencia/ubicacion contra una vacante de prueba
- [x] 8. Busqueda Activa y Atraccion — vacantes publicas en el portal + postulacion directa del candidato
- [x] 9. Preseleccion y Filtro Inicial — etapa "Postulacion" del pipeline de reclutamiento
- [x] 10. Entrevistas y Evaluaciones — Evaluacion Tecnica + Entrevista Cultural con score y recomendacion
- [x] 11. Verificacion de Referencias y Documentacion — checklist de investigacion (LinkedIn, portafolio, certificados, referencias laborales)
- [x] 12. Shortlist Final — modulo de Negociaciones arma el shortlist por vacante (`PTNegotiation`)
- [x] 13. Presentacion al Cliente — estado "Presentada" de la negociacion

### C. Contratacion y Pago

- [x] 14. Seleccion del Candidato por el Cliente — `NegotiationService.CloseAsync` acepta al ganador, rechaza al resto, cierra la vacante
- [x] 15. Cliente paga segundo 50% — **construido 13-Sep**: tramo "Validacion" trackeado (pagado/pendiente, con nota y quien lo marco) en el mismo panel del contrato. No bloquea nada (a diferencia del 30%, tu diagrama no tiene un gate aqui) — es un admin quien lo marca a mano
- [x] 16. Contratacion Laboral candidato-empresa (formal desde el Dia 1) — **construido 18-Sep, alcance recortado por decision de Darwin**: TD no formaliza ni gestiona ese contrato laboral (es exclusivamente entre la empresa y el candidato); el sistema solo registra `HiringDate` en `PTNegotiation`/`PTCandidateDelivery` — la fecha en que la empresa contrata formalmente, independiente de `IncorporationDate` (primer dia de trabajo)
- [x] 17. Incorporacion del Candidato — **construido 13-Sep**: campo `IncorporationDate` en `PTNegotiation` (flujo de Negociaciones) y en `PTCandidateDelivery` (Embudo Ciego) - los dos caminos de "hire" que existen en el sistema, ver nota debajo. Un admin la registra desde `/vacancies` (negociacion cerrada) o desde la ficha del candidato (entrega en estado Contratado)
- [x] 18. Cliente paga ultimo 20% (30 dias post-incorporacion) — **construido 13-Sep**: tramo "Consolidacion" trackeado igual que el 15. Sigue faltando el disparo automatico a los 30 dias (depende del paso 17, fecha de incorporacion, que todavia no existe) — por ahora un admin lo marca pagado cuando corresponda

### D. Seguimiento y Garantia

- [x] 19. Seguimiento Durante el Periodo de Garantia — **construido 13-Sep** (visibilidad, no bitacora de contacto): `WarrantyCalculator` calcula en vivo Activa/Por vencer/Vencida a partir de `IncorporationDate` + `WarrantyDays` del contrato, mostrado como badge junto a cada negociacion/entrega. Sigue faltando el registro de contacto activo con cliente/candidato y el gate explicito "¿continua?" que dispara la reposicion (eso es el paso 20, ver abajo)
- [x] 20. Activacion de Garantia de Reposicion — **construido 16-Sep**: entidad `PTWarrantyReplacement` + boton "Activar Garantia de Reposicion" junto al badge de garantia (en `/vacancies` para Negociaciones y en la ficha de candidato para Entregas). Sigue siendo activacion manual (no hay `BackgroundService` que vigile vencimientos, ver alcance abajo)
- [x] 20.1 Analisis de Causa Raiz — **construido 16-Sep**: 10 motivos estructurados en `WarrantyReplacementReason` (6 cubiertos por la garantia + 4 exclusiones contractuales) + notas libres, en vez de texto libre sin estructura
- [x] 20.2 Nueva Busqueda Sin Costo (1a reposicion) — **construido 16-Sep**: activar la reposicion reabre la vacante (`Status=Active`) para una nueva busqueda; cuando la nueva negociacion/entrega cierra, se "Vincula" desde el panel del contrato, dejando el registro real que faltaba entre la reposicion y el nuevo proceso de reclutamiento
- [x] 20.3 Segunda Reposicion (50% del valor) — **construido 16-Sep**: la 2a reposicion sobre la misma vacante genera automaticamente un cargo del 50% del `FeeAmount` (reutiliza el mecanismo de tramos de pago existente, `PaymentTrancheType.ReposicionSegunda`), visible en el mismo panel "Tramos de Pago". Un 3er intento sobre la misma vacante es rechazado (tope de 2 reposiciones cubiertas)
- [x] 21. Cierre del Proceso (post-garantia) — **construido 18-Sep**: `ProcessClosedAt`/`ProcessClosedByUserId`/`ProcessClosureNotes` en `PTNegotiation`/`PTCandidateDelivery`. Boton "Cerrar Proceso" junto al badge de garantia, habilitado solo cuando el proceso esta Cerrada/Hired, la garantia ya vencio (o no hay garantia definida) y no hay una reposicion en curso sin resolver
- [x] 22. Feedback y Mejora Continua — **construido 18-Sep**: `FeedbackRating` (1-5)/`FeedbackComments`/`FeedbackRecordedAt` en las mismas entidades. Boton "Registrar Feedback", habilitado solo una vez cerrado el proceso (paso 21) — antes no existia ningun registro de satisfaccion del cliente

### Politicas del documento oficial (paneles laterales)

- [x] Garantia por tipo de perfil (Operativo=30d / Encargados-Tecnicos=45d / Responsables-Cualificados=60d) — **corregido 18-Sep**: `/pricing/job-levels` tenia Encargados y Tecnicos en 40 dias (no 45); corregido vía UI a 30/45/60, coincide con el documento oficial
- [x] Exclusiones de Garantia (impago de nomina, cambio sustancial de condiciones, cierre del negocio, incumplimiento normativo) — **construido 16-Sep**: las 4 causales son motivos estructurados de `WarrantyReplacementReason` (`ImpagoDeNomina`/`CambioSustancialDeCondiciones`/`CierreDelNegocio`/`IncumplimientoNormativo`); al elegir una, la reposicion queda `ExcluidaDeGarantia` (no cuenta como 1a/2a, no reabre la vacante). El campo de texto libre ("Excepciones pactadas") del contrato sigue existiendo aparte, para condiciones no cubiertas por estas 4
- [x] "¿Y si el candidato...?" (roba/falta grave, baja medica, renuncia voluntaria) — **construido 16-Sep**: son 3 de los 6 motivos cubiertos de `WarrantyReplacementReason` (`FaltaGraveORobo`/`BajaMedica`/`RenunciaVoluntaria`), quedan registrados igual que cualquier otra causa de reposicion
- [x] 30/50/20 ligado a eventos reales del sistema — **cerrado 18-Sep, decision explicita de Darwin**: la Apertura (30%) esta ligada a un evento real (bloquea publicar la vacante). Validacion y Consolidacion quedan a criterio manual del admin (no se automatiza su disparo al cerrar la negociacion ni a los 30 dias de incorporacion) — los 3 tramos siguen siendo registros reales pagado/pendiente, no solo porcentajes

---

## Estructura del Proyecto

```
OpenToWork/
├── src/
│   ├── OpenToWork.API/          # API REST del portal de candidatos (puerto 5000)
│   ├── OpenToWork.AdminAPI/     # API REST del portal administrativo (puerto 5001)
│   ├── OpenToWork.CorporateAPI/ # API REST del portal corporativo (puerto 5002) [Fase 5]
│   ├── OpenToWork.WEB/          # Blazor Server del portal de candidatos (puerto 5100)
│   ├── OpenToWork.AdminWEB/     # Blazor Server del portal administrativo (puerto 5101)
│   ├── OpenToWork.CorporateWEB/ # Blazor Server del portal corporativo (puerto 5102) [Fase 5]
│   ├── OpenToWork.SharedUI/     # Razor Class Library con componentes compartidos
│   ├── OpenToWork.Core/         # Logica de negocio, scoring y validacion
│   ├── OpenToWork.Models/       # Entidades EF Core y AppDbContext
│   └── OpenToWork.Shared/       # DTOs, Enums y constantes
├── docs/                        # Documentacion completa del proyecto
└── OpenToWork.slnx              # Solucion (.slnx)
```

### Referencias entre proyectos

```
API / AdminAPI / CorporateAPI  ->  Core  ->  Models  ->  (EF Core, Pomelo MySQL)
                                 ->  Shared
WEB / AdminWEB / CorporateWEB   ->  SharedUI
                                 ->  Core (via API HTTP)
                                 ->  Shared
```

---

## Stack Tecnologico

| Componente | Tecnologia |
|---|---|
| Backend | C# .NET 8, ASP.NET Core Web API |
| Frontend | Blazor Server |
| ORM | Entity Framework Core 8 + Pomelo MySQL |
| Base de datos | MySQL 8.0+ |
| Autenticacion | JWT Bearer + Refresh Tokens |
| UI | CSS puro con variables, Bento Grid, sin Bootstrap |
| i18n | JSON files + LanguageService |

---

## Como ejecutar

### Requisitos previos

- .NET 10 SDK
- MySQL 8.0+ corriendo en localhost:3306
- (Opcional) Visual Studio 2022 o VS Code
- Google OAuth credentials (opcional, para login con Google)
- reCAPTCHA keys (opcional, para proteccion anti-bot)

### 1. Clonar el repositorio

```bash
git clone https://github.com/lunagonzalezivan85/OpenToWork.git
cd OpenToWork
```

### 2. Base de datos

Crear la base de datos en MySQL:

```sql
CREATE DATABASE OpenToWorkDb CHARACTER SET utf8mb4;
```

Aplicar todas las migraciones con EF Core (incluye Fase 1 y Fase 2):

```bash
dotnet ef database update --project src/OpenToWork.Models --startup-project src/OpenToWork.Models
```

### 3. Configurar connection string y claves

Editar `src/OpenToWork.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=OpenToWorkDb;User=root;Password=TU_PASSWORD;CharSet=utf8mb4;"
  },
  "Jwt": {
    "Key": "TU_JWT_KEY_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "OpenToWork.API",
    "Audience": "OpenToWork.WEB"
  },
  "Google": {
    "ClientId": "TU_GOOGLE_CLIENT_ID",
    "ClientSecret": "TU_GOOGLE_CLIENT_SECRET"
  },
  "Recaptcha": {
    "SiteKey": "TU_RECAPTCHA_SITE_KEY",
    "SecretKey": "TU_RECAPTCHA_SECRET_KEY"
  }
}
```

Editar `src/OpenToWork.WEB/appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5000/"
  },
  "Security": {
    "AesKey": "TU_AES_KEY_PARA_ENCRYPTAR_TOKENS"
  },
  "Recaptcha": {
    "SiteKey": "TU_RECAPTCHA_SITE_KEY"
  }
}
```

### 4. Build

Compilar toda la solucion:

```bash
dotnet build OpenToWork.slnx
```

### 5. Ejecutar

Necesitas dos terminales abiertas:

**Terminal 1 - API (puerto 5000):**

```bash
dotnet run --project src/OpenToWork.API
```

- Swagger: `http://localhost:5000/swagger`
- Endpoints de auth: `http://localhost:5000/api/auth/*`
- Endpoints de vacantes: `http://localhost:5000/api/permanentvacancies/*`
- Endpoints de solicitudes: `http://localhost:5000/api/applications/*`
- Endpoints de perfil: `http://localhost:5000/api/profile/*`

**Terminal 2 - WEB Blazor Server (puerto 5100):**

```bash
dotnet run --project src/OpenToWork.WEB
```

- Portal principal: `http://localhost:5100`
- Login: `http://localhost:5100/login`
- Registro: `http://localhost:5100/register`
- Recuperar contrasena: `http://localhost:5100/forgot-password`
- Dashboard: `http://localhost:5100/dashboard`
- Vacantes: `http://localhost:5100/vacancies`
- Mis Vacantes: `http://localhost:5100/myvacancies`
- Perfil: `http://localhost:5100/profile`
- Wizard (10 pasos): `http://localhost:5100/wizard`

**Terminal 3 - AdminAPI (puerto 5001):**

```bash
dotnet run --project src/OpenToWork.AdminAPI
```

- Swagger: `http://localhost:5001/swagger`
- Login admin: `POST http://localhost:5001/api/admin/auth/login`
- Dashboard: `GET http://localhost:5001/api/admin/dashboard/metrics`
- Usuarios: `GET http://localhost:5001/api/admin/users`
- Vacantes: `GET http://localhost:5001/api/admin/vacancies`
- Skills: `GET http://localhost:5001/api/admin/skills`
- Auditoria: `GET http://localhost:5001/api/admin/audit-log`
- Export CSV: `GET http://localhost:5001/api/admin/export/users`

**Terminal 4 - AdminWEB Blazor Server (puerto 5101):**

```bash
dotnet run --project src/OpenToWork.AdminWEB
```

- Portal admin: `http://localhost:5101`
- Login: `http://localhost:5101/login`
- Dashboard: `http://localhost:5101/`
- Usuarios: `http://localhost:5101/users`
- Vacantes: `http://localhost:5101/vacancies`
- Skills: `http://localhost:5101/skills`
- Auditoria: `http://localhost:5101/audit-log`

**Credenciales de prueba (Portal Admin):**

| Campo | Valor |
|-------|-------|
| URL | `http://localhost:5101` |
| Email | `admin@opentowork.com` |
| Password | `Admin123!` |

> **Nota:** El usuario admin debe tener `PrimaryRole = 2` (Admin) en `SC_Users`. Para crearlo, registra un usuario via la API principal y luego actualiza el rol en MySQL:
> ```sql
> UPDATE SC_Users SET PrimaryRole = 2 WHERE Email = 'admin@opentowork.com';
> ```

### 5.1. Datos de prueba (Seed Data - Hosteleria)

El script [`docs/seed-data.sql`](docs/seed-data.sql) puebla la base de datos con datos de ejemplo del ramo de **hosteleria** (hoteles, restaurantes, catering). Ejecutar despues de aplicar todas las migraciones:

```bash
mysql -u root -p OpenToWorkDb < docs/seed-data.sql
```

**Password para TODOS los usuarios de prueba: `Empresa123!`**

#### Empresas (3)

| Email | Empresa | Pais | Ciudad | Tamano | Verificada |
|-------|---------|------|--------|--------|-----------|
| `rrhh@hotelsolcaribe.com` | Hotel Sol Caribe | Colombia | Cartagena | 220 | Si |
| `rrhh@grupolapaella.es` | Grupo La Paella | Espana | Madrid | 160 | Si |
| `rrhh@cateringdelmar.es` | Catering Del Mar | Espana | Barcelona | 90 | No |

#### Candidatos (8)

| Email | Nombre | Titulo | Pais | Ciudad | Exp. (anos) |
|-------|--------|--------|------|--------|-------------|
| `ana.martinez@gmail.com` | Ana Martinez | Chef de Parte - Cocina Internacional | Espana | Madrid | 5 |
| `luis.fernandez@hotmail.com` | Luis Fernandez | Recepcionista Hotelero Bilingue | Colombia | Cartagena | 4 |
| `sofia.torres@outlook.com` | Sofia Torres | Camarera Profesional y Sumiller | Espana | Barcelona | 6 |
| `javier.morales@outlook.com` | Javier Morales | Cocinero - Cocina Mediterranea | Colombia | Cartagena | 3 |
| `elena.ruiz@gmail.com` | Elena Ruiz | Gobernanta de Hotel | Espana | Madrid | 7 |
| `pablo.garcia@hotmail.com` | Pablo Garcia | Barista y Camarero de Cafeteria | Espana | Barcelona | 4 |
| `carmen.vega@outlook.com` | Carmen Vega | Pastelera - Reposteria de Autor | Espana | Barcelona | 5 |
| `diego.hernandez@gmail.com` | Diego Hernandez | Maitre de Sala - Restauracion | Colombia | Cartagena | 8 |

#### Vacantes permanentes (8)

| Empresa | Vacante | Estado | Categoria |
|---------|---------|--------|-----------|
| Hotel Sol Caribe | Chef de Parte - Cocina Internacional | Activa | Cocina |
| Hotel Sol Caribe | Recepcionista de Hotel Bilingue | Activa | Recepcion |
| Hotel Sol Caribe | Gobernante/a de Hotel (Housekeeping) | Activa | Housekeeping |
| Grupo La Paella | Camarero/a de Sala - Restaurante Gourmet | Activa | Sala |
| Grupo La Paella | Segundo/a de Cocina (Sous Chef) | Activa | Cocina |
| Catering Del Mar | Camarero/a de Eventos y Banquetes | Activa | Banquetes |
| Catering Del Mar | Pastelero/a - Produccion de Reposteria | Borrador | Pasteleria |
| Grupo La Paella | Jefe/a de Sala | Cerrada | Sala |

#### Vacantes temporales (2)

| Empresa | Vacante | Tipo | Ubicacion |
|---------|---------|------|-----------|
| Hotel Sol Caribe | Extra de Sala - Temporada Alta | Contrato | Cartagena |
| Grupo La Paella | Cocinero/a de Refuerzo - Eventos | Contrato | Madrid |

#### Skills (20)

Cocina: Cocina Internacional, Cocina Mediterranea, Reposteria y Pasteleria, HACCP / Seguridad Alimentaria, Cocina Creativa / Autor
Sala: Servicio de Sala, Maridaje de Vinos, Banquetes y Eventos, Sommellerie, Maitre d Hotel
Barra: Cocteleria, Cafe y Barista, Barista / Cafeteria
Recepcion: Gestion de PMS (Opera/Sihot)
Housekeeping: Housekeeping / Limpieza
Idiomas: Ingles B2, Ingles C1
Gestion: Gestion de Equipos, Control de Costes
Transversal: Atencion al Cliente

#### Postulaciones (10)

| Candidato | Vacante | Estado |
|-----------|---------|--------|
| Ana Martinez | Chef de Parte - Cocina Internacional | En revision |
| Luis Fernandez | Recepcionista de Hotel Bilingue | Pendiente |
| Sofia Torres | Camarero/a de Sala - Restaurante Gourmet | Pendiente |
| Javier Morales | Chef de Parte - Cocina Internacional | Pendiente |
| Ana Martinez | Camarero/a de Eventos y Banquetes | Aceptada |
| Elena Ruiz | Gobernante/a de Hotel (Housekeeping) | En revision |
| Pablo Garcia | Camarero/a de Sala - Restaurante Gourmet | Pendiente |
| Carmen Vega | Pastelero/a - Produccion de Reposteria | Pendiente |
| Diego Hernandez | Camarero/a de Sala - Restaurante Gourmet | En revision |
| Sofia Torres | Camarero/a de Eventos y Banquetes | Rechazada |

### 6. Flujo de trabajo con Git

Antes de comenzar cualquier tarea, **siempre** seguir este checklist:

1. **Validar la fecha actual** — confirmar que se trabaja en el dia correspondiente.
2. **Crear una nueva rama** para la tarea o feature:
   ```bash
   git checkout -b feature/nombre-descriptivo
   ```
3. **Validar siempre cambios pendientes de todos** — antes de empezar, revisar si hay cambios sin commitear (propios o de otros agentes):
   ```bash
   git status
   git log --oneline -5
   ```
   Si hay cambios pendientes, commitearlos o coordinar antes de continuar.
4. **Commitear frecuentemente** con mensajes descriptivos.
5. **Hacer push al finalizar** la tarea:
   ```bash
   git add .
   git commit -m "feat: descripcion del cambio"
   git push origin feature/nombre-descriptivo
   ```

### 7. Migraciones (solo si se modifican entidades)

Crear nueva migracion:

```bash
dotnet ef migrations add NombreMigracion --project src/OpenToWork.Models --startup-project src/OpenToWork.Models
```

Aplicar migracion:

```bash
dotnet ef database update --project src/OpenToWork.Models --startup-project src/OpenToWork.Models
```

### 8. Estructura de puertos

| Proyecto | Puerto | Descripcion |
|---|---|---|
| OpenToWork.API | 5000 | API REST del portal de candidatos |
| OpenToWork.WEB | 5100 | Blazor Server del portal de candidatos |
| OpenToWork.AdminAPI | 5001 | API REST del portal administrativo (Fase 4) |
| OpenToWork.AdminWEB | 5101 | Blazor Server del portal administrativo (Fase 4) |
| OpenToWork.CorporateAPI | 5002 | API REST del portal corporativo (Fase 5) |
| OpenToWork.CorporateWEB | 5102 | Blazor Server del portal corporativo (Fase 5) |

---

## Fases del Proyecto

> Ver `docs/PLAN_DE_PROYECTO.md` para el detalle completo de cada fase.

### Fase 1: Fundacion - COMPLETADA

- [x] Estructura de 8 proyectos creada
- [x] Entidades EF Core con prefijos (SC_, PT_, SY_) y auditoria
- [x] AppDbContext con configuracion MySQL
- [x] DTOs y Enums en Shared
- [x] Servicios de autenticacion (register, login, JWT, refresh, device fingerprinting)
- [x] Controllers de Auth, Candidates y Vacancies
- [x] Componentes SharedUI (BentoCard, OTButton, OTInput, Wizard, ThemeSwitcher, LanguageSwitcher)
- [x] Sistema de temas (navy, dark, light) con CSS variables
- [x] Sistema de i18n (es/en) con archivos JSON
- [x] Paginas: Home, Login, Register, Wizard, Dashboard, Vacancies
- [x] Migracion inicial aplicada a MySQL

### Fase 2: Portal de Candidatos - COMPLETADA

- [x] Vacantes permanentes (empresas)
- [x] Sistema de solicitudes (aplicar a vacantes)
- [x] Gestion de estados de solicitud (Pendiente, En revision, Aceptada, Rechazada)
- [x] Perfil completo del candidato (skills, experiencia, educacion, certificaciones)
- [x] Subida de CV (URL)
- [x] Login con Google OAuth
- [x] reCAPTCHA en login desde dispositivo desconocido
- [x] Encriptacion de datos de sesion en localStorage (AES-256)
- [x] Recuperacion de contrasena
- [x] Wizard pasos 7-10 (experiencia, educacion, certificaciones, CV)
- [x] Migracion Phase2 + Phase2Security aplicada
- [x] i18n completo (es + en) con claves nuevas
- [x] UI/UX: One UI, Bento Grid, Command-Driven, temas (navy/dark/light)

### Fase 3: Motor de Evaluacion y Scoring Automatico - COMPLETADA (Dsiezar, 01-Sep-2026)

> **Actualizacion (01-Sep-2026, Dsiezar):** Las 8 sub-fases de este plan se completaron en orden estricto (3.1 → 3.8), cada una con sus preguntas respondidas y documentadas en `docs/dsiezar/fase-3-sub1.md` a `fase-3-sub8.md` **antes** de escribir codigo, tal como exige la instruccion obligatoria de abajo. Rama `dsiezar-fase-3` (8 commits, `e746f21`..`85bf4ea`), pusheada a origin, pendiente de PR/revision de Iluna. El plan detallado de cada sub-fase se deja intacto abajo como documentacion de los requisitos originales - ver la Bitacora de Cambios para el resumen de que se construyo en cada una.

> **⚠️ INSTRUCCION OBLIGATORIA PARA DARWIN (Dsiezar / IA):**
>
> Este es el plan oficial y obligatorio para construir el Motor de Scoring Automatico. **Darwin debe seguir este plan paso a paso, en el orden indicado.** Cada sub-fase genera preguntas que deben responderse ANTES de escribir codigo — las respuestas definen los algoritmos de calculo automatico.
>
> **Regla:** No se puede saltar sub-fases. Cada sub-fase debe estar 100% completada (entidades + servicio + endpoint + UI basica) antes de pasar a la siguiente. Al final de cada sub-fase, documentar en `docs/dsiezar/fase-3-subN.md` las decisiones tomadas.
>
> **Rama obligatoria:** `dsiezar-fase-3`

#### Sub-fase 3.1: Entidades de Scoring + Migracion

**Objetivo:** Crear el modelo de datos que soporta todos los calculos automaticos.

**Entidades a crear:**

- `PTCandidateScore` — score intrinseco del candidato
  - `Id`, `PT_CandidateId` (FK), `StabilityIndex` (0-100), `ReliabilityIndex` (0-100), `EvidenceIndex` (0-100), `CompatibilityIndex` (0-100), `OverallScore` (0-100), `CalculatedAt` (DateTime), `Version` (int)
- `PTJobMatchScore` — score por par candidato-vacante
  - `Id`, `PT_CandidateId` (FK), `PT_VacancyId` (FK), `MatchPercentage` (0-100), `SkillsMatch` (int), `ExperienceMatch` (int), `EducationMatch` (int), `CalculatedAt`, `WeightsConfig` (JSON)
- `PTVerification` — verificaciones automaticas
  - `Id`, `PT_CandidateId` (FK), `Type` (enum: Identity=0, LinkedIn=1, Portfolio=2, CvCoherence=3, Education=4, Reference=5), `Status` (enum: Pending=0, InProgress=1, Verified=2, Failed=3), `VerifiedAt`, `Result` (JSON), `Score` (0-100)
- `PTCandidateReference` — referencias laborales del candidato
  - `Id`, `PT_CandidateId` (FK), `ContactName`, `CompanyName`, `Phone`, `Email`, `Relationship` (enum: Manager=0, Peer=1, Subordinate=2), `Status` (enum: Pending=0, Sent=1, Responded=2, Verified=3, Failed=4), `Rating` (1-5), `Feedback`
- `PTSkillTest` — banco de retos tecnicos
  - `Id`, `Category`, `Difficulty` (enum: Easy=0, Medium=1, Hard=2), `Title`, `Description`, `TimeLimit` (int minutos), `Questions` (JSON), `IsActive`
- `PTCandidateTestResult` — resultados de retos
  - `Id`, `PT_CandidateId` (FK), `PT_SkillTestId` (FK), `Score` (0-100), `TimeTaken` (int segundos), `CompletedAt`, `AntiCheatFlags` (int)

**Migracion:** `ScoringEngine` — crea las 6 tablas con indices en `PT_CandidateId` y `PT_VacancyId`.

**Preguntas que Darwin debe responder antes de codificar (respuestas en `docs/dsiezar/fase-3-sub1.md`):**

1. ¿El `OverallScore` se almacena como un campo calculado en la tabla, o se calcula on-the-fly cada vez que se consulta? ¿Por que?
2. ¿Que estrategia se usa para el versionado (`Version`)? ¿Incremental por recalculo, o timestamp?
3. ¿`PTJobMatchScore.WeightsConfig` que formato JSON debe tener? Definir el schema exacto.
4. ¿Las verificaciones (`PTVerification`) se insertan automaticamente al crear un candidato, o se disparan bajo demanda?
5. ¿`PTCandidateReference` tiene soft delete o se elimina fisicamente?
6. ¿`PTSkillTest.Questions` que estructura JSON debe tener? ¿Multiple choice, codigo, o ambos?
7. ¿Se necesita una entidad `PTScoreWeight` configurable por el admin, o los pesos van hardcodeados en el ScoringService?

---

#### Sub-fase 3.2: ValidationService — Verificaciones Automaticas

**Objetivo:** Sistema que verifica datos del candidato sin intervencion humana.

**Metodos a implementar:**

- `VerifyLinkedInAsync(candidateId)` — valida que la URL de LinkedIn existe y tiene el formato correcto del candidato
- `VerifyPortfolioAsync(candidateId)` — hace HTTP GET a la URL del portfolio y verifica que responde 200
- `VerifyCvCoherenceAsync(candidateId)` — analiza coherencia cronologica entre experiencias (gaps > 6 meses, superposiciones, fechas imposibles)
- `VerifyIdentityAsync(candidateId)` — validacion de documento subido (formato, legibilidad)
- `DetectRedFlagsAsync(candidateId)` — saltos laborales < 3 meses, cambios de sector frecuentes, gaps inexplicables
- `RunAllVerificationsAsync(candidateId)` — ejecuta todas las verificaciones y guarda resultados en `PTVerification`

**Endpoint:** `POST api/candidates/{id}/verifications/run` — dispara todas las verificaciones

**Preguntas que Darwin debe responder antes de codificar (respuestas en `docs/dsiezar/fase-3-sub2.md`):**

1. ¿La verificacion de LinkedIn hace un scraping real de la pagina, o solo valida que la URL responde y tiene el formato `linkedin.com/in/{slug}`?
2. ¿La verificacion de portfolio tiene timeout? ¿Cuanto? ¿Que pasa si responde 403 o 401?
3. ¿Como se define un "gap inexplicable"? ¿Cuantos meses sin empleo se consideran un gap? ¿Se penaliza mas si es reciente o antiguo?
4. ¿Que se considera "superposicion sospechosa"? ¿Dos empleos simultaneos por mas de X meses?
5. ¿La verificacion de identidad que valida exactamente? ¿Formato de documento, OCR, o solo presencia del archivo?
6. ¿Cada cuanto se re-ejecutan las verificaciones automaticamente? ¿On-demand, diario, semanal?
7. ¿Si una verificacion falla, se reintenta automaticamente? ¿Cuantos reintentos, con que intervalo?
8. ¿El `Score` de cada verificacion (0-100) como se calcula? ¿Es binario (100 si pasa, 0 si falla) o hay matices?
9. ¿Que red flags se detectan exactamente? Definir la lista completa de reglas.
10. ¿Las red flags afectan el `ReliabilityIndex` o tienen un campo separado en `PTCandidateScore`?

---

#### Sub-fase 3.3: ScoringService — Indices Automaticos

**Objetivo:** Algoritmos que calculan los 4 indices del Candidate Score automaticamente.

**Metodos a implementar:**

- `CalculateStabilityIndex(candidate)` — analiza `PTCandidateExperience`:
  - Duracion promedio en empleos (mas duracion = mas estable)
  - Frecuencia de cambios (menos cambios = mas estable)
  - Penalizacion por empleos < 3 meses
  - Bonus por empleo actual > 12 meses
- `CalculateReliabilityIndex(candidate)` — analiza coherencia:
  - Coherencia cronologica entre experiencias (sin gaps ni superposiciones = 100)
  - Penalizacion por gaps > 6 meses sin explicacion
  - Penalizacion por superposiciones imposibles
  - Bonus por progresion logica (ascensos, misma industria)
- `CalculateEvidenceIndex(candidate)` — suma de verificaciones:
  - LinkedIn verificado = +25
  - Portfolio verificado = +25
  - CV subido y coherente = +25
  - Identidad verificada = +25
  - Si no tiene alguna verificacion, el indice es proporcional
- `CalculateCompatibilityIndex(candidate)` — matching de skills:
  - Compara skills del candidato vs. skills demandadas en vacantes activas
  - Mientras mas skills demandadas tenga el candidato, mayor el indice
  - Penalizacion si tiene skills que nadie demanda
- `CalculateOverallScore(candidate)` — promedio ponderado de los 4 indices
- `RecalculateAsync(candidateId)` — recalcula todos los indices y guarda en `PTCandidateScore`
- `RecalculateAllAsync()` — recalculo en lote para todos los candidatos

**Endpoint:** `POST api/candidates/{id}/score/recalculate` — recalcula score de un candidato

**Preguntas que Darwin debe responder antes de codificar (respuestas en `docs/dsiezar/fase-3-sub3.md`):**

1. ¿Que pesos tiene cada indice en el `OverallScore`? Definir los 4 pesos exactos (ej: Estabilidad 30%, Confiabilidad 25%, Evidencia 25%, Compatibilidad 20%).
2. ¿La duracion promedio en empleos como se pondera? ¿Es lineal o hay un techo (ej: 5+ anos = 100)?
3. ¿Cuantos cambios de empleo por ano se consideran "frecuentes"? ¿Como escala la penalizacion?
4. ¿Un gap de 6 meses se penaliza igual que uno de 2 anos? ¿O es proporcional?
5. ¿La "progresion logica" como se detecta automaticamente? ¿Que criterios objetivos usa el algoritmo?
6. ¿El `CompatibilityIndex` se calcula contra todas las vacantes activas, o solo las de la industria del candidato?
7. ¿Si no hay vacantes activas en el sistema, el `CompatibilityIndex` es 0, 50 (neutral), o se omite del calculo?
8. ¿El recalculo en lote (`RecalculateAllAsync`) se ejecuta via un job programado (Hangfire/Quartz) o manualmente desde el admin?
9. ¿Cada cuanto se debe recalcular el score automaticamente? ¿Diario, semanal, mensual?
10. ¿El score anterior se guarda para comparar (historico de scores) o se sobrescribe?
11. ¿El candidato puede ver el desglose de cada indice, o solo el `OverallScore`?
12. ¿Que pasa si un candidato no tiene experiencias cargadas? ¿StabilityIndex = 0, 50 (neutral), o N/A?

---

#### Sub-fase 3.4: CompatibilityService — Job Match Score

**Objetivo:** Algoritmo que calcula que tan compatible es un candidato con una vacante especifica.

**Metodos a implementar:**

- `CalculateJobMatch(candidateId, vacancyId)` — compara:
  - Skills requeridas vs. skills del candidato (peso configurable)
  - Experiencia requerida vs. anos de experiencia del candidato
  - Educacion requerida vs. educacion del candidato
  - Ubicacion / modalidad (remoto, hibrido, presencial)
  - Nivel de ingles u otros idiomas
- `GenerateShortlist(vacancyId)` — ranking automatico de candidatos por match score
- `GenerateShortlist(vacancyId, limit)` — top N candidatos para una vacante

**Endpoints:**
- `POST api/vacancies/{id}/matches/calculate` — calcula matches para una vacante
- `GET api/vacancies/{id}/matches` — lista de candidatos rankeados
- `GET api/vacancies/{id}/matches?limit=10` — top 10 candidatos

**Preguntas que Darwin debe responder antes de codificar (respuestas en `docs/dsiezar/fase-3-sub4.md`):**

1. ¿Los pesos del Job Match Score son fijos o configurables por la empresa? Si son configurables, ¿que valores puede ajustar?
2. ¿El matching de skills es binario (tiene/no tiene) o ponderado por `ProficiencyLevel`?
3. ¿Si una vacante requiere 5 anos de experiencia y el candidato tiene 3, el `ExperienceMatch` es 60% (3/5), 0%, o hay una curva?
4. ¿La ubicacion geografica como se compara? ¿Exacta, por pais, por region?
5. ¿El nivel de ingles se valida contra un campo del candidato o se infiere de las experiencias?
6. ¿El shortlist se genera automaticamente al crear una vacante, o lo dispara el admin/TD?
7. ¿Cuantos candidatos aparecen en el shortlist por defecto? ¿Es configurable?
8. ¿El `MatchPercentage` se recalcula si el candidato actualiza su perfil despues de que se genero el match?
9. ¿Se necesita un endpoint para que TD apruebe/rechaze matches antes de que lleguen a la empresa? (ver "Nueva feature de Admin" en la definicion estrategica)
10. ¿La empresa puede ver el desglose del match (skills, experiencia, educacion) o solo el porcentaje total?

---

#### Sub-fase 3.5: Referencias Laborales Automaticas

**Objetivo:** Sistema de referencias donde el candidato agrega contactos y el sistema los verifica.

**Metodos a implementar:**

- `AddReferenceAsync(candidateId, dto)` — candidato agrega 2-3 referencias
- `SendReferenceRequestAsync(referenceId)` — sistema envia email/solicitud al contacto
- `SubmitReferenceFeedbackAsync(referenceId, rating, feedback)` — el contacto responde
- `VerifyReferenceAsync(referenceId)` — sistema valida la respuesta y la marca como verificada
- `GetReferencesAsync(candidateId)` — lista de referencias con estado

**Endpoints:**
- `GET/POST api/candidates/{id}/references`
- `POST api/references/{id}/send` — envia solicitud
- `POST api/references/{id}/feedback` — el contacto responde (endpoint publico o con token)

**Preguntas que Darwin debe responder antes de codificar (respuestas en `docs/dsiezar/fase-3-sub5.md`):**

1. ¿Cuantas referencias minimas se exigen? ¿2 o 3?
2. ¿El email de solicitud de referencia se envia via SMTP, o se genera un link que el candidato comparte?
3. ¿El contacto de referencia necesita crear una cuenta en OpenToWork, o responde via un link publico con token?
4. ¿Que informacion se le pide al contacto? ¿Solo rating + feedback, o tambien confirmar datos del candidato?
5. ¿Las referencias verificadas suman al `EvidenceIndex`? ¿Cuanto?
6. ¿Si una referencia no responde en X dias, se marca como fallida? ¿Cuanto es X?
7. ¿El candidato puede ver el feedback que dio la referencia, o es privado para TD?
8. ¿Se validan que las referencias no sean del mismo empresa donde trabajo (para evitar sesgo)?

---

#### Sub-fase 3.6: Pruebas de Habilidades (Retos Tecnicos)

**Objetivo:** Banco de retos tecnicos con puntaje automatico.

**Metodos a implementar:**

- `CreateSkillTestAsync(dto)` — admin crea un reto (CRUD completo)
- `GetAvailableTestsAsync(category)` — lista de retos disponibles por categoria
- `StartTestAsync(candidateId, testId)` — candidato inicia un reto (registra intento + timer)
- `SubmitTestAsync(resultId, answers)` — candidato envia respuestas, sistema calcula puntaje automatico
- `GetTestResultsAsync(candidateId)` — historial de resultados del candidato

**Endpoints:**
- `GET/POST/PUT/DELETE api/skill-tests` — CRUD admin
- `GET api/skill-tests/available` — lista para candidatos
- `POST api/skill-tests/{id}/start` — inicia intento
- `POST api/skill-tests/results/{id}/submit` — envia respuestas

**Preguntas que Darwin debe responder antes de codificar (respuestas en `docs/dsiezar/fase-3-sub6.md`):**

1. ¿Los retos son multiple choice, codigo ejecutable, o ambos?
2. ¿El puntaje es automatico (sistema corrige) o requiere revision manual de TD?
3. ¿Si es codigo ejecutable, se usa un juez online (ej: Judge0, Piston) o se evalua con tests unitarios propios?
4. ¿El anti-copia que medidas tiene? ¿Tab switching, copiar/pegar, tiempo limite?
5. ¿Cuantos intentos tiene el candidato por reto? ¿1, 3, ilimitados?
6. ¿Los resultados de retos suman al `CandidateScore`? ¿A que indice?
7. ¿El candidato puede ver los retos disponibles antes de completar su perfil, o requiere perfil completo?
8. ¿Se puede retomar un reto despues de cerrar el navegador, o se anula el intento?

---

#### Sub-fase 3.7: Estado "Verificado TD" Automatico

**Objetivo:** Sistema que asigna automaticamente el estado de verificacion progresivo.

**Estados progresivos:**
```
Perfil registrado → Perfil completo → Evaluado → Verificacion en proceso → Verificado TD
```

**Metodos a implementar:**

- `GetVerificationStatusAsync(candidateId)` — retorna el estado actual
- `EvaluateVerificationStatusAsync(candidateId)` — evalua criterios y asigna estado:
  - **Perfil registrado:** candidato existe en el sistema
  - **Perfil completo:** `ProfileCompletionPercentage >= 80`
  - **Evaluado:** tiene `PTCandidateScore` con `OverallScore > 0` y al menos 3 verificaciones completadas
  - **Verificacion en proceso:** tiene verificaciones pendientes o en progreso
  - **Verificado TD:** todas las verificaciones pasaron, `OverallScore >= umbral`, referencias verificadas

**Endpoint:** `GET api/candidates/{id}/verification-status`

**Preguntas que Darwin debe responder antes de codificar (respuestas en `docs/dsiezar/fase-3-sub7.md`):**

1. ¿Cual es el `OverallScore` minimo para alcanzar "Verificado TD"? ¿60, 70, 80?
2. ¿Cuantas verificaciones deben pasar como minimo? ¿Todas o un subconjunto?
3. ¿Las referencias verificadas son obligatorias para "Verificado TD", o solo recomendadas?
4. ¿El estado se recalcula automaticamente cada vez que se completa una verificacion, o hay un job periodico?
5. ¿Si un candidato era "Verificado TD" y despues falla una verificacion (ej: portfolio cae), pierde el estado automaticamente?
6. ¿El distintivo ★ aparece en el perfil publico del candidato para las empresas? ¿Como se muestra?
7. ¿El candidato recibe notificacion cuando alcanza "Verificado TD"?
8. ¿Se puede revocar manualmente el estado desde el admin? ¿Quien tiene ese poder?

---

#### Sub-fase 3.8: UI — Integracion en los 3 portales

**Portal del Candidato:**
- Dashboard: 4 graficos circulares (Estabilidad, Confiabilidad, Evidencia, Compatibilidad) + OverallScore
- Seccion "Verificaciones": lista con estado (pendiente, verificada, fallida) + boton "Ejecutar verificaciones"
- Seccion "Referencias": CRUD para agregar contactos, ver estado de solicitudes
- Seccion "Retos tecnicos": lista de retos disponibles por categoria, tomar reto con timer
- Badge "Verificado TD" en el perfil cuando se cumpla

**Portal Admin:**
- Gestion de scores: ver indices de cada candidato, boton "Recalcular"
- Verificaciones manuales: aprobar/rechazar verificaciones
- Cola de shortlist: TD revisa matches antes de enviar a la empresa
- Banco de retos: CRUD de `PTSkillTest`

**Portal de Empresa:**
- Shortlist: ver candidatos rankeados por Job Match Score para cada vacante
- Scorecard configurable: ajustar pesos del Job Match Score por vacante
- Ver desglose del match (skills, experiencia, educacion)

**Preguntas que Darwin debe responder antes de codificar (respuestas en `docs/dsiezar/fase-3-sub8.md`):**

1. ¿Los graficos de indices en el dashboard del candidato son SVG circulares (como el donut existente) o barras horizontales?
2. ¿El candidato puede ver el desglose de que penalizo su `StabilityIndex` (ej: "Gap de 8 meses en 2023")?
3. ¿La cola de shortlist del admin tiene un workflow de aprobacion (pendiente → aprobado → enviado a empresa)?
4. ¿El scorecard configurable de la empresa es un formulario con sliders, o inputs numericos?
5. ¿El banco de retos del admin tiene preview del reto antes de publicarlo?

---

> **Resumen de sub-fases:** 8 sub-fases, cada una con preguntas que deben responderse antes de codificar. Las respuestas definen los algoritmos. El orden es secuencial: 3.1 → 3.2 → 3.3 → 3.4 → 3.5 → 3.6 → 3.7 → 3.8.
>
> **Checklist original (automatizacion via ValidationService/ScoringService) — implementado (Dsiezar, 01-Sep-2026):**

- [x] Entidades de scoring (`PTCandidateScore`, `PTVerification`, `PTCandidateReference`, `PTJobMatchScore`, `PTSkillTest`, `PTCandidateTestResult` — sub-fase 3.1)
- [x] ValidationService: verificacion automatica (LinkedIn, portafolio, coherencia cronologica, identidad como stub documentado — sub-fase 3.2)
- [x] ScoringService: indices de Estabilidad, Confiabilidad, Evidencia y Compatibilidad — sub-fase 3.3
- [x] CompatibilityService: match candidato-vacante (Job Match Score) — sub-fase 3.4
- [x] API endpoints: `GET/POST api/candidates/{id}/score[/recalculate]`, `GET/POST api/candidates/{id}/verifications[/run]` — sub-fases 3.2/3.3/3.8
- [x] Dashboard candidato: scores, verificaciones y badge "Verificado TD" en el perfil — sub-fase 3.8
- [x] Referencias laborales: CRUD + flujo de solicitud/feedback publico en `/references` — sub-fase 3.5
- [x] Pruebas de habilidades: `PTSkillTest`/`PTCandidateTestResult` + UI de reto con timer en `/skill-tests` — sub-fase 3.6

> **Nota (2026-08-24, Dsiezar):** Iluna construyo un **Pipeline de Reclutamiento** (ver Bitacora, sesion 21-Ago) que cubre gran parte del *objetivo* de negocio de Fase 3 (evaluar y verificar candidatos antes de mostrarlos a la empresa), pero con una **arquitectura distinta a la planeada aqui**: es un flujo de **evaluacion manual/asistida por un reclutador** (checklist de investigacion, evaluacion tecnica, entrevista cultural, score general por etapa) en vez de un motor 100% automatico. Entidades nuevas: `PTCandidateRecruitment`, `PTInvestigationChecklist`, `PTReferenceCheck`, `PTTechnicalEvaluation`, `PTRecruitmentStageLog`, `PTRecruitmentDismissal`. Ambos flujos coexisten: el Pipeline de Iluna sigue siendo la evaluacion manual/asistida por reclutador durante el proceso de investigacion; el Motor de Scoring de esta sub-fase es el calculo 100% automatico (`PTCandidateScore`/`PTVerification`) que corre en paralelo y alimenta el dashboard del candidato y el shortlist de la empresa.
>
> **Actualizacion (01-Sep-2026, Dsiezar):** el checklist original de arriba ya esta implementado de punta a punta - ver el detalle completo en `docs/dsiezar/fase-3-sub1.md` a `fase-3-sub8.md` y en la Bitacora de Cambios.

### Fase 4: Portal Administrativo - 100% COMPLETADA (por Dsiezar) + Pipeline de Reclutamiento (por Iluna)

- [x] AdminAPI con JWT independiente (puerto 5001)
- [x] AdminWEB con login y layout (puerto 5101)
- [x] Gestion de usuarios (activar, desactivar, eliminar)
- [x] Moderacion de vacantes (permanentes + temporales)
- [x] Dashboard admin con metricas y estadisticas reales
- [x] Gestion de categorias y skills (CRUD)
- [x] Log de auditoria admin (`ADAuditLog`)
- [x] Exportacion de datos (CSV)
- [x] i18n admin (es/en)
- [x] QA+SEC: 6 bugs corregidos (enumeracion de cuentas, paginacion negativa, CSV injection, estado vacantes temporales, auto-bloqueo admin, clave i18n)
- [x] Consola de candidatos con 4 tabs, filtros, stats, export CSV, acciones masivas (Iluna, 21-Ago)
- [x] Pipeline de reclutamiento: kanban por etapas, asignacion de reclutador, historial, descarte (Iluna, 21-Ago)
- [x] Checklist de investigacion + verificacion de referencias laborales (auto-generadas desde experiencia) (Iluna, 21-Ago)
- [x] Evaluaciones tecnicas y entrevistas culturales con puntuacion, score general por etapa (Iluna, 21-Ago)
- [x] Verificaciones manuales (aprobar/rechazar `PTVerification`) — resuelto en sub-fase 3.8: `CandidateProfile.razor` (admin) tiene botones Aprobar/Rechazar por verificacion, via `ValidationService.SetVerificationStatusAsync`
- [x] Revision de validaciones automaticas — resuelto en sub-fase 3.8: score e indices visibles con boton Recalcular en el mismo perfil
- [x] Gestion de roles de usuario (cambiar rol, no solo activar/desactivar) (Dsiezar, 29-Ago)
- [x] **CRM de Captacion de Empresas** (Iluna, 06-Sep) — pipeline de seguimiento comercial con 6 etapas (Lead, Contactado, Reunion, Propuesta Enviada, Negociacion, Cerrado Ganado), wizard por etapas, asignacion de responsables, descarte de empresas con motivo, restauracion, historial de cambios de etapa, mapa interactivo (Leaflet + OpenStreetMap) para pais/ciudad, seleccion de plan (Basic/Premium/Platinum) en etapa Propuesta Enviada, generacion de contrato de prestacion de servicios listo para firmar e imprimir
- [x] **Entrega de personal verificado (Embudo Ciego)** (Iluna, 08-Sep) — `DeliveriesController` en AdminAPI + `DeliveryService`: el reclutador entrega un candidato en etapa "Listo a Entregar" y con distintivo Verificado TD a una vacante/empresa concreta (`PTCandidateDelivery`, migracion `20260908113350_CandidateDeliveries`). La empresa solo ve conteos, nunca identidades, hasta la entrega

**Deuda tecnica documentada (4 items) — resueltos 29-Ago (Dsiezar):**
- [x] Unificar `AdminAuthService` con `AuthService` (logica duplicada) — extraida a `ITokenCryptoService` compartido en Core
- [x] Optimizar `AdminVacancyService` (carga tablas completas en memoria antes de paginar) — ahora traduce a `UNION ALL` con `Skip/Take` del lado del servidor
- [x] Mover `LocalStorageService`/`LanguageService` de AdminWEB a SharedUI
- [x] Centralizar guard de autenticacion en `AdminLayout` (copiado en 9 paginas; ademas protegia por primera vez las 4 paginas del Pipeline de Reclutamiento, que no tenian guard)

Los 2 items que dependian de Fase 3 quedaron resueltos el 01-Sep-2026 (ver sub-fase 3.8). Fase 4 no tiene items pendientes.

### Fase 5: Portal Corporativo - Parcialmente COMPLETADO (estructura base en OpenToWork.WEB)

> **Nota (31-Ago-2026, Iluna):** El portal corporativo YA EXISTE en `OpenToWork.WEB` (puerto 5147). No se necesitan los proyectos separados `OpenToWork.CorporateAPI`/`OpenToWork.CorporateWEB`. El portal de empresa funciona dentro del mismo proyecto que el portal de candidatos, con autenticacion JWT compartida y rutas diferenciadas (`/company-dashboard`, `/verified-applicants`, `/applicant-profile/{id}`, etc.).

**Completado:**
- [x] Registro de empresas (rol 1 en `Register.razor`, mismo flujo que candidatos)
- [x] Login de empresas (mismo `Login.razor`, JWT con rol diferenciado)
- [x] Dashboard corporativo (`CompanyDashboard.razor`) — command bar IA, badges de estadisticas, hero slider, postulantes recientes
- [x] Gestion de vacantes (`Vacancies.razor`, `VacancyManage.razor`, `MyVacancies.razor`)
- [x] Lista de postulantes verificados (`VerifiedApplicants.razor`) — cards con % perfil completado
- [x] Perfil completo del candidato en modo lectura (`ApplicantProfile.razor`) — estilo CV con layout 70/30
- [x] Entidad `PTCompany` — Name, Industry, Size, Website, Description, LogoUrl (ya existe en el modelo)
- [x] Mensajeria (`Messages.razor`)
- [x] Navegacion adaptada para rol empresa (`MainLayout.razor`)

**Desbloqueado por Fase 3 (Dsiezar, 01-Sep-2026):**
- [x] Ranking automatico de candidatos por compatibilidad — `CompatibilityService.GenerateShortlist`, sub-fase 3.4
- [x] Shortlist con Job Match Score — visible en `VacancyManage.razor` ("Ranking por Compatibilidad"), sub-fase 3.8
- [x] Scorecard configurable por vacante — `PTVacancy.WeightsConfig`, formulario en `VacancyManage.razor`, sub-fase 3.8

**Embudo Ciego — entrega de personal verificado (Iluna, 08-Sep-2026):**
- [x] La empresa NO ve candidatos ni identidades — `CompanyDashboard.razor` y `VerifiedApplicants.razor` muestran solo conteos ("X postulantes / Y en verificacion") por vacante
- [x] Entidad `PTCandidateDelivery` (`DeliveryStatus`: Delivered/ViewedByCompany/Interested/Hired/RejectedByCompany) + migracion `20260908113350_CandidateDeliveries`
- [x] `DeliveryService.DeliverCandidateAsync` — exige etapa `ReadyToDeliver` + `IsVerifiedTD == true` + vacante con empresa + no entregado dos veces a la misma vacante
- [x] `DeliveriesController` en AdminAPI (entregar) y en API publica (la empresa consulta y responde)
- [x] Videos `v01.mp4` / `v02.mp4` en bucle en login y home; seed de hosteleria (`docs/seed-hosteleria.sql`); menu de empresa sin "Busqueda avanzada" (se reactiva cuando la empresa tome un plan destinado a eso, otra fase)
- [x] Validacion del flujo con el agente de RH — `docs/rh/validacion-flujo-negocio-2026-09-08.md`, `docs/rh/guia-embudo-ciego-iluna.md`

**Pendiente:**
- [ ] Sistema de suscripciones para empresas — planes reales del sistema son Basic/Premium/Platinum (`PT_Plans`, ver "Observaciones para Darwin / Dsiezar" punto 4, pedido 18-Sep), no Basic/Pro/Enterprise; requiere definir modelo de ingresos, activar detras de un flag de configuracion
- [ ] Plan de Prioridad para Candidatos (5.99 EUR) — nueva idea de Darwin (18-Sep), ver "Observaciones para Darwin / Dsiezar" punto 4
- [ ] Entidad `COSubscription` — CompanyId, Plan, Status, StartDate, EndDate, MonthlyFee
- [ ] Entidad `COSearchHistory` — CompanyId, Filters, ResultCount, SearchedAt
- [ ] Entidad `COCandidateView` — CompanyId, CandidateId, ScoreSnapshot, ViewedAt
- [ ] Busqueda avanzada con filtros por score, confiabilidad, estabilidad (`PTCandidateScore` ya existe desde Fase 3, falta la UI de busqueda)
- [ ] Vista de perfiles evaluados con checkmarks de verificacion (`PTVerification` ya existe desde Fase 3, falta el checkmark en `VerifiedApplicants.razor`)
- [ ] Reportes avanzados
- [ ] Migracion EF Core para entidades corporativas restantes

### Fase 6: Servicios Premium - Pendiente

- [ ] Verificacion manual de referencias (servicio premium para empresas)
- [ ] Evaluaciones especificas por industria
- [ ] Integraciones con sistemas de RRHH (API endpoints externos)
- [ ] Analytics avanzados de reclutamiento

### Fase 7: Integraciones Externas - Pendiente

- [ ] LinkedIn API (validacion real de perfiles)
- [ ] Pasarela de pagos (Stripe/PayPal para suscripciones)
- [ ] Notificaciones por email (SMTP)
- [ ] Notificaciones push

### Fase 8: Pruebas y Despliegue - Pendiente

- [ ] Pruebas unitarias (cobertura > 70% en Core)
- [ ] Pruebas de integracion (3 APIs)
- [ ] Documentacion final
- [ ] Despliegue en produccion

---

## Tareas Pendientes

> **Tracker vigente:** el checklist operativo del ciclo comercial (que falta del negocio real, pieza por pieza) vive en la seccion "Auditoria del Ciclo Comercial Completo (Gap Analysis)" al inicio de este documento, y se actualiza a medida que se construye cada pieza. Esta seccion es el resumen por fase tecnica del plan original.

| Fase | Tareas pendientes | Bloquea a |
|------|-------------------|-----------|
| **Fase 1-4** | 0 tareas — COMPLETADAS | — |
| **Fase 5** | 8 tareas (suscripciones, entidades CO, busqueda por score, checkmarks, reportes) — estructura base + shortlist/scorecard ya existen | Fase 6 |
| **Fase 6** | 4 tareas (servicios premium) | — |
| **Fase 7** | 4 tareas (integraciones externas) | — |
| **Fase 8** | 4 tareas (pruebas, despliegue) — sigue sin cobertura automatizada mas alla de `OpenToWork.Tests` (Fase 1) | — |

**Bugs resueltos en main:**
- [x] `#blazor-error-ui` siempre visible en `OpenToWork.WEB` — corregido con `display: none`
- [x] Google OAuth config en `OpenToWork.API` — corregido: lee `GoogleOAuth:ClientId` y solo registra si hay credenciales

**Portal Administrativo — items marcados "bloqueados por Fase 3" que ya se resolvieron (sub-fase 3.8, 01-Sep):**
- [x] Verificaciones manuales — aprobar/rechazar `PTVerification` desde el perfil del candidato en el admin (`CandidateProfile.razor`)
- [x] Revision de validaciones automaticas — LinkedIn, portafolio y coherencia cronologica visibles en el mismo panel
- [x] Gestion de scores de candidatos — los 4 indices + `OverallScore` se muestran y recalculan desde el perfil del candidato

**Deuda tecnica resuelta (29-Ago, Dsiezar):**
- [x] Gestion de roles de usuario — cambio de `PrimaryRole` desde `/users` con guardia de auto-bloqueo
- [x] Unificar `AdminAuthService`/`AuthService` — crypto extraida a `ITokenCryptoService`
- [x] Optimizar `AdminVacancyService` — `Concat` a nivel `IQueryable`, traducido a `UNION ALL` con paginacion en servidor
- [x] Mover `LocalStorageService`/`LanguageService` a `SharedUI`
- [x] Centralizar guard de autenticacion en `AdminLayout`

**Pendiente real (no bloqueada, sin priorizar todavia):**
- [ ] Pruebas unitarias para AdminAPI (`OpenToWork.AdminTests` — login, dashboard, CRUD, moderacion, export)
- [ ] Pruebas de seguridad admin (acceso cruzado candidato→admin, auto-bloqueo, paginacion con valores negativos)

---

## Ruta de Trabajo

### Fases independientes (pueden avanzar en paralelo)

Las siguientes fases **no tienen dependencias entre si** y pueden trabajarse simultaneamente por desarrolladores diferentes:

| Fase | Independiente de | Rama sugerida |
|------|------------------|---------------|
| **Fase 3** (Motor de Scoring) | No depende de ninguna otra fase | `iluna-fase-3` |
| **Fase 5** (Portal Corporativo) | Solo depende de Fase 3 para los scores, pero la estructura base (proyecto, JWT, layout, registro de empresas, suscripciones) se puede construir en paralelo | `dsiezar-fase-5` |
| **Fase 7** (Integraciones Externas) | LinkedIn API y pasarela de pagos son independientes del resto | cualquier rama |

### Fases con dependencias (secuenciales)

| Fase | Depende de | Motivo |
|------|------------|--------|
| **Fase 4** (completar 15%) | Fase 3 | Verificaciones manuales requieren `PTVerification` y `ValidationService` |
| **Fase 5** (busqueda por score) | Fase 3 | Filtros por score requieren `PTCandidateScore` |
| **Fase 6** (Servicios Premium) | Fase 5 | Servicios premium requieren portal corporativo funcional |
| **Fase 8** (Pruebas) | Fases 3-7 | Pruebas integrales requieren todo funcional |

### Orden recomendado de ejecucion

```
Fase 3 (Motor de Scoring) ──────────────────────────────────────┐
  │                                                              │
  ├── Fase 4 (completar verificaciones admin)                    │
  │                                                              │
  ├── Fase 5 (Portal Corporativo) ──── Fase 6 (Premium)          │
  │                                                              │
  └─────────────────────────────────────── Fase 7 (Integraciones)│
                                                                 │
  Fase 8 (Pruebas y Despliegue) ◄────────────────────────────────┘
```

### Plan de ejecucion detallado

1. ~~**Fase 3 - Motor de Evaluacion y Scoring**~~ — **COMPLETADA (Dsiezar, 01-Sep-2026)**, ver detalle en la seccion "Fases del Proyecto" y en `docs/dsiezar/fase-3-sub1.md` a `fase-3-sub8.md`.

2. **Fase 4 - Portal Administrativo (COMPLETADA):**
   - [x] Verificaciones manuales (aprobar/rechazar `PTVerification`) — resuelto en sub-fase 3.8 (Dsiezar, 01-Sep)
   - [x] Revision de validaciones automaticas — resuelto en sub-fase 3.8 (Dsiezar, 01-Sep)
   - [x] Gestion de roles de usuario (cambiar rol, no solo activar/desactivar) (Dsiezar, 29-Ago)
   - [x] Resueltos los 4 items de deuda tecnica (Dsiezar, 29-Ago):
     - Unificar `AdminAuthService` con `AuthService`
     - Optimizar `AdminVacancyService` (paginacion en BD, no en memoria)
     - Mover `LocalStorageService`/`LanguageService` a SharedUI
     - Centralizar guard de autenticacion en `AdminLayout`
   - **Validacion: ejecutado AdminAPI + AdminWEB contra MySQL real, pantallas verificadas en navegador**

3. **Fase 5 - Portal Corporativo** (nota 31-Ago de Iluna: no hacen falta proyectos `CorporateAPI`/`CorporateWEB` separados, el portal ya vive en `OpenToWork.WEB` - items de infraestructura de abajo obsoletos):
   - ~~Crear `OpenToWork.CorporateAPI` (puerto 5002, JWT independiente)~~ — obsoleto, ver nota
   - ~~Crear `OpenToWork.CorporateWEB` (puerto 5102)~~ — obsoleto, ver nota
   - Entidad `COCompany` — ya cubierta por `PTCompany` (Registro/Login/Dashboard de empresa funcionando)
   - Entidades `COSubscription`/`COSearchHistory`/`COCandidateView` — pendientes
   - Sistema de suscripciones (planes: Basic, Pro, Enterprise) — pendiente
   - Busqueda avanzada con filtros por score — pendiente (`PTCandidateScore` ya existe desde Fase 3)
   - Vista de perfiles evaluados con checkmarks — pendiente
   - [x] Ranking automatico de candidatos por compatibilidad — `CompatibilityService`, sub-fase 3.4/3.8 (Dsiezar, 01-Sep)
   - Reportes avanzados — pendiente
   - **Validacion: ejecutar CorporateAPI + CorporateWEB, verificar pantallas funcionen**
   - **Validacion: comprobar patron de diseno One UI consistente**

4. **Fase 6 - Servicios Premium:**
   - Verificacion manual de referencias (premium)
   - Evaluaciones por industria
   - Integraciones RRHH

5. **Fase 7 - Integraciones Externas (independiente, puede avanzar en paralelo):**
   - LinkedIn API, pasarela de pagos, notificaciones

6. **Fase 8 - Pruebas y Despliegue:**
   - Cobertura > 70%, 3 APIs, despliegue produccion

### Criterios de validacion por fase (obligatorios antes de avanzar)

Antes de marcar cualquier fase como completada, se debe validar:

1. **Build sin errores:** `dotnet build OpenToWork.slnx` -> 0 errores
2. **API funcional:** ejecutar la API correspondiente y verificar endpoints con datos reales (no mocks)
3. **WEB funcional:** ejecutar el frontend correspondiente y verificar pantallas en navegador
4. **Patron de diseno:** comprobar que la UI cumple con One UI (squircles `border-radius: 20px`, pill buttons `border-radius: 9999px`, Bento Grid, temas navy/dark/light, espaciado consistente)
5. **i18n:** sin texto hardcoded, todas las claves existen en es/en
6. **Responsive:** verificar en tablet (1024px), mobile (768px) y small mobile (480px)
7. **Sin regresiones:** las fases anteriores siguen funcionando

---

## NOTAS

### Creditos de analisis IA por plan (Idea)

- En un **plan basico** se le daran **5 creditos de analisis IA** a la empresa.
- La empresa podra subir los CV que quieran.
- Una **IA especializada en RRHH** generara un **analisis FODA** del CV, indicando si el candidato es apto para X vacante.
- **Flujo del CV:** la empresa sube el CV → nosotros lo guardamos para contactar al posible postulante → obtenemos los permisos para verificarlo → lo proponemos a la empresa con sus habilidades.

---

## Notas de Actualizacion

> **Regla obligatoria:** Todo desarrollador debe agregar sus notas de cambios en esta seccion cada vez que haga un commit en `main`. El formato es: fecha, nombre del desarrollador, fase, resumen de cambios. Esto mantiene a ambos enterados del progreso sin necesidad de revisar commits uno por uno.

### Estado actual del proyecto

- **Fase 1 (Fundacion):** COMPLETADA
- **Fase 2 (Portal de Candidatos):** COMPLETADA
- **Fase 3 (Motor de Evaluacion y Scoring):** COMPLETADA (Dsiezar, 01-Sep-2026) — las 8 sub-fases del plan obligatorio de Iluna. Mergeada a `main` el 07-Sep. El **Pipeline de Reclutamiento manual** de Iluna (21-Ago) sigue activo en paralelo, ver nota en la seccion "Fases del Proyecto"
- **Fase 4 (Portal Administrativo):** COMPLETADA — Dsiezar (roles + deuda tecnica, 29-Ago; verificaciones manuales, 01-Sep) + Pipeline de Reclutamiento completo (Iluna, 21-Ago) + CRM de Captacion de Empresas (Iluna, 06-Sep: pipeline comercial, planes, contratos, mapa interactivo) + RBAC de Personal Administrativo y flujo de Cierre de Negociaciones (Dsiezar, 05-Sep: roles SuperAdmin/Reclutador/Comercial con enforcement real, pantalla "Personal Administrativo", presentar candidatos a la empresa y cerrar negociacion). Todo mergeado a `main` el 07-Sep, con 5 bugs de integracion encontrados y corregidos en QA (ver bitacora 07-Sep)
- **Fase 5 (Portal Corporativo):** Parcial — estructura base + shortlist/scorecard (Fase 3) + busqueda avanzada por score (Dsiezar) ya funcionan; categorias de vacante migradas al rubro de hosteleria y gate de login/registro en detalle de vacante (Dsiezar, 05-Sep); **Embudo Ciego** operativo (Iluna, 08-Sep: la empresa solo ve conteos, TD entrega personal verificado via `PTCandidateDelivery`); falta suscripciones
- **Fases 6-8:** Pendientes
- **`main` esta al dia** con todo lo anterior, incluida la rama `iluna-embudo-ciego` (ya integrada de punta a punta). Ultimos merges: mejoras del documento contractual + ContactDniNie + pagina de edicion de empresa (Iluna, 10-Sep), fix de la columna huerfana `PT_VacancyContracts.PT_VacancyId` que rompia "Generar contrato" siempre (Dsiezar, 11-Sep), **precios B2B por tipo de puesto + codigos promocionales** (Dsiezar, 12-Sep, ver detalle abajo)

### Nota para Darwin (Dsiezar) — Que falta para empezar a operar

Darwin, el portal administrativo esta funcional con CRM de empresas, pipeline de reclutamiento, embudo ciego, documento contractual y ahora precios reales por tipo de puesto. Para que Trato Directo pueda **iniciar operaciones reales de reclutamiento**, falta:

1. **Sistema de suscripciones (Fase 5)** — las empresas necesitan poder contratar un plan (Basic/Premium/Platinum) para acceder al servicio. Hoy los planes existen como seed data pero no hay flujo de pago/seleccion. **No confundir con el modulo de precios nuevo**: los planes son la suscripcion de la empresa al servicio; el modulo de precios (12-Sep) es la tarifa que TD cobra por cada vacante que gestiona.
2. **Notificaciones por email (Fase 7)** — el flujo de envio de contratos y notificaciones a empresas requiere SMTP configurado. Hoy los contratos se generan pero no se envian por email.
3. **Claves i18n de las clausulas del contrato** — las 23 clausulas del Contrato Marco estan hardcoded en espanol en `VacancyContractDocument.razor`. Falta migrar a claves i18n para soportar ingles.
4. ~~Integrar rama `iluna-embudo-ciego`~~ — ya mergeada a `main`.
5. **CRUD de Planes** — admin necesita poder gestionar los planes (Basic/Premium/Platinum, suscripcion de empresa) desde el portal (crear, editar, desactivar). Sigue pendiente.
6. **Precios reales** — los 9 precios base sembrados en `docs/seed-job-pricing.sql` son placeholder (calcados proporcionalmente del dummy de 1500 EUR que existia antes). Hay que reemplazarlos por la tarifa real de Trato Directo desde `/pricing/job-types`.

### Consulta a RH (@rh) — Que falta para empezar a andar

**Soy RH.** Identifico el proceso en curso: **arranque operativo de Trato Directo como agencia de seleccion**.

El portal administrativo tiene el flujo completo: CRM de captacion de empresas, pipeline de reclutamiento (investigacion, evaluacion tecnica, entrevista cultural, referencias), embudo ciego (entrega de personal verificado), y documento contractual formal. Sin embargo, para que Trato Directo pueda **iniciar operaciones reales**, faltan elementos que desde RH identificamos como criticos:

1. **Suscripciones de empresas** — las empresas necesitan un mecanismo formal de contratacion de plan. Hoy no hay pasarela de pago ni flujo de seleccion de plan activo. **Esto bloquea la comercializacion.**
2. **Notificaciones por email** — el envio de contratos a empresas y la comunicacion con candidatos/referencias requiere SMTP. Hoy todo es manual. **Esto bloquea la operacion a escala.**
3. **Definicion de scorecards por vacante** — antes de publicar vacantes reales, necesitamos definir las rubricas de competencias (tecnicas y blandas) por cada puesto tipo. Hoy el pipeline de reclutamiento tiene evaluacion pero sin scorecards estandarizadas por rol.
4. **Sourcing activo** — no hay candidatos cargados en el sistema. Para iniciar, necesitamos cargar un primer lote de perfiles (minimo 20-30) en la base de datos para que el pipeline tenga material con el que trabajar.
5. **Proceso de onboarding de empresa piloto** — necesitamos seleccionar 1-2 empresas del CRM que esten en etapa "Cerrado Ganado" (o cerca) y ejecutar el flujo end-to-end con ellas: contrato → vacante → pipeline → entrega.

**Recomendacion de RH:** Priorizar (1) suscripciones y (2) email como bloqueantes operativos. Paralelamente, cargar candidatos seed y definir scorecards para los primeros puestos de hosteleria. Con eso, Trato Directo puede iniciar operaciones con 1-2 empresas piloto.

### Indicaciones para continuar

1. **Culminar las fases pendientes en orden de prioridad.** Fase 5 (suscripciones, busqueda por score), despues Fase 6/7, luego Fase 8 (testing/despliegue).
2. **No avanzar a la siguiente fase hasta validar que las pantallas funcionen correctamente.** Ejecutar API + WEB y verificar en navegador con datos reales.
3. **Validar que se cumpla el patron de diseno solicitado** (Samsung One UI: squircles, pill buttons, Bento Grid, temas consistentes, espaciado uniforme).
4. **Mejorar todo lo que sea posible para verse mas profesional.** Cada fase debe entregar una UI pulida, no solo funcional.
5. **La Fase 5 (Portal Corporativo) puede avanzar en paralelo con la Fase 3** en su estructura base (proyecto, JWT, layout, registro de empresas, suscripciones). La busqueda por score si requiere que Fase 3 este terminada.
6. **La Fase 7 (Integraciones Externas) es independiente** y puede avanzar en paralelo con cualquier otra fase.

### Fases que pueden trabajarse en paralelo

| Desarrollador | Fase | Rama | Independiente de |
|---------------|------|------|------------------|
| Desarrollador A | Fase 3 (Motor de Scoring) | `iluna-fase-3` | Sin dependencias |
| Desarrollador B | Fase 5 (estructura base Portal Corporativo) | `dsiezar-fase-5` | Solo depende de Fase 3 para busqueda por score |
| Cualquiera | Fase 7 (Integraciones Externas) | rama dedicada | LinkedIn API y pagos son independientes |

### Bitacora de cambios en main

| Fecha | Desarrollador | Fase | Cambios |
|------|---------------|------|--------|
| 2026-08-12 | Iluna | Fase 2 | Rediseno UI/UX Home: hero navy, capsule search bar, pill badges, Bento Grid role cards, footer corporativo, cinta de vacantes destacadas |
| 2026-08-12 | Iluna | Fase 2 | Fix CSS loading: middleware order en Program.cs (UseStaticFiles antes de UseHttpsRedirection) |
| 2026-08-12 | Iluna | Fase 2 | AuthLayout: unificar nav-brand con logo OTW + texto OpenToWork |
| 2026-08-12 | Iluna | Fase 2 | Register: segmented control pill toggle (One UI) reemplazando role cards pesadas |
| 2026-08-12 | Iluna | Docs | BUSINESS_PROPOSAL.md: propuesta de negocio completa |
| 2026-08-12 | Iluna | Docs | PLAN_DE_PROYECTO.md v2.0: 3 portales, 8 fases, entidades nuevas |
| 2026-08-12 | Iluna | Docs | README: alineado con 3 portales y propuesta de negocio |
| 2026-08-13 | Dsiezar | Fase 4 | AdminAPI: JWT independiente, login admin, auditoria, controllers (users, vacancies, skills, dashboard, export) |
| 2026-08-13 | Dsiezar | Fase 4 | AdminWEB: login, layout, dashboard, pages (users, vacancies, skills, audit-log) |
| 2026-08-13 | Dsiezar | Fase 4 | QA+SEC: 6 bugs corregidos (enumeracion cuentas, paginacion, CSV injection, vacantes temporales, auto-bloqueo, i18n) |
| 2026-08-13 | Dsiezar | Fase 4 | Fix fuera de alcance: Google OAuth config en API, #blazor-error-ui en WEB |
| 2026-08-14 | Iluna | Docs | README: notas de actualizacion, ruta de trabajo, fases paralelas, criterios de validacion |
| 2026-08-14 | Iluna | Docs | DEPLOYMENT.md: guia de despliegue a Windows Server/IIS (Web Deploy, PSRemoting, GitHub Actions) |
| 2026-08-14 | Iluna | Docs | README: instrucciones de ejecucion AdminAPI + AdminWEB, credenciales de prueba admin |
| 2026-08-14 | Iluna | Fase 4 | AdminWEB: rediseno layout sidebar + topbar profesional, admin.css, tablas con status badges, empty/loading states |
| 2026-08-14 | Iluna | Fase 4 | AdminWEB: pendiente - mejorar tablas con filtros, pulir diseno inspirado en Cazvid (pipeline visual, cards de aplicantes) |
| 2026-08-14 | Iluna | Fase 4 | Seed data: 3 empresas, 10 vacantes permanentes, 3 vacantes temporales, 20 skills, 3 postulantes, 5 aplicaciones |
| 2026-08-14 | Iluna | Docs | seed-data.sql: script de datos de prueba con credenciales para todos los roles |
| 2026-08-15 | Dsiezar | Docs | Respuesta a las 17 preguntas de RH + definicion estrategica consolidada (dos scores separados, verificacion como estado progresivo, retencion basada en estado) |
| 2026-08-21 | Iluna | Fase 4 | Pipeline de Reclutamiento completo: consola de candidatos, kanban, checklist de investigacion, referencias automaticas, evaluaciones tecnicas, entrevistas culturales, score general (6 migraciones nuevas) |
| 2026-08-24 | Dsiezar | Docs | Migraciones del Pipeline de Reclutamiento aplicadas localmente; README sincronizado con el estado real de Fase 3/4 (estaba desactualizado, faltaba registrar 60+ commits) |
| 2026-09-06 | Iluna | Fase 4 | CRM de Captacion de Empresas: pipeline comercial 6 etapas (Lead → Contactado → Reunion → Propuesta Enviada → Negociacion → Cerrado Ganado), wizard progresivo con inputs unificados, mapa Leaflet+OpenStreetMap para pais/ciudad, tabla PT_Plans con 3 planes seed (Basic/Premium/Platinum), API endpoint GET /plans, seleccion de plan en etapa Propuesta Enviada con 3 cards, generacion de contrato de prestacion de servicios en HTML imprimible (Trato Directo), descarte/restauracion de empresas, historial de cambios de etapa |
| 2026-09-07 | Dsiezar | Merge | Fusionados a `main`: RBAC de personal administrativo + negociaciones + rubro hosteleria (Fase 5, `dsiezar-fase-5`) junto con el CRM de Empresas de Iluna. Conflictos resueltos a mano (nav de AdminLayout, i18n, DI, `AppDbContext`); `AppDbContextModelSnapshot.cs` (3500+ lineas, 26 conflictos) regenerado con `dotnet ef migrations add` en vez de resuelto linea por linea |
| 2026-09-07 | Dsiezar | QA | QA automatizado del portal administrativo (Panel, Candidatos, Empresas/CRM), con datos reales contra MySQL. 5 bugs reales encontrados y corregidos: (1) `OpenToWork.API` no arrancaba por un DI mal registrado en el commit del CRM; (2) crash de circuito Blazor al abrir el perfil de un candidato sin nombre (`Substring` fuera de rango); (3) `PT_Companies` sin las columnas nuevas del CRM (`LegalName`/`TaxId`/`ContactName`/`ContactPosition`/`Status`) - faltaba la migracion real, tiraba 500 en Empresas/Pipeline/Captacion; (4) clave de traduccion `admin.common.back` faltante; (5) 52 reglas de `admin.css` usaban `var(--border)`, variable inexistente en los temas - todos los bordes/separadores del panel admin se veian planos. De paso, rediseno del Pipeline de Empresas: color por etapa (progresion neutro→frio→marca→calido→ganado) y fix del layout de las tarjetas (fecha y responsable aparecian pegados sin espacio) |
| 2026-09-07 | Dsiezar | Docs | Analisis RH: alineacion con el modelo Trato Directo y por que no arranco el reclutamiento real (`docs/rh/alineacion-trato-directo-y-arranque.md`). Hallazgo: el gate de `CandidateSearchService` era `IsProfilePublic && WizardCompleted` - la verificacion era un filtro opcional, no un piso obligatorio |
| 2026-09-08 | Iluna | Fase 5 | **Embudo Ciego** - entrega de personal verificado a empresas: entidad `PTCandidateDelivery` + `DeliveryStatus`, `DeliveryService.DeliverCandidateAsync` (exige etapa "Listo a Entregar" + Verificado TD + vacante con empresa), `DeliveriesController` en AdminAPI y API publica, migracion `20260908113350_CandidateDeliveries`. `CompanyDashboard`/`VerifiedApplicants` ahora muestran solo conteos, nunca identidades. Videos v01/v02 en bucle en login y home, seed de hosteleria, menu de empresa sin "Busqueda avanzada", validacion del flujo con el agente RH (`docs/rh/`) |
| 2026-09-08 | Dsiezar | Fix | `RecruitmentService.GetVacancyOptionsAsync` filtraba `Status == 0` (Borrador) en vez de `1` (Activa) - el modal "Vincular vacante" del flujo de entrega siempre decia "No hay vacantes activas", dejando la feature de Embudo Ciego inutilizable. Adoptada la implementacion de Iluna sobre la propia (`git reset` de un commit no pusheado que duplicaba el mismo flujo) |
| 2026-09-09 | Dsiezar | Fase 2 | Pagina publica de **Preguntas Frecuentes** (`/faq`, `Faq.razor`) - acordeon con 10 Q&A alineadas al modelo (gratis para candidatos, la empresa ve conteos no perfiles, distintivo Verificado TD, evaluacion, datos personales, vigencia de perfil, idiomas). Claves `common.faq.*` en es/en, estilos `.faq-*` en `components.css` (v24→v25). Resuelve el enlace del footer a `/faq` que devolvia 404 |
| 2026-09-10 | Iluna | Fase 4 | Mejoras del documento contractual: layout limpio (popup sin admin shell), fuente Arial, texto justificado, line-height 1.5, ANEXO I en pagina aparte, fondos grises eliminados, firmas alineadas, datos reales de TRATO DIRECTO hardcodeados, estado oculto, cache-busting CSS. Campo `ContactDniNie` end-to-end (entidad, DTOs, servicios, UI, migracion). Pagina de edicion de empresa (`Edit.razor`). Flujo de aprobacion de contratos confirmado (Send/Accept/Reject). Commit `c0ccb04` |
| 2026-09-11 | Iluna | Fase 4 | Wizard de contrato multi-vacante (1 contrato : N vacantes vía `PT_ContractVacancies`), borrado en cascada de empresa, filtro de vacantes por empresa desde el detalle |
| 2026-09-11 | Dsiezar | Fix | "Generar contrato" fallaba siempre con el mensaje generico "Verifica que la distribucion de pago sume 100%" (que ni siquiera era la causa real). La migracion que paso el contrato a multi-vacante nunca elimino la columna huerfana `PT_VacancyContracts.PT_VacancyId` (NOT NULL + FK), asi que EF insertaba sin ella y MySQL rechazaba la fila. Migracion `FixOrphanContractVacancyColumn`; de paso corregido que el snapshot de EF nunca habia registrado `ContactDniNie` (hubiera roto el siguiente `migrations add`) |
| 2026-09-12 | Dsiezar | Feature | **Precios B2B por tipo de puesto + codigos promocionales.** Catalogo configurable `PTJobLevel`/`PTJobType` (reemplaza el enum fijo `ContractJobType` para este fin), lista de precios versionada `PTJobTypePrice` (nunca se sobreescribe, se cierra el vigente y se abre uno nuevo), `PTPromoCode`/`PTPromoCodeRedemption` (% o monto fijo, restringible a nivel y/o tipo, vigencia, limite de usos, auditoria de canjes). El precio ahora es por vacante dentro del contrato (`PT_ContractVacancies` gana BasePrice/PromoCode/DiscountAmount/FinalPrice/IsManualOverride), no un monto unico por contrato. 3 pantallas nuevas en `/pricing`. Reemplaza el precio ficticio de 1500 EUR/vacante del wizard de captacion de empresa. Seed: 3 niveles, 9 tipos mapeados desde las categorias existentes, codigo `ALEJO26` de ejemplo (10% en Operativo). Verificado end-to-end: 900 EUR → 810 EUR con el codigo aplicado, contador de usos incrementado, documento formal reflejando el precio final |
| 2026-09-13 | Dsiezar | Docs | Rediseno del Pipeline de Empresas (metricas, filtros por responsable/sector/fecha en una sola linea, vista Tablero/Lista) + badge "Verificado TD" en la ficha de candidato del admin. Infografia **"Del Lead al Contrato"** publicada (ver seccion "Diagramas y Documentacion Visual" arriba) con el ciclo completo: CRM de Iluna → Pipeline de Reclutamiento de Iluna → Negociaciones de Dsiezar, para que quede clara la conexion entre ambos flujos |
| 2026-09-13 | Dsiezar | Fix | Mismo rediseno (metricas, filtros en una linea, vista Tablero/Lista) aplicado al Pipeline de Candidatos, con `City` agregado a `RecruitmentPipelineDto`. Fix de fondo: las secciones de Investigacion, Evaluacion Tecnica, Entrevista Cultural y Preferencias del candidato en `PipelineDetail.razor` solo se mostraban en su etapa exacta (`CurrentStage == N`) y desaparecian por completo al avanzar de etapa, impidiendo corregir datos ya cargados - cambiado a `CurrentStage >= N` para que queden editables en cualquier etapa posterior (mientras no este descartado). Tambien: `.admin-content-inner` (max-width 1200px en todo el admin) dejaba un hueco enorme a los lados del kanban en monitores anchos - se quita solo para paginas con `.admin-pipeline-kanban` (via `:has()`) y las columnas pasan de ancho fijo a flex-grow. Botones Tablero/Lista con iconos en vez de texto |
| 2026-09-13 | Dsiezar | Feature | **Tramos de pago del contrato (30/50/20).** Primer item resuelto del gap-analysis "Auditoria del Ciclo Comercial" (ver seccion arriba). Entidad nueva `PTContractPayment` (migracion `ContractPaymentTranches`): 3 tramos (Apertura/Validacion/Consolidacion) generados automaticamente al aceptar el contrato (`AdminContractService.DecideAsync`), con monto congelado segun `FeeAmount` y el % vigente. Panel "Tramos de Pago" en `/vacancies/{id}/contract` para que un admin los marque pagado/pendiente (con nota) - sin pasarela de pago integrada. Gate real: `PermanentVacancyService.PublishVacancyAsync` bloquea la publicacion de la vacante con un error explicito si el tramo de Apertura de su contrato no esta pagado (coincide con el unico rombo de decision del diagrama oficial - Validacion y Consolidacion se trackean pero no bloquean nada, por decision explicita de Darwin). Verificado end-to-end: contrato aceptado -> 3 tramos generados (243/405/162 EUR sobre un contrato de 810 EUR) -> intento de publicar bloqueado -> Apertura marcada pagada -> publicacion exitosa |
| 2026-09-13 | Dsiezar | Feature | **Fecha de incorporacion + visibilidad de garantia (pasos 17 y 19 del gap-analysis).** Se descubrio que existen DOS flujos de "contratado" sin conexion entre si: `PTNegotiation` (Negociaciones, shortlist->cierre) y `PTCandidateDelivery` (Embudo Ciego, respuesta de la empresa). Se agrego `IncorporationDate` a ambas entidades (migracion `IncorporationDateTracking`), por decision explicita de Darwin de cubrir los dos caminos. `WarrantyCalculator` (calculo puro, sin DB) + `IWarrantyLookupService` (resuelve `WarrantyDays` del contrato via `PT_ContractVacancies`) calculan en vivo el estado Activa/Por vencer (<=7 dias)/Vencida - nunca se persiste, se recalcula en cada lectura. UI: en `/vacancies` (negociacion cerrada) y en la ficha del candidato (entrega Contratada) se puede registrar la fecha y se ve el badge de garantia. Verificado end-to-end: calculo matematico (5 casos limite, incluyendo sin fecha/sin garantia definida) + flujo real en `/vacancies` (persistio en `PT_Negotiations.IncorporationDate`, mostro "Sin garantia definida" al no haber contrato vinculado) + endpoint de Entregas probado directo (incluyendo que el guard rechaza registrar incorporacion si la entrega no esta en estado Contratado). Pendiente (fuera de este alcance): el registro de contacto activo con cliente/candidato y el gate "¿continua?" que dispara la reposicion (paso 20) |
| 2026-09-13 | Dsiezar | Fix | **Paginacion real en Moderacion de Vacantes** (`/vacancies`). `AdminVacancyService.GetVacanciesAsync` devolvia una pagina de 20 sin TotalCount ni forma de saber si habia mas - con 21 vacantes activas, la ultima quedaba invisible y sin manera de llegar a ella. Se envuelve la respuesta en `AdminVacancyResultDto` (Items/TotalCount/TotalPages, mismo patron que `CandidateConsoleResultDto`), se agrega el conteo real (`CountAsync` sobre la misma query combinada permanentes+temporales antes del Skip/Take) y controles Anterior/Siguiente en la UI (mismo componente que `Candidates/Index.razor`). De paso corregidos los otros 2 consumidores del mismo metodo (`DashboardResults.razor`, `ExportService.ExportVacanciesCsvAsync`) que esperaban la lista plana. Verificado: 21 vacantes -> Pagina 1 de 2 (20 items, incluye la que antes faltaba) -> Pagina 2 de 2 (2 items) -> botón Siguiente deshabilitado en la ultima pagina |

---

## Observaciones para Darwin / Dsiezar

> **Actualizado 06-Sep-2026 (Iluna):** Tareas pendientes derivadas del CRM de Captacion de Empresas.

### 1. CRUD de Planes — resuelto distinto a lo planteado (Dsiezar, 17-Sep)
- ~~Crear el CRUD completo de `PTPlan` en el portal administrativo para administrar los planes. Modificar los 3 planes existentes (Basic, Premium, Platinum) para alinearlos a la marca Trato Directo.~~
- **Decision de Darwin (17-Sep):** el modelo actual es venta directa por posicion cubierta, no autogestion por planes de suscripcion. En vez de construir un CRUD para `PTPlan`, la etapa "Propuesta Enviada" del CRM ahora consume el catalogo ya existente y ya editable de **Precios y Niveles de Precio** (`/pricing/job-levels`, `/pricing/job-types`) — el mismo que usan los contratos reales. Ver Bitacora, sesion 2026-09-17.
- `PTPlan`/`GetPlansAsync()` se dejan intactos (sin CRUD) para cuando exista el modulo de autogestion de empresas a futuro.

### 2. Tabla de Configuracion del Sistema
- Crear una tabla `SYSystemConfig` (o similar) para centralizar todas las configuraciones del sistema.
- Con prioridad: **modelos de contrato** — guardar ahi el contrato modelo (template) para que el sistema rellene los espacios dinamicamente en lugar de tener el HTML hardcodeado en `contract-generator.js`.
- Campos sugeridos: `Key` (string unico), `Value` (text/JSON), `Category` (string), `Description`, `IsActive`.
- Ejemplos de configuracion: `contract_template_default` (HTML del contrato), `company_name` ("Trato Directo"), `company_tax_id`, `company_address`, etc.
- El generador de contratos debe leer el template desde la BD y reemplazar variables (`{{CompanyName}}`, `{{PlanName}}`, `{{Price}}`, etc.) en lugar de usar un template fijo en JS.

### 3. Consulta a IA y al Agente de RH — HECHO (Dsiezar, 07-Sep) + validado (Iluna, 08-Sep)
- ~~Consultar a la IA y al agente de Recursos Humanos (ver `.agents/rh.md`) si lo llevado hasta ahora cumple con lo alineado a **Trato Directo**.~~ → `docs/rh/alineacion-trato-directo-y-arranque.md`
- ~~Evaluar: que falta, por que no hemos iniciado el proceso de reclutamiento e investigacion, y que pasos siguen.~~ → mismo doc; hallazgo principal: el gate de busqueda no exigia verificacion como piso obligatorio
- El Embudo Ciego (Iluna, 08-Sep) implementa la conclusion: la empresa solo ve conteos y TD entrega personal verificado. Validado en `docs/rh/validacion-flujo-negocio-2026-09-08.md`

### 1 y 2 (CRUD de Planes, tabla `SYSystemConfig` para templates de contrato) — siguen pendientes
- La rama `iluna-embudo-ciego` (`40e2bc5`, 09-Sep) adelanta parte del punto 2 con la feature de **Contrato de vacantes** (`PTVacancyContract`), pero el template sigue sin salir de codigo a una tabla de configuracion. Pendiente de integrar a `main`.

### 4. Dos funcionalidades nuevas a futuro, apagadas por defecto (pedido de Darwin, 18-Sep)

No construir ahora — dejar documentado como intencion de producto para activar mas adelante mediante un parametro de configuracion (la tabla `SYSystemConfig` del punto 2, o un mecanismo equivalente si se construye antes que esa tabla). Ninguna de las dos debe quedar habilitada por defecto: el codigo que las implemente debe leer el flag y comportarse igual que hoy mientras este apagado.

- **Plan de Prioridad para Candidatos (5.99 EUR)** — el candidato paga para que su perfil tenga prioridad de contratacion dentro de Trato Directo (por ejemplo, peso extra en el ranking de `CompatibilityService.CalculateMatchesForVacancyAsync`/`GenerateShortlist`, o aparecer primero en "Cumplen sin postularse"). Hoy no existe ninguna entidad ni campo para esto — haria falta algo como un `IsPriority`/fecha de vigencia en `PT_Candidates` o una tabla de suscripcion dedicada, mas el cobro (no hay pasarela de pagos integrada todavia, ver Fase 7).
- **Autogestion de empresas via planes Basic/Premium/Platinum** — el catalogo `PT_Plans` ya existe (sembrado 06-Sep por Iluna) pero sin uso real: la etapa "Propuesta Enviada" del CRM usa el catalogo de Precios y Niveles de Precio para venta directa por posicion (decision de Darwin, 17-Sep, ver Observacion #1 arriba), no `PT_Plans`. Cuando se decida activar la autogestion, `PT_Plans` es el punto de partida — falta el CRUD, el checkout, y definir que desbloquea cada nivel.
- Flag propuesto: `feature_candidate_priority_plan_enabled` y `feature_company_self_service_plans_enabled` (u otros nombres, a definir junto con `SYSystemConfig`) — controlan si estas dos opciones aparecen en la UI y si la logica de negocio asociada corre.

---

## Datos de prueba (Seed Data)

> **Importante:** Ejecutar `docs/seed-data.sql` despues de aplicar todas las migraciones. Los hashes BCrypt se deben generar registrando los usuarios via API y copiando el hash. Ver procedimiento en el script.

### Pasos para cargar los datos de prueba (instrucciones para el equipo)

> **Nota para Dsiezar:** Corre estos pasos en tu maquina local para tener los mismos datos de prueba. Ya el script `docs/seed-data.sql` esta en `main`.

#### Paso 1: Hacer pull de main

```bash
git pull origin main
```

#### Paso 2: Aplicar migraciones (si faltan)

```bash
dotnet ef database update --project src/OpenToWork.Models --startup-project src/OpenToWork.API
```

Esto aplica las migraciones `InitialCreate`, `Phase2`, `Phase2Security` y `AdminAuditLog`.

#### Paso 3: Ejecutar el script seed-data.sql en MySQL

```powershell
Get-Content docs\seed-data.sql -Raw | C:\xampp\mysql\bin\mysql.exe -u root OpenToWorkDb
```

> Si tienes MySQL en otra ruta, ajusta la ruta del ejecutable. En Linux/Mac: `mysql -u root OpenToWorkDb < docs/seed-data.sql`

#### Paso 4: Generar hashes BCrypt validos

El script inserta usuarios con un hash temporal que no es BCrypt valido. Para que el login funcione, hay que registrar usuarios temporales via la API y copiar el hash:

1. **Iniciar la API principal:**
   ```bash
   dotnet run --project src/OpenToWork.API
   ```

2. **Registrar usuarios temporales (postulantes):**
   ```powershell
   $candidates = @(
       @{email="juan.perez.test@gmail.com";firstName="Juan";lastName="Perez"},
       @{email="maria.gonzalez.test@hotmail.com";firstName="Maria";lastName="Gonzalez"},
       @{email="carlos.rodriguez.test@outlook.com";firstName="Carlos";lastName="Rodriguez"}
   )
   foreach ($c in $candidates) {
       $body = @{email=$c.email;password="Candidato123!";firstName=$c.firstName;lastName=$c.lastName} | ConvertTo-Json
       Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" -Method Post -Body $body -ContentType "application/json"
   }
   ```

3. **Registrar usuarios temporales (empresas):**
   ```powershell
   $companies = @(
       @{email="techcorp.test@gmail.com";firstName="Tech";lastName="Corp"},
       @{email="innovate.test@gmail.com";firstName="Innovate";lastName="Labs"},
       @{email="globalsoft.test@gmail.com";firstName="Global";lastName="Soft"}
   )
   foreach ($c in $companies) {
       $body = @{email=$c.email;password="Empresa123!";firstName=$c.firstName;lastName=$c.lastName} | ConvertTo-Json
       Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" -Method Post -Body $body -ContentType "application/json"
   }
   ```

4. **Copiar los hashes a los usuarios reales y eliminar los temporales:**
   ```sql
   -- Ejecutar en MySQL
   UPDATE SC_Users u1 JOIN SC_Users u2 ON u2.Email = 'juan.perez.test@gmail.com'
       SET u1.PasswordHash = u2.PasswordHash WHERE u1.Email = 'juan.perez@gmail.com';
   UPDATE SC_Users u1 JOIN SC_Users u2 ON u2.Email = 'maria.gonzalez.test@hotmail.com'
       SET u1.PasswordHash = u2.PasswordHash WHERE u1.Email = 'maria.gonzalez@hotmail.com';
   UPDATE SC_Users u1 JOIN SC_Users u2 ON u2.Email = 'carlos.rodriguez.test@outlook.com'
       SET u1.PasswordHash = u2.PasswordHash WHERE u1.Email = 'carlos.rodriguez@outlook.com';
   UPDATE SC_Users u1 JOIN SC_Users u2 ON u2.Email = 'techcorp.test@gmail.com'
       SET u1.PasswordHash = u2.PasswordHash WHERE u1.Email = 'empresa@techcorp.com';
   UPDATE SC_Users u1 JOIN SC_Users u2 ON u2.Email = 'innovate.test@gmail.com'
       SET u1.PasswordHash = u2.PasswordHash WHERE u1.Email = 'contacto@innovatelabs.com';
   UPDATE SC_Users u1 JOIN SC_Users u2 ON u2.Email = 'globalsoft.test@gmail.com'
       SET u1.PasswordHash = u2.PasswordHash WHERE u1.Email = 'rrhh@globalsoft.com';

   DELETE FROM SC_Users WHERE Email LIKE '%.test.%';
   DELETE FROM PT_Candidates WHERE SCUserId NOT IN (SELECT Id FROM SC_Users);
   ```

5. **Crear usuario admin (si no existe):**
   ```powershell
   $body = @{email="admin@opentowork.com";password="Admin123!";firstName="Admin";lastName="System"} | ConvertTo-Json
   Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" -Method Post -Body $body -ContentType "application/json"
   ```
   Luego en MySQL:
   ```sql
   UPDATE SC_Users SET PrimaryRole = 2 WHERE Email = 'admin@opentowork.com';
   ```

#### Paso 5: Verificar

```powershell
C:\xampp\mysql\bin\mysql.exe -u root -e "SELECT Email, PrimaryRole, IsActive FROM SC_Users WHERE IsDeleted=0 ORDER BY Email;" OpenToWorkDb
```

Deberias ver 7 usuarios: 1 admin, 3 empresas, 3 postulantes. Todos con `IsActive = 1`.

#### Paso 6: Ejecutar los 4 proyectos y probar

```bash
# Terminal 1
dotnet run --project src/OpenToWork.API
# Terminal 2
dotnet run --project src/OpenToWork.WEB
# Terminal 3
dotnet run --project src/OpenToWork.AdminAPI
# Terminal 4
dotnet run --project src/OpenToWork.AdminWEB
```

- Portal candidatos: `http://localhost:5100` (login con juan.perez@gmail.com / Candidato123!)
- Portal admin: `http://localhost:5101` (login con admin@opentowork.com / Admin123!)

### Credenciales de prueba

#### Portal Admin (AdminWEB - puerto 5101)

| Campo | Valor |
|-------|-------|
| URL | `http://localhost:5101` |
| Email | `admin@opentowork.com` |
| Password | `Admin123!` |

#### Portal de Candidatos (WEB - puerto 5100)

| Postulante | Email | Password |
|------------|-------|----------|
| Juan Perez | `juan.perez@gmail.com` | `Candidato123!` |
| Maria Gonzalez | `maria.gonzalez@hotmail.com` | `Candidato123!` |
| Carlos Rodriguez | `carlos.rodriguez@outlook.com` | `Candidato123!` |

#### Empresas (para probar portal corporativo cuando este listo)

| Empresa | Email | Password |
|---------|-------|----------|
| TechCorp Solutions | `empresa@techcorp.com` | `Empresa123!` |
| Innovate Labs | `contacto@innovatelabs.com` | `Empresa123!` |
| GlobalSoft Inc. | `rrhh@globalsoft.com` | `Empresa123!` |

### Datos disponibles en la BD

| Tabla | Cantidad | Detalle |
|-------|----------|---------|
| `SC_Users` | 7 | 1 admin, 3 empresas, 3 postulantes |
| `PT_Companies` | 3 | TechCorp, Innovate Labs, GlobalSoft |
| `PT_Vacancies` | 10 | 8 activas, 1 draft, 1 cerrada |
| `PT_TempVacancies` | 3 | Freelance UX, contrato full stack, part-time CM |
| `PT_Skills` | 20 | C#, .NET, React, Python, Docker, etc. |
| `PT_Candidates` | 3 | 2 con wizard completo, 1 incompleto |
| `PT_Applications` | 5 | 1 reviewing, 1 aceptada, 3 pendientes |

---

## Notas de diseno (referencia Cazvid)

El panel administrativo debe inspirarse en **Cazvid** (cazvid.com/features/ats) para los flujos de gestion:

- **Pipeline visual:** Aplicantes movidos entre estados (Applied, Screening, Interview, Offer, Hired) con drag-and-drop
- **Card de aplicante:** Resume, skills, score, info de contacto en una sola vista
- **Filtros rapidos:** Por rating, por estado, por score - un solo clic
- **Notas y seguimiento:** Notas internas, log de llamadas, recordatorios
- **Mensajeria integrada:** Conversaciones adjuntas al historial del aplicante

### Pendiente de diseno en AdminWEB

1. **Filtros en todas las tablas** - busqueda por texto, filtro por estado, filtro por fecha
2. **Mejorar tablas** - columnas ordenables, paginacion visible, densidad configurable
3. **Vista de aplicaciones** - pipeline visual estilo Kanban en vez de tabla
4. **Card de usuario/detalle** - panel lateral con info completa al hacer clic
5. **Dashboard avanzado** - graficos, tendencias, no solo numeros

---

## Documentacion

| Documento | Descripcion |
|---|---|
| `docs/BUSINESS_PROPOSAL.md` | Propuesta de negocio y producto - plataforma de evaluacion de talento |
| `docs/PLAN_DE_PROYECTO.md` | Plan de proyecto con 3 portales y 8 fases |
| `docs/PRD.md` | Product Requirements Document - requisitos del producto |
| `docs/TRN.md` | Technical Requirements Note - requisitos tecnicos |
| `docs/APPFLOW.md` | Diagramas de flujo de la aplicacion |
| `docs/IMPLEMENTACION.md` | Guia de implementacion paso a paso |
| `docs/DATABASE_DESIGN.md` | Diseno completo de la base de datos |
| `docs/DESIGN_SYSTEM.md` | Sistema de diseno (UI/UX, temas, componentes) |
| `docs/NEURAL_MAP.md` | Mapa neuronal del proyecto para IA |
| `docs/DEPLOYMENT.md` | Guia de despliegue a Windows Server / IIS (Web Deploy, PSRemoting, CI/CD) |
| `docs/OpenToWork_InitialCreate.sql` | Script SQL inicial de la base de datos |

---

## Convenciones

- **Tablas:** Prefijos `SC_` (Security), `PT_` (Portal), `SY_` (System), `AD_` (Admin), `CO_` (Corporate), `VR_` (Verification)
- **Auditoria:** Todas las tablas tienen `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `IsDeleted`, `DeletedAt`, `DeletedBy`
- **Soft delete:** No se usa `DELETE` fisico, solo `IsDeleted = true`
- **i18n:** Prohibido texto hardcoded en `.razor`. Usar `Lang.T("section.key")`
- **Nombres:** Tablas y columnas en ingles

---

## NOTA

> **Importante para cualquier IA o desarrollador que trabaje en este proyecto.**

### 1. Usa los agentes predefinidos

Este proyecto tiene 4 agentes definidos en `.agents/`. Antes de empezar a trabajar, revisa y sigue el rol que te corresponda:

| Agente | Archivo | Rol |
|---|---|---|
| **PM** | `.agents/pm.md` | Administrador del proyecto, controla el flujo |
| **QA** | `.agents/qa.md` | Tester: valida diseno, funcionalidad, calidad |
| **FS** | `.agents/fs.md` | Full Stack: implementa frontend y backend |
| **SEC** | `.agents/sec.md` | Seguridad: audita vulnerabilidades |

Lee `.agents/WORKFLOW.md` para entender el flujo de 8 etapas (0 a 7) por fase.

### 2. Declara tu identidad

Al iniciar una sesion, declara quien eres: **"Soy Iluna"** o **"Soy Dsiezar"**. Documenta todos tus cambios en:

- `docs/iluna/fase-N.md` si eres Iluna
- `docs/dsiezar/fase-N.md` si eres Dsiezar

Usa `docs/iluna/PLANTILLA.md` o `docs/dsiezar/PLANTILLA.md` como base.

### 3. Documenta la fase en la que estas

Siempre documenta en tu bitacora que fase estas trabajando, en que etapa del flujo estas, y que cambios realizaste. Sin documentacion, el trabajo no cuenta como completado.

### 4. Revisa dependencias antes de empezar

Antes de iniciar una fase, **revisa si esa fase depende de la otra persona**. Si tu fase necesita algo que la otra persona aun no ha terminado, comunicalo para no atascarte.

- **Minimiza dependencias** entre tu fase y la del otro.
- Si puedes trabajar de forma independiente, mejor.
- Si hay una dependencia critica, acuerden un punto de integracion antes de empezar.

### 5. Cada fase es una rama

**Nunca trabajes directamente en `main`.** Cada fase tiene su propia rama con el formato:

```
{ia}-{fase}
```

Ejemplos:
- `iluna-fase-2` - Iluna trabajando en Fase 2
- `dsiezar-fase-2` - Dsiezar trabajando en Fase 2

Solo se hace merge a `main` cuando la fase esta 100% completada y aprobada por PM, QA y SEC.

Ver `docs/GIT_BRANCHES.md` para mas detalles.

### 6. Tratemos de no usar muchas dependencias entre fases

Si ambos estan trabajando en paralelo, cada uno debe poder avanzar sin bloquear al otro. Diseñen las tareas de manera que las dependencias cruzadas sean minimas. Si una dependencia es inevitable, definan un contrato (interface, DTO, endpoint) antes de empezar para que ambos puedan trabajar contra el contrato.

---

## RH — Análisis: Portafolio de Candidatos de Calidad

> **Soy RH.** Análisis del proyecto desde Reclutamiento y Selección.
> Ver documento completo: [`docs/rh/analisis-portafolio-candidatos.md`](docs/rh/analisis-portafolio-candidatos.md)

### Lo que YA existe

- Registro de candidato (Wizard 10 pasos)
- Perfil con experiencia, educación, certificaciones y skills
- URLs de LinkedIn, portafolio y CV (sin verificación real)
- Vacantes, postulaciones y mensajería (mock)
- Dashboard admin con métricas y vista de perfil en modo lectura

### Módulos FALTANTES (priorizados)

#### 🔴 Críticos — Sin esto no hay portafolio de calidad

| Módulo | Descripción | Fase |
|--------|-------------|------|
| **Scorecard de Competencias** | Escala 1-5 por competencia técnica y blanda, rubrica objetiva, comparación candidato vs. vacante | Fase 3 |
| **Evaluación Práctica (Retos)** | Banco de retos técnicos por categoría, timer, anti-copia, puntaje automático | Fase 3 |
| **Verificaciones Reales** | Identidad (documento/video), experiencia (referencias), educación (instituciones), badges de confianza | Fase 3 |
| **Índices de Scoring** | Estabilidad, Confiabilidad, Evidencia, Compatibilidad — los 4 índices que diferencian a OpenToWork | Fase 3 |
| **Pipeline ATS (Kanban)** | Applied → Screening → Interview → Offer → Hired, drag-and-drop, notas, log de actividad | Fase 4 |

#### 🟡 Alta prioridad — Diferenciador competitivo

| Módulo | Descripción | Fase |
|--------|-------------|------|
| **Video Pitch** | Grabación 30-60s desde el portal, almacenamiento cloud, moderación admin | Fase 4 |
| **Referencias Laborales** | Candidato agrega 2-3 contactos, sistema envía solicitud, resultado en perfil | Fase 3 |
| **People Analytics** | Time-to-Hire, Quality of Hire, costo por contratación, funnel de conversión, tendencias | Fase 4 |
| **Búsqueda Avanzada** | Booleana (AND/OR/NOT), filtros múltiples, ranking por match, alertas, shortlist | Fase 5 |
| **Match Inteligente** | Algoritmo de compatibilidad candidato-vacante, score 0-100%, recomendaciones automáticas | Fase 5 |

#### 🟢 Media prioridad — Optimización y experiencia

| Módulo | Descripción | Fase |
|--------|-------------|------|
| **Candidate Experience** | Notificaciones automáticas, feedback de rechazo, timeline del proceso, NPS | Fase 4 |
| **Entrevistas Integradas** | Agendamiento, videoentrevistas, plantillas STAR/CAR, evaluación post-entrevista | Fase 5 |
| **Ofertas y Onboarding** | Carta de oferta, firma digital, checklist onboarding, seguimiento 30/60/90 días | Fase 5 |
| **Detección de Red Flags** | Análisis de saltos laborales, incongruencias, score de riesgo de rotación | Fase 3 |
| **Employer Branding** | Perfil de empresa con cultura, reseñas, rating, estadísticas públicas | Fase 5 |

### Preguntas estratégicas para el equipo

> Ver las 17 preguntas completas en [`docs/rh/analisis-portafolio-candidatos.md`](docs/rh/analisis-portafolio-candidatos.md#4-preguntas-estratégicas-para-el-equipo)

**Modelo de negocio:**
1. ¿El portafolio es gratuito para candidatos y pago para empresas?
2. ¿Qué módulos son del plan gratuito vs. premium?
3. ¿Se cobra por candidato contratado o por suscripción mensual?

**Datos y privacidad:**
4. ¿Quién es dueño de los datos del candidato?
5. ¿El candidato puede eliminar su perfil y todos sus datos? (GDPR/Ley 25.326)
6. ¿Las notas internas de reclutadores son accesibles al candidato?

**Evaluación y scoring:**
7. ¿El scoring es transparente para el candidato?
8. ¿El candidato puede apelar un score bajo?
9. ¿Con qué frecuencia se recalcula el score?

**Competencia:**
10. ¿Qué nos diferencia de LinkedIn, Computrabajo, Bumeran?
11. ¿El video pitch o el scoring es el diferenciador principal?

### Recomendación de RH

> **OpenToWork tiene una base sólida de datos del candidato, pero le falta la capa de evaluación y confianza que justifica su propuesta de valor.** Sin scoring, sin verificaciones reales y sin evaluación práctica, la plataforma es una bolsa de empleo más. La Fase 3 (Motor de Evaluación) es el bloque crítico que convierte los datos en decisiones de contratación confiables.

---

## Bitácora de Cambios

### Sesión 2026-09-18 — Paso 7 re-evaluado: ya estaba construido (Dsiezar)

Cierra formalmente la **Auditoría del Ciclo Comercial completa** (25/25). No fue necesario escribir código nuevo — el paso llevaba semanas resuelto bajo un nombre distinto al que describía la evaluación anterior del gap-analysis.

El README decía: "existe `CandidateSearchService`, pero solo filtra por score/skills genéricos, no por los requisitos de una vacante puntual, y vive en la API pública no en el admin". Esa evaluación miraba el servicio equivocado — `CandidateSearchService` es el buscador genérico que usa la **empresa** desde su portal (`/candidate-search`), sin relación con una vacante puntual, y es correcto que ese no resuelve el paso 7.

Pero existe un segundo motor, completamente distinto, construido en una fase anterior para la función "Calcular Matches" del shortlist: `CompatibilityService.CalculateMatchesForVacancyAsync` consulta exactamente la base de datos prevalidada (`PT_Candidates` con `IsProfilePublic && WizardCompleted`) y calcula un `MatchPercentage` contra los requisitos puntuales de esa vacante (skills requeridas pesan el doble que las opcionales, bucket de años de experiencia, ubicación/modalidad). `GetNonApplicantMatchesAsync` filtra ese resultado a quienes **no han postulado todavía** — es decir, candidatos que ya están en la base y cumplen, pero TD todavía no los contactó para esta vacante en particular.

Esto vive en el **admin** (no en la API pública), en la pestaña "Cumplen sin postularse" de la ficha de vacante (`/vacancies/{id}`, `VacancyDetail.razor`), con filtro de % mínimo de match (todos/≥50%/≥70%/≥80%) y un botón "Postular seleccionados" para llevar directamente a esos candidatos al pipeline de esa vacante.

Verificado en vivo contra una vacante de prueba ("Camarero", Hostal Costa Brava, Junior, 2 años de experiencia, inglés requerido): "Cumplen sin postularse" pasó de 0 a 2 al ejecutar "Calcular matches", mostrando el desglose real de Skills/Experiencia/Ubicación por candidato.

No se tocó código — solo se corrigió la evaluación en este README.

### Sesión 2026-09-18 — Fecha de Contratación candidato-empresa (paso 16) (Dsiezar)

Cierra el último ítem faltante (no parcial) de la Auditoría del Ciclo Comercial. **Decisión de Darwin:** Trato Directo no formaliza ni gestiona el contrato laboral entre el candidato y la empresa — eso lo hace la empresa por su cuenta. Lo mínimo que el sistema debe registrar es la fecha en que la empresa contrata formalmente al candidato.

Se agregó `HiringDate` a `PTNegotiation` y `PTCandidateDelivery` (migración `HiringDate`), deliberadamente **independiente** de `IncorporationDate` (que ya existía desde el 13-Sep y representa el primer día de trabajo) — la empresa puede firmar el contrato antes de que el candidato empiece a trabajar, son dos hitos distintos. Botón "Registrar fecha de contratación" junto al de "Registrar incorporación", mismo patrón, sin gating entre ambos (se pueden registrar en cualquier orden o ninguno).

Verificado end-to-end en el flujo de Negociaciones (vacante "Camarero"/Xian tian Di, candidato Juan Perez): negociación cerrada → "Contratado el 10/09/2026" registrado → "Incorporado el 17/09/2026" registrado por separado → ambas fechas conviven sin afectar el cálculo de garantía (que sigue usando solo `IncorporationDate`) ni el resto del flujo (el botón "Activar Garantía de Reposición" sigue disponible con normalidad).

- Commit `c07ce98` en `dsiezar-fase-5`, merge fast-forward a `main`.

### Sesión 2026-09-18 — Cierre del Proceso post-garantía y Feedback de Mejora Continua (pasos 21 y 22) (Dsiezar)

Cierra por completo la Auditoría del Ciclo Comercial: los últimos dos pasos del flujo oficial que quedaban sin construir. Sesión iniciada probando en vivo cómo funciona la Garantía de Reposición (paso 20) con el usuario, lo que llevó a confirmar y corregir de paso el dato de garantía de "Encargados y Técnicos" en `/pricing/job-levels`: estaba en 40 días, se corrigió a 45 (vía UI, sin tocar código) para que coincida con el documento oficial (30/45/60).

**Paso 21 — Cierre del Proceso (post-garantía):** antes no había ningún estado que distinguiera "cerrado exitoso post-garantía" de simplemente `Status=Cerrada`/`Hired`. Se agregó `ProcessClosedAt`/`ProcessClosedByUserId`/`ProcessClosureNotes` a `PTNegotiation` y `PTCandidateDelivery` (migración `ProcessClosure`). Botón "Cerrar Proceso" junto al badge de garantía (mismo lugar que "Activar Garantía de Reposición"), habilitado solo cuando: el proceso está Cerrada/Hired, la garantía ya venció (o no hay garantía definida en el contrato), y no hay una reposición en curso sin resolver. La regla (`CanCloseProcess`) se calcula en el servidor, no se duplica en el cliente.

**Paso 22 — Feedback y Mejora Continua:** no existía ningún registro de satisfacción del cliente. Se agregó `FeedbackRating` (1-5)/`FeedbackComments`/`FeedbackRecordedAt`/`FeedbackRecordedByUserId` a las mismas dos entidades (migración `ProcessFeedback`). Botón "Registrar Feedback", habilitado solo una vez que el proceso ya está cerrado (paso 21) y no se registró feedback antes.

Ambos pasos siguen el mismo patrón ya usado para la Garantía de Reposición: un formulario inline a la vez, sin controllers nuevos (endpoints agregados a `NegotiationsController`/`DeliveriesController` ya existentes, mismos roles Comercial/Reclutador), y cubriendo los dos flujos de "contratado" (Negociaciones y Entregas) por igual.

Verificado end-to-end en el flujo de Negociaciones (vacante "Camarero de Sala"/Las Brasas, candidato Donald): negociación presentada → cerrada → incorporación registrada con fecha pasada (01/06/2026, sin garantía definida en el contrato de prueba) → "Cerrar Proceso" aparece y funciona ("Proceso cerrado el 18/09/2026") → "Registrar Feedback" aparece recién después de cerrado, no antes → feedback 5/5 con comentario guardado y mostrado correctamente. El flujo de Entregas usa el mismo código (`CloseProcessDto`/`RecordFeedbackDto` compartidos) y compila limpio, pero no se pudo probar en vivo en esta sesión por una limitación de UI ya documentada (una vacante Cerrada pierde el toggle "Cola de Shortlist" en `/vacancies`, y el estado de qué vacante está expandida es solo del cliente) — pendiente de una prueba en vivo si se decide construir una vista de negociación/entrega que no dependa de ese toggle.

- Commits `1146468` (feature) en `dsiezar-fase-5`, merge fast-forward a `main`.

### Sesión 2026-09-18 — Fix: "Cola de Shortlist" invisible en vacantes cerradas (Dsiezar)

Resuelve la limitación de UI documentada en la sesión anterior, que había impedido probar en vivo el "Cierre del Proceso"/"Feedback" sobre una vacante ya `Cerrada`. Causa raíz: el botón "Cola de Shortlist" en `/vacancies` estaba condicionado a `Status == Active`, dejando todo el panel de negociaciones (garantía, incorporación, contratación, cierre de proceso, feedback) inaccesible desde la lista una vez cerrada la vacante — el resto del panel no tenía ningún gating propio, solo el botón que lo abre.

Se quitó el guard; el botón ahora se muestra sin importar el estado de la vacante. Verificado en vivo: vacante "Camarero/a para restaurante de tapas" (Cerrada, Test Company Inc) → "Cola de Shortlist" → panel completo visible y funcional (negociación con Donald, "Incorporado el 01/06/2026", "Sin garantía definida en el contrato", "Proceso cerrado el 18/09/2026", botón "Registrar Feedback"). De paso se confirmó que `Candidates/PipelineDetail.razor` (flujo de Entregas) nunca tuvo un guard equivalente — no depende del estado de la vacante para mostrar sus controles, así que el flujo de Entregas no estaba bloqueado por este mismo bug.

- Commit `c33772f` en `dsiezar-fase-5`, merge fast-forward a `main`.

### Sesión 2026-09-18 — Fix: guard `Lang.InitializeAsync()` en Cartera de Clientes (Dsiezar)

Mismo bug ya corregido el 17-Sep en `Payments.razor` (texto sin traducir — claves crudas en vez del idioma — en un arranque en frío del circuito Blazor), pendiente de aplicar en `Portfolio.razor`/`PortfolioDetail.razor` ("Cartera de Clientes", construidas en esta sesión). Se agregó el mismo guard (`if (Lang._translations.Count == 0) await Lang.InitializeAsync();` antes de `LoadAsync()`) a ambos `OnInitializedAsync`. Verificado con un reinicio real del servidor (circuito genuinamente frío) navegando directo a `/portfolio` y a `/portfolio/{comercialId}`: todo el texto renderiza traducido, sin claves crudas.

- Commit `b2d8591` en `dsiezar-fase-5`, merge fast-forward a `main`.

### Sesión 2026-09-18 — Decisión: Validación y Consolidación quedan manuales (Dsiezar)

Único punto que quedaba `[ ]` en "Políticas del documento oficial": el disparo automático de los tramos de pago Validación (al cerrar la negociación) y Consolidación (a los 30 días de incorporación). Pregunta directa a Darwin — respuesta: **quedan a criterio manual del admin**, no se automatiza. La Apertura (30%) sigue siendo la única con gate automático real (bloquea publicar la vacante), por ser la única que el diagrama oficial marca con un rombo de decisión.

No hubo cambio de código — solo se cerró la evaluación en el README, sin dejar ningún ítem abierto en la Auditoría del Ciclo Comercial ni en sus políticas asociadas.

### Sesión 2026-09-18 — 6 Indicadores de Negocio en el panel principal (Dsiezar)

Darwin: el panel principal (`Dashboard.razor`) solo tenía métricas de completitud de perfiles de candidatos — nada relevante para el dueño de la plataforma. Se agregó una sección nueva "Indicadores del Negocio" (arriba del todo, antes de "Perfiles"), con 6 tarjetas construidas sobre datos reales:

1. **Ingresos Cobrados vs Pendiente** — suma de `PT_ContractPayments` por estado.
2. **Tasa de Cierre Comercial** — Cerrado Ganado vs Cerrado Perdido del pipeline de empresas.
3. **Tiempo Promedio de Contratación (días)** — desde `PT_Vacancy.PublishedAt` hasta `HiringDate`, combinando Negociaciones y Entregas.
4. **Tasa de Éxito de Colocación** — 100% menos el % de contrataciones que necesitaron una reposición de garantía no excluida.
5. **Valor en Pipeline Abierto** — suma de `FeeAmount` de contratos todavía en Draft/Sent (dinero que entraría si firman).
6. **Empresas Estancadas (30+ días)** — empresas en pipeline abierto (no Ganado/Perdido) sin cambio de etapa reciente, señal de riesgo de perder el trato por falta de seguimiento.

Nuevo endpoint `GET /api/admin/dashboard/business-metrics` (`BusinessMetricsDto`, `AdminDashboardService.GetBusinessMetricsAsync`), separado del endpoint de métricas operativas existente. Tarjetas de Ingresos y Empresas Estancadas navegan a `/payments` y `/companies/pipeline` respectivamente.

Verificado en vivo contra MySQL real: 378€ cobrados / 1,332€ pendientes, 100% de cierre (6/0), 83% de éxito de colocación (1 de 6 con reposición), 900€ en pipeline abierto (2 contratos sin firmar). El indicador de Tiempo Promedio de Contratación mostró "-" porque el único dato de prueba con `HiringDate` tiene una fecha anterior a la publicación de su vacante (dato cargado manualmente durante testing, no un caso real) — el cálculo excluye duraciones negativas a propósito, así que se llenará correctamente con datos reales.

**Hallazgo aparte (no corregido, fuera de alcance de hoy):** durante la verificación, el servidor de AdminWEB se cayó por completo (`Unhandled exception`, proceso terminado) por un bug pre-existente en `Companies/Pipeline.razor` (pantalla de Iluna) — el debounce del buscador llama `StateHasChanged()` desde un hilo que no es el del Dispatcher de Blazor, una excepción no controlada que mata el proceso entero para todos los usuarios. Reportado a Darwin, pendiente de que decida si se corrige (requiere tocar una pantalla de Iluna).

- Commit `98daa03` en `dsiezar-fase-5`, merge fast-forward a `main`.

### Sesión 2026-09-17 — Exigir vacante registrada antes de avanzar a Propuesta Enviada (Dsiezar)

Pregunta de Darwin que destapó un hueco real: "¿en qué momento voy a ingresar la vacante que necesita la empresa, con sus requisitos?". Investigación: existían dos caminos para cargar una vacante (el wizard de "Captación" en `/companies/new`, que crea empresa+vacante+contrato de una vez; y "Ver Vacantes" en la ficha de una empresa ya existente, con botón "+ Nueva vacante") pero **ninguno de los dos estaba conectado al pipeline de ventas**. Llegar a "Cerrado Ganado" solo ofrecía "Generar Contrato" (que busca un contrato *ya existente* y falla si no hay ninguno) — nada en el flujo Lead→...→Cerrado Ganado le pedía al admin cargar la vacante.

**Decisión de Darwin:** después de "Reunión Agendada", avanzar al siguiente nivel debe llevar a inscribir la vacante, y de ahí seguir hasta cerrar el trato.

**Implementación (`Companies/PipelineDetail.razor`):**
- Nueva sección "Vacantes de la empresa" (conteo + listado + botón "+ Nueva vacante") visible en el panel de las etapas "Reunión Agendada" y "Propuesta Enviada", y también dentro del modal de avance — reutiliza `GetVacanciesAsync(companyId:...)` y el wizard ya existente de `Vacancies.razor`, sin duplicar UI.
- Gate nuevo: avanzar de "Reunión Agendada" a "Propuesta Enviada" ahora exige, además de la tarifa (ver sesión de abajo), al menos 1 vacante registrada para la empresa — mismo patrón que los demás gates del modal (método de contacto, fecha de reunión).
- `Vacancies.razor`: el wizard "+ Nueva vacante" preselecciona la empresa cuando se llega desde `/companies/{id}/vacancies`, en vez de obligar a elegirla de nuevo entre 200 opciones.

**Verificado end-to-end:** empresa de prueba en "Reunión Agendada" con 0 vacantes → botón "Confirmar avance" deshabilitado aun con tarifa y comentario completos → se registra una vacante ("Camarero de Sala") desde el mismo modal → botón habilitado → avance confirmado a "Propuesta Enviada", con la vacante visible junto a las tarjetas de tarifa.

**Siguiente eslabón de la misma cadena — "Generar Contrato" en Cerrado Ganado:** ese botón solo buscaba un contrato *ya existente* (`GetContractByCompanyAsync`) y mostraba un error generico si no habia ninguno, sin llevar al admin a ningun lado util para crearlo (la creacion real vive en `/vacancies/{id}/contract`, un `@page` aparte). Con el gate de arriba, al llegar a Cerrado Ganado la empresa ya tiene garantizada al menos 1 vacante, asi que ahora:
- Con 1 sola vacante, "Generar Contrato" navega directo a `/vacancies/{id}/contract`.
- Con mas de una, avisa que elija una y lleva a `/companies/{id}/vacancies` para elegirla.
- Sin ninguna (caso limite que ya no deberia ocurrir), mantiene el aviso original.

Tambien se agrego el bloque "Vacantes de la empresa" al panel de Cerrado Ganado. Verificado end-to-end: la misma empresa de prueba, ya en Cerrado Ganado con su vacante "Camarero de Sala" → "Generar Contrato" navega directo a `/vacancies/{vacancyId}/contract`.

- Commits `6c71044` y `dd17a44` en `dsiezar-fase-5`, merge fast-forward a `main`.

### Sesión 2026-09-17 — Bento Grid en Pagos + Tarifas reales en la Propuesta del CRM (Dsiezar)

**Pagos (`/payments`):** rediseño visual con el patrón Bento Grid (`admin-stat-grid`) ya usado en otras pantallas del admin, agregando una 4ª tarjeta (Total de Tramos) a las 3 que ya existían (Monto Total/Pagado/Pendiente). De paso, verificación en vivo tras cada cambio encontró y corrigió 3 defectos reales: `GetTrancheLabel` no tenía el caso para `ReposicionSegunda` (mostraba "-" para los cargos de la Garantía de Reposición del 16-Sep); faltaban las claves `admin.common.filter`/`previous`/`next` (se mostraba la clave cruda); y faltaba el guard `Lang.InitializeAsync()` en `OnInitializedAsync`, causando texto sin traducir en un arranque en frío del circuito (bug intermitente, solo visible al navegar directo a `/payments` sin login previo en esa sesión). También se agregó quién marcó un tramo como pagado (`PaidByName`) bajo el badge "Pagado".

**CRM — etapa "Propuesta Enviada" (`Companies/PipelineDetail.razor`):** hasta ahora esa etapa ofrecía 3 planes fijos de `PT_Plans` (Basic 49€/Premium 99€/Platinum 199€, sembrados a mano en 2026-09-06, sin CRUD ni relación con los precios reales de los contratos — ver punto pendiente "1. CRUD de Planes" en Observaciones). Decisión de Darwin: el modelo actual es venta directa por posición cubierta, no autogestión por planes de suscripción — los precios de `PT_Plans` quedan reservados para un futuro módulo de autogestión de empresas. En su lugar, esa etapa ahora consume el catálogo real y ya editable de **Precios y Niveles de Precio** (`PTJobLevel`/`PTJobType`, el mismo que usan los contratos en `/pricing/job-levels` y `/pricing/job-types`): las 3 tarjetas muestran cada nivel de puesto con su precio "desde" (el mínimo vigente entre sus tipos de puesto) y su cobertura/garantía de referencia. El texto que queda grabado en el historial de etapas pasa de `[PLAN: Basic]` a `[TARIFA: Personal Operativo - desde 450€]`. `PT_Plans`/`GetPlansAsync()` no se tocan, quedan intactos para cuando exista la autogestión.

Verificado end-to-end contra MySQL real: empresa de prueba "Xian tian Di" en etapa "Reunión Agendada" → modal de avance muestra los 3 niveles reales (450€/1100€/2200€ "desde") → selección de "Personal Operativo" → avance confirmado a "Propuesta Enviada" → historial de etapas registra `[TARIFA: Personal Operativo - desde 450€]` → el panel de resumen de la etapa (fuera del modal) muestra el mismo catálogo real.

- Commit `89573d2` en `dsiezar-fase-5`, merge fast-forward a `main`.

### Sesión 2026-09-16 — Garantía de Reposición (paso 20 del Gap Analysis) (Dsiezar)

Cierra el último bloque grande de la Auditoría del Ciclo Comercial: cuando una colocación falla dentro del período de garantía, un admin puede registrar por qué, activar una reposición gratuita (la 1ª) o cobrada al 50% (la 2ª), y dejar un vínculo real con la nueva colocación que la resuelve — cubre los pasos 20/20.1/20.2/20.3 y 3 bullets más de "Políticas del documento oficial" (exclusiones de garantía, "¿y si el candidato...?", garantía por tipo de perfil).

**Modelo de datos:**
- Nueva tabla `PT_WarrantyReplacements` (migración `WarrantyReplacements`): motivo (`WarrantyReplacementReason`, 10 valores: 6 cubiertos + 4 exclusiones contractuales), si es exclusión, número de reposición (1/2/0), estado (`EnCurso`/`Completada`/`Cancelada`/`ExcluidaDeGarantia`), quién y cuándo la solicitó, vínculo a la colocación original (`PTNegotiation` o `PTCandidateDelivery`) y a la de reemplazo una vez vinculada.
- `PaymentTrancheType` gana `ReposicionSegunda=3` — el cargo del 50% de la 2ª reposición se crea como una fila más en la tabla de tramos de pago ya existente (`PTContractPayment`), reutilizando el mismo panel "Tramos de Pago" y el botón "Marcar como pagado" sin cambios adicionales.

**Lógica (`IWarrantyReplacementService`):**
- Activar desde una negociación cerrada o una entrega Contratada: si el motivo es una exclusión contractual, la reposición queda `ExcluidaDeGarantia` (no cuenta, no reabre la vacante). Si no, cuenta cuántas reposiciones no-canceladas/no-excluidas tiene ya la vacante — 1ª gratis, 2ª genera el cargo del 50%, una 3ª es rechazada ("ya agotó las 2 reposiciones cubiertas por la garantía").
- Activar reabre la vacante (`Status=Active`) para una nueva búsqueda — el vínculo real que pedía el punto 20.2.
- "Vincular" (desde el panel del contrato) conecta la reposición con la nueva negociación/entrega que la resuelve, marcándola `Completada`; "Cancelar" la da de baja sin contar para el tope de 2.

**UI:**
- Botón "Activar Garantía de Reposición" junto al badge de garantía en `Vacancies.razor` (Negociaciones, rol Comercial) y `Candidates/PipelineDetail.razor` (Entregas, rol Reclutador) — formulario inline con los 10 motivos + notas, mismo patrón visual que el resto del panel.
- Panel nuevo "Garantías de Reposición" en `VacancyContract.razor`, debajo de "Tramos de Pago": lista las reposiciones del contrato con motivo, chip de exclusión, número/costo, estado, candidato original → candidato de reposición, y los controles Vincular/Cancelar.
- Roles: los endpoints nuevos se agregaron a los controllers existentes (`NegotiationsController`/`VacancyContractController` = Comercial, `DeliveriesController` = Reclutador), sin crear un controller nuevo.

**Verificado end-to-end** contra MySQL real y ambos flujos (contrato de prueba `TD-2026-0004`, vacante "Camarero", garantía de 90 días): 1ª reposición activada desde una negociación cerrada real (Juan Perez) → vacante reabierta; 2ª reposición sobre la misma vacante → cargo de 450€ (50% de 900€) creado correctamente y visible en Tramos de Pago; 3er intento → rechazado con el mensaje esperado; motivo de exclusión → `ExcluidaDeGarantia`, sin reabrir la vacante; "Cancelar" y "Vincular" probados desde la UI; flujo de Entregas probado directo contra el endpoint (fixture sintético, eliminado después de la prueba) confirmando paridad con Negociaciones. De paso, verificado contra la BD real que los 3 niveles de garantía están sembrados pero con **30/40/60 días, no 30/45/60** — pendiente confirmar con Darwin si hay que corregir el catálogo.

- Commit(s) en `dsiezar-fase-5`, merge fast-forward a `main`.

### Sesión 2026-09-15 — Skills predeterminados por tipo de puesto (Dsiezar)

Cada tipo de puesto del catálogo (Camarero/a, Barman, etc.) puede tener skills predeterminados configurables por admin, que se usan como requisitos por defecto al crear una vacante, tanto desde el portal de empresa como desde la captación de empresa del CRM admin.

**Modelo de datos:**
- Nueva tabla `PT_JobTypeSkills` (join `PTJobType` ↔ `PTSkill`), migración `AddJobTypeSkills`.

**Admin (Pricing > Tipos de Puesto):**
- Botón "Skills predeterminados" por tipo de puesto — modal con checkboxes del catálogo completo de skills.
- Endpoints `GET`/`PUT api/admin/pricing/job-types/{id}/skills`.

**Empresa - Nueva Vacante (`OpenToWork.WEB`):**
- El campo libre "Categoría" se reemplaza por un selector de "Tipo de puesto".
- Al elegir el puesto, sus skills predeterminados aparecen como chips editables (tildar/destildar), guardados en `PT_VacancySkills`.
- Nuevos endpoints públicos de solo lectura `api/job-types` y `api/skills`.
- `Category` se sigue derivando del tipo de puesto (con el código legado que usan los filtros de búsqueda públicos) para no romper búsqueda/filtrado existentes.

**Captación de Empresa (CRM admin, `Companies/Create.razor`):**
- Al elegir el tipo de puesto que busca la empresa, los Requisitos se autocompletan con los skills predeterminados (editable por el admin) y se guardan también como `PT_VacancySkills` estructurados.

**Revisión de código posterior — 10 hallazgos, todos verificados en vivo antes de aplicarlos:**
- `SetJobTypeSkillsAsync` reactiva filas soft-deleted en vez de duplicar (evitaba una violación del índice único al tildar/destildar un skill más de una vez).
- Se excluyen skills soft-deleted de los defaults de un tipo de puesto.
- Validación de tipo de puesto obligatorio en el wizard de empresa.
- `SkillIds` inválidos se filtran antes de insertar (evita 500 por violación de FK).
- `AdminVacancyService.GetByIdAsync` ahora también proyecta `Skills`.
- `MapToDtoAsync` batch-fetchea tipos de puesto y skills (elimina N+1 en listados/búsqueda de vacantes).
- `GET job-types/{id}/skills` valida existencia (400 en vez de 200+`[]`).
- CSS vars inexistentes corregidas en los chips de skills del wizard.
- `OnInitializedAsync` usa `Task.WhenAll` en vez de awaits secuenciales.

**Otros cambios incluidos en este push:**
- Fix del selector de mapa en `Companies/Create.razor` y `PipelineDetail.razor` — el botón "Usar ubicación" no aplicaba la ubicación elegida.
- `scripts/start-dev.bat` / `stop-dev.bat` para levantar/bajar MySQL + los 4 servidores localmente.
- Enums `WarrantyReplacementReason`/`Status` (borrador, sin wiring todavía).

**Nota:** el wizard de captación de empresa es pantalla de Iluna (CRM) — el cambio ahí es lógica de negocio nueva, no solo visual, avisarle.

- Commits `dbf087c`..`013b2df` en `dsiezar-fase-5`, merge fast-forward a `main`.

### Sesión 2026-09-10 — Mejoras del documento contractual + campo ContactDniNie + página de edición de empresa (Iluna)

**Documento contractual (`VacancyContractDocument.razor`) — refinamiento visual y de contenido:**
- Nuevo `ContractLayout.razor` (layout limpio sin sidebar/topbar) para que el documento se abra en ventana emergente sin la mancha gris del admin shell.
- Botón "Ver documento formal" ahora abre con `forceLoad: true` (nueva pestaña).
- Fuente Arial, texto justificado, line-height 1.5 en todo el documento.
- Header: reducido tamaño del nombre de marca (1.1rem) y subtítulo (0.72rem) con `white-space: nowrap` para que la fecha sea visible.
- "Anexo Nº" → "Nº" en es/en.
- ANEXO I en página aparte (`page-break-after: always` en el divider).
- Eliminados fondos grises: `cd-id-strip` sin `background`, `cd-anexo-divider` sin línea visible, `cd-footer` sin `border-top`, `cd-anexo-section` sin `border-bottom`.
- Footer y sello de aceptación movidos al final del Contrato Marco (antes del ANEXO I).
- Firmas alineadas: fecha centrada arriba de ambos bloques, `padding: 0` en `.cd-signatures`.
- Datos reales de TRATO DIRECTO HUMAN SERVICES, S.L. hardcodeados (domicilio, CIF, Registro Mercantil, representante Luis Alejandro Velasquez, DNI/NIE).
- Estado del contrato oculto en el documento.
- Cache-busting para `admin.css` con `?v=@DateTime.Now.ToString("s")` en `App.razor`.

**Campo `ContactDniNie` end-to-end:**
- Entidad `PTCompany` + migración `20260911025117_AddContactDniNie`.
- DTOs: `CompanyDetailDto`, `UpdateCompanyDto`, `CreateCompanyDto`, `CompanyPipelineDetailDto`, `AdminVacancyContractDto`.
- `CompanyCrmService` mapea `ContactDniNie` en create/update.
- `AdminContractService` proyecta `CompanyContactDniNie`.
- UI: inputs en `Create.razor` y `Edit.razor`, display en `VacancyContractDocument.razor`.

**Página de edición de empresa:**
- Nueva `Edit.razor` en `Components/Pages/Companies/`.
- Botón "Editar" en `Detail.razor` linking a `/companies/{id}/edit`.
- Claves i18n `admin.companies.edit` en es/en.

**Flujo de aprobación de contratos:**
- Confirmado: `SendAsync` (Draft → Sent), `DecideAsync` (Sent → Accepted/Rejected) en `AdminContractService`.
- UI: botones "Enviar", "Aceptar", "Rechazar" en `VacancyContract.razor` según estado.

- Commit `c0ccb04` en `main`.

### Sesión 2026-09-09 — Página pública de Preguntas Frecuentes (Dsiezar)

Nueva página `/faq` ([`Faq.razor`](src/OpenToWork.WEB/Components/Pages/Faq.razor)) en el portal público — acordeón, primer ítem abierto por defecto, chevron que rota, borde con acento en el ítem abierto. Cierra el enlace del footer a `/faq` que devolvía 404.

**10 preguntas y respuestas** alineadas al modelo de negocio (Embudo Ciego): qué es Trato Directo, gratis para candidatos, cómo funciona para una empresa (ve conteos, no perfiles), por qué no ve a todos los postulantes, distintivo "Verificado TD", cómo se evalúa a un candidato, visibilidad de la puntuación para el candidato, datos personales, vigencia del perfil (12 meses), idiomas.

- Claves `common.faq.*` (title, subtitle, q1–q10, a1–a10) en `es/common.json` y `en/common.json`.
- Estilos `.faq-*` en `components.css`, cache-buster `v=24` → `v=25` en `App.razor`.
- Commit `aad9e0f` en `dsiezar-fase-5`, merge fast-forward a `main`.

### Sesión 2026-09-08 — Embudo Ciego: entrega de personal verificado + adopción sobre implementación propia (Iluna + Dsiezar)

**Iluna (`a02f727`..`2e76e4c`)** — la empresa deja de ver candidatos: solo ve el conteo de postulantes por vacante. Trato Directo investiga y evalúa en el pipeline y luego *entrega* a la empresa un candidato concreto.

- Entidad `PTCandidateDelivery` (`PT_CandidateRecruitmentId`, `PT_CandidateId`, `PT_VacancyId`, `PT_CompanyId`, `DeliveredByUserId`, `Status`, `AdminNote`, `CompanyFeedback`) + enum `DeliveryStatus` (Delivered/ViewedByCompany/Interested/Hired/RejectedByCompany). Migración `20260908113350_CandidateDeliveries`.
- `DeliveryService.DeliverCandidateAsync` valida: reclutamiento en etapa `ReadyToDeliver`, candidato `IsVerifiedTD == true`, vacante existente con empresa, sin entrega previa a esa vacante.
- `DeliveriesController` en AdminAPI (el reclutador entrega) y en la API pública (la empresa consulta lo entregado y responde).
- `CompanyDashboard.razor` y `VerifiedApplicants.razor` reescritos para mostrar solo conteos ("X postulantes / Y en verificación").
- Videos `v01.mp4` / `v02.mp4` en bucle en login y home; `docs/seed-hosteleria.sql`; menú de empresa sin "Búsqueda avanzada" (se reactivará cuando la empresa tome un plan destinado a eso, en otra fase).
- Validación del flujo con el agente de RH: `docs/rh/validacion-flujo-negocio-2026-09-08.md`, `docs/rh/guia-embudo-ciego-iluna.md`, `docs/rh/guia-implementacion-iluna.md`.

**Dsiezar (`a3f3ab4`, `97c419f`)** — análisis RH previo (`docs/rh/alineacion-trato-directo-y-arranque.md`): el gate de `CandidateSearchService` dejaba pasar perfiles con `IsProfilePublic && WizardCompleted` sin exigir verificación. Tras el merge del embudo ciego se detectó que `RecruitmentService.GetVacancyOptionsAsync` filtraba `Status == 0` (Borrador) en vez de `1` (Activa) — el modal "Vincular vacante" siempre decía "No hay vacantes activas" y la entrega era inutilizable; corregido. Un commit propio no pusheado que implementaba el mismo flujo de forma paralela se descartó (`git reset`) para adoptar la versión de Iluna.

### Sesión 2026-09-01 — Fase 3 completa: Motor de Evaluación y Scoring Automático, 8 sub-fases (Dsiezar)

Se completó Fase 3 de punta a punta siguiendo el plan obligatorio de Iluna (`a340397`), sub-fase por sub-fase en orden estricto, con las preguntas de cada una respondidas y documentadas **antes** de escribir código. Detalle completo por sub-fase en [`docs/dsiezar/fase-3-sub1.md`](docs/dsiezar/fase-3-sub1.md) a [`fase-3-sub8.md`](docs/dsiezar/fase-3-sub8.md). Rama `dsiezar-fase-3` (8 commits, `e746f21`..`85bf4ea`), pusheada a origin, pendiente de PR/revisión de Iluna.

- **3.1 — Entidades + migración:** `PTCandidateScore`, `PTJobMatchScore`, `PTVerification`, `PTCandidateReference`, `PTSkillTest`, `PTCandidateTestResult`, todas con FK a `PT_CandidateId` (no `SCUserId`).
- **3.2 — `ValidationService`:** verificación automática de LinkedIn/portafolio (formato + alcanzabilidad HTTP) y coherencia cronológica del CV (gaps, saltos laborales, solapamientos); identidad queda como stub documentado (`PTCandidate` no tiene campo de documento todavía).
- **3.3 — `ScoringService`:** los 4 índices del Candidate Score (Estabilidad/Confiabilidad/Evidencia/Compatibilidad) + `OverallScore` ponderado, recalculado automáticamente en cada edición de perfil/experiencia/educación/CV.
- **3.4 — `CompatibilityService`:** Job Match Score por par candidato-vacante (skills/experiencia/ubicación — educación e idioma quedan fuera por falta de campos estructurados), shortlist rankeado, admin-driven (no automático).
- **3.5 — Referencias laborales:** alta desde el perfil, link público con token (sin SMTP, mismo patrón que el reset de contraseña), feedback y verificación automática al responder.
- **3.6 — Retos técnicos:** banco de preguntas multiple-choice, intento con timer server-side, anti-cheat básico, CRUD admin.
- **3.7 — Estado "Verificado TD":** calculado en vivo (nunca persistido) a partir de score + verificaciones + referencias, sin necesitar tabla ni job nuevo.
- **3.8 — UI en los 3 portales:** dashboard del candidato (4 índices + badge), gestión de scores y verificaciones manuales en el admin, banco de retos admin con preview, cola de shortlist, y shortlist + scorecard configurable en el portal de empresa. De paso se agregó `PTVacancy.WeightsConfig` (no existía un lugar propio para que la empresa configure pesos) y se corrigió un bug real de redirección durante el prerender estático de Blazor Server.

Todo verificado end-to-end contra MySQL real (no solo compilado) en cada sub-fase — la 3.8 además en navegador real contra los 3 portales corriendo en simultáneo. Con esto, Fase 4 queda 100% completa (los 2 items que esperaban a Fase 3 ya están resueltos) y Fase 5 desbloquea shortlist/scorecard/ranking por compatibilidad.

### Sesión 2026-08-29 — Cierre de Fase 4: gestión de roles + 4 items de deuda técnica (Dsiezar)

Se completó todo lo pendiente de Fase 4 que no dependía de Fase 3. Detalle completo en [`docs/dsiezar/fase-4.md`](docs/dsiezar/fase-4.md).

- **Gestión de roles de usuario:** nuevo endpoint `PUT /api/admin/users/{id}/role` (`AdminUserService.ChangeRoleAsync`) con guardia de auto-bloqueo y validación de rol; selector de rol por tarjeta en `/users` con confirmación antes de aplicar el cambio.
- **Unificación `AdminAuthService`/`AuthService`:** la lógica de criptografía de tokens (firma JWT, refresh token, hashing) que estaba duplicada se extrajo a `ITokenCryptoService` en `OpenToWork.Core`. Cada servicio conserva su propia lógica de claims y su propia configuración `Jwt:*`.
- **Paginación de `AdminVacancyService`:** `GetVacanciesAsync` ya no carga `PT_Vacancies`/`PT_TempVacancies` completas en memoria — ambas se proyectan a `IQueryable<AdminVacancyDto>` con el mismo conjunto de propiedades y se unen con `.Concat()`, que EF Core/Pomelo traduce a un `UNION ALL` con `ORDER BY`/`LIMIT`/`OFFSET` del lado del servidor.
- **`LocalStorageService`/`LanguageService` movidos a `SharedUI`:** `LanguageService` se unificó recibiendo el arreglo de secciones de traducción por constructor (preserva el comportamiento de ambos portales sin cambios).
- **Guard de autenticación centralizado en `AdminLayout`:** eliminado de las 9 páginas que lo duplicaban. Efecto colateral: las 4 páginas del Pipeline de Reclutamiento (Iluna) que nunca tuvieron este guard quedan protegidas automáticamente.

Todo verificado end-to-end contra MySQL real (no solo compilado): cambio de rol con reversión, guardias de auto-bloqueo (409) y rol inválido (400), login/refresh-token en ambos portales tras la unificación de crypto, paginación y filtro por status de vacantes tras la reescritura con `Concat`, traducciones ES/EN tras la migración a `SharedUI`, y redirección a `/login` sin sesión en páginas antes desprotegidas.

Quedan bloqueados por Fase 3 (sin cambios): verificaciones manuales (`PTVerification`) y revisión de validaciones automáticas.

### Sesión 2026-08-21 — Pipeline de Reclutamiento completo (Iluna)

> **Nota de Dsiezar (2026-08-24):** Esta entrada documenta 60+ commits que ya estaban en `main` pero no tenían registro en la Bitácora — se agrega ahora al leer el README y sincronizar migraciones. El detalle línea por línea está en el historial de git; aquí el resumen funcional.

#### Consola de candidatos (`Candidates/Index.razor` — nuevo)
- 4 tabs: Sin iniciar, En proceso, Finalizado, Descartados (filtro `recruitmentStatus` en la API)
- Estadísticas, búsqueda por nombre/email/título, acciones masivas (activar/desactivar seleccionados), exportación CSV
- Botón "Asignar candidato" — modal con selección de usuario admin, redirige al pipeline

#### Pipeline de reclutamiento (`Candidates/Pipeline.razor`, `PipelineDetail.razor` — nuevo)
- Vista kanban por etapas + stepper en el perfil del candidato
- Historial de etapas (`PTRecruitmentStageLog`), descarte con motivo (`PTRecruitmentDismissal`)
- Página `Assigned.razor`: candidatos asignados al reclutador actual, con etapa e info de investigación

#### Checklist de investigación y referencias (`PTInvestigationChecklist`, `PTReferenceCheck`)
- 5 pasos por defecto + validaciones personalizadas, tracking de duración (`StartedAt`/`CompletedAt`)
- Sub-panel de referencias con empresa/contacto/estado — **se auto-generan desde las experiencias laborales** del candidato
- Captura/edición del teléfono del candidato desde el checklist si falta

#### Evaluaciones técnicas y entrevistas culturales (`PTTechnicalEvaluation`)
- Evaluaciones técnicas: CRUD completo en modal, puntuación, promedio por etapa
- Entrevistas culturales: notas, puntuación, recomendación, listadas como cards con promedio (sin endpoint separado)

#### Score general del candidato
- Círculo de puntaje en el perfil (incluye porcentaje de investigación completada, no solo evaluaciones)
- Modal con resumen de puntuaciones por etapa, notas clickeables en el stepper con detalle apto/no apto

#### Backend — nuevas entidades y servicios
- Entidades: `PTCandidateRecruitment`, `PTInvestigationChecklist`, `PTReferenceCheck`, `PTTechnicalEvaluation`, `PTRecruitmentStageLog`, `PTRecruitmentDismissal`
- `RecruitmentController.cs`, `RecruitmentService.cs` / `IRecruitmentService.cs`
- `AdminCandidateService.cs` / `IAdminCandidateService.cs` — endpoint dedicado de consola con filtros/estadísticas
- `RecruitmentDtos.cs`, `RecruitmentEnums.cs`
- 6 migraciones EF Core: `RecruitmentPipeline`, `UpdateInvestigationChecklist`, `InvestigationTrackingAndReferences`, `AutoReferencesFromExperiences`, `TechnicalEvaluations`, `CulturalInterviewFields`
- Fix: query de candidatos dividida (subquery `TopSkills` no traducía a SQL en MySQL/Pomelo vía `OUTER APPLY`)
- Fix: `GetCulturalInterview` retorna `NotFound` en vez de `Ok(null)` (causaba error de parseo JSON en el cliente)

#### Navegación
- Sidebar de `AdminWEB` simplificado: Panel + grupo "Reclutamiento" (Candidatos, Asignados, Pipeline)

#### Relación con Fase 3 y la definición estratégica
Este pipeline es una implementación **manual/asistida por reclutador** del objetivo de Fase 3 (evaluar y verificar candidatos antes de exponerlos a la empresa) — no el motor 100% automático (`ValidationService`/`ScoringService`) que describía el checklist original. Encaja con el paso "TD revisa candidatos" de la definición estratégica consolidada (sesión 2026-08-15): confirma que Trato Directo cura candidatos activamente, no solo da acceso a una base. Ver detalle en la sección "Fase 3" más arriba.

---

### Sesión 2026-08-15 — Respuesta de Darwin a RH + definición estratégica consolidada

#### ✅ Darwin respondió las 17 preguntas de RH

Respuesta completa en `docs/dsiezar/respuesta-rh.md`. Además, se recibió y validó un segundo análisis (consolidación de dos planteamientos de negocio) que **refina la dirección sin contradecir lo ya construido** (Fase 1, Fase 2 y el Portal Admin de Fase 4 quedan intactos). Los cambios de rumbo afectan únicamente al diseño de **Fase 3 (Motor de Evaluación)**, que todavía no se ha empezado a construir — llega en el momento correcto.

#### Decisión estratégica central: Trato Directo es Tech-Enabled Recruitment, no un ATS self-service

> Trato Directo **selecciona y cura** candidatos para la empresa (no solo le da acceso a una base para que ella haga todo el trabajo). El diferenciador es: **candidato evaluado → candidato verificado → matching con la vacante → shortlist de calidad.**

Flujo completo que debe soportar el sistema (MVP = que este ciclo funcione de punta a punta, aunque sea con un solo candidato y una sola empresa — **el MVP valida la transacción, no el volumen**):

```
Candidato se registra → Completa perfil → TD evalúa → TD verifica →
Sistema calcula Candidate Score → Candidato entra a base elegible →
Empresa registra vacante → Sistema calcula Job Match → TD revisa candidatos →
Se genera shortlist → Empresa revisa shortlist → Entrevista → Contratación/descarte
→ Todo evento relevante queda auditado
```

#### Cambio de diseño técnico: dos scores separados, no uno

- **Candidate Score** — intrínseco del candidato (experiencia, formación, competencias, estabilidad, referencias, verificación). La empresa **no puede modificarlo**.
- **Job Match Score** — específico por candidato-vacante (compatibilidad). La empresa **sí puede ajustar los pesos** por vacante (scorecard configurable).

Implica **dos entidades separadas** en el modelo de datos de Fase 3 (`PTCandidateScore` y algo tipo `PTJobMatchScore` calculado por par candidato-vacante), no una sola tabla de "scoring" mezclada.

#### "Verificado Trato Directo" es un estado, no un booleano

Estado progresivo: `Perfil registrado → Perfil completo → Evaluado → Verificación en proceso → Verificado TD`, con dimensiones internas propias (identidad, experiencia, formación, referencias, documentación, evaluación realizada, fecha de última verificación). El distintivo ★ solo aparece cuando se cumplen los criterios mínimos — es un activo de confianza, no solo un ícono.

#### Corrección sobre retención (reemplaza la regla de "12 meses" de `respuesta-rh.md`)

En vez de una expiración automática por tiempo fijo, el candidato **permanece en la plataforma indefinidamente con un estado que identifica que ya fue validado**. La visibilidad para empresas se gobierna por ese estado, no por un temporizador — evita fijar en código una regla comercial que todavía no está cerrada. (Retención/soft delete siguen siendo obligatorios desde el diseño, solo se parametriza el criterio de expiración en vez de hardcodearlo).

#### Apelación de score: se deja abierta, no cerrada

`respuesta-rh.md` decía "no hay apelación". Se corrige a: **no se cierra la decisión todavía** — el modelo de evaluación debe poder re-evaluarse/versionarse (ya era necesario por el recálculo periódico de la pregunta 11), sin comprometerse aún a un flujo formal de disputa.

#### Nueva feature de Admin identificada (no estaba en el diseño original de Fase 4)

Pantalla de **revisión de matches / cola de shortlist** — antes de que un match candidato-vacante llegue a la empresa, alguien de Trato Directo lo revisa y aprueba. Se agrega al alcance de cuando se conecte Fase 3 con el Portal Admin.

#### Fuera del MVP (confirmado, sin cambios respecto a `respuesta-rh.md`)

Integración HRIS, API empresarial, ML avanzado, multiidioma más allá de ES/EN, automatizaciones Enterprise, reporting sofisticado, personalizaciones extensas por cliente.

#### 7 decisiones que se dejan abiertas a propósito (no cerrar todavía)

Metodología exacta de "Verificado TD" · pesos del Candidate Score · variables configurables del Job Match Score · modelo de ingresos inicial · nivel de intervención humana de TD por plan · valor concreto gratuito para el candidato · política de revisión/actualización de evaluaciones.

---

### Sesión 2026-08-15 — Dashboard clickeable, vista de resultados, perfil de usuario y análisis RH

#### ⚠️ Ojo Darwin — Necesito que respondas las preguntas del experto en RH

> **Darwin:** El agente RH (ver `/rh`) publicó un análisis completo del portafolio de candidatos en `docs/rh/analisis-portafolio-candidatos.md`. Antes de seguir avanzando, necesito que leas las **17 preguntas estratégicas** que hizo RH y respondas cada una. Las preguntas están agrupadas en:
>
> - **Modelo de negocio** (3 preguntas): planes gratuito vs. premium, modelo de cobro
> - **Datos y privacidad** (3 preguntas): GDPR/Ley 25.326, ownership de datos, notas internas
> - **Evaluación y scoring** (4 preguntas): transparencia del score, apelaciones, recálculo
> - **Competencia y escalabilidad** (5 preguntas): diferenciadores vs. LinkedIn/Computrabajo, video pitch vs. scoring, soporte multiidioma
>
> **Pregunta clave:** ¿Crees que estas preguntas se alinean a lo que estamos haciendo? ¿O hay alguna que no aplica o que cambiarías?
>
> Tu respuesta va a definir el alcance de la Fase 3 (Motor de Evaluación) y la Fase 4 (ATS + Video). Responde en `docs/dsiezar/respuesta-rh.md` o directo en este README.

---

#### Dashboard clickeable (`Dashboard.razor`)
- Gráficos del dashboard ahora son clickeables y redirigen a vista de resultados
- Cada chart navega con query params: `role`, `filter`, `section`

#### Vista de resultados (`DashboardResults.razor` — nuevo)
- Página `/dashboard/results` que muestra datos filtrados según el gráfico clickeado
- Cards con avatar, nombre, email, estado y badges (evaluado, LinkedIn, portfolio, CV)
- Filtros: evaluated, pending, scores, linkedin, portfolio, cv, companies, vacancies
- Cards clickeables que navegan al perfil del usuario

#### Vista de perfil de usuario (`UserProfile.razor` — nuevo)
- Página `/user/{id}` con perfil completo en modo lectura
- **Candidatos:** header con avatar + donut de completitud, info de contacto, resumen, skills, experiencia (timeline), educación (timeline), certificaciones (cards), info de cuenta
- **Empresas:** header con logo, info de contacto, descripción, industria, tamaño, vacantes activas
- Donut chart SVG de progreso de completitud del perfil (14 campos para candidatos, 10 para empresas)
- Botón de volver al dashboard

#### Vista de usuarios rediseñada (`Users.razor`)
- Reemplazada tabla por grid de cards
- Búsqueda en tiempo real por nombre o email
- Chips por rol: Todos, Candidatos, Empresas, Admins (con contador)
- Cards clickeables que navegan al perfil del usuario
- Badges en cada card: rol, evaluado/pendiente, LinkedIn, portfolio, CV
- Acciones de activar/desactivar/eliminar con `@onclick:stopPropagation`

#### Backend — API
- `AdminUserProfileDto`: DTO con todos los datos del candidato (skills, experiencia, educación, certificaciones) y empresa
- `AdminUserService.GetUserProfileAsync`: carga perfil con includes anidados
- Endpoint `GET /api/admin/users/{id}/profile`
- `AdminAuthApiService.GetUserProfileAsync`: método cliente en AdminWEB

#### Análisis RH (`docs/rh/analisis-portafolio-candidatos.md` — nuevo)
- 15 módulos faltantes priorizados (críticos, alta, media)
- 17 preguntas estratégicas para el equipo
- Recomendación: Fase 3 es el bloque crítico

#### Archivos nuevos
- `src/OpenToWork.AdminWEB/Components/Pages/DashboardResults.razor`
- `src/OpenToWork.AdminWEB/Components/Pages/UserProfile.razor`
- `docs/rh/analisis-portafolio-candidatos.md`

#### Archivos modificados
- `src/OpenToWork.AdminWEB/Components/Pages/Dashboard.razor`
- `src/OpenToWork.AdminWEB/Components/Pages/Users.razor`
- `src/OpenToWork.AdminWEB/Services/AdminAuthApiService.cs`
- `src/OpenToWork.Core/Services/AdminUserService.cs`
- `src/OpenToWork.Core/Interfaces/IAdminUserService.cs`
- `src/OpenToWork.AdminAPI/Controllers/UsersController.cs`
- `src/OpenToWork.Shared/DTOs/AdminDtos.cs`
- `src/OpenToWork.AdminWEB/wwwroot/css/admin.css`
- `src/OpenToWork.AdminWEB/wwwroot/config/language/es/admin.json`
- `src/OpenToWork.AdminWEB/wwwroot/config/language/en/admin.json`
- `README.md`

---

### Sesión 2026-08-14 — Rediseño Samsung One UI + Bento Grid + PWA

**Autorización de diseño:** Iluna (diseño visual) · Darwin (supervisión de procesos)

#### Rediseño de Perfil (Profile Sidebar)
- Eliminado el header azul sólido, reemplazado por tarjeta blanca `#FFFFFF` con banner suave `#F0F7FF`
- Avatar rediseñado como squircle (`border-radius: 20px`) con fondo `#0066FF` y borde blanco
- Nombre en `#0B132B` con `font-weight: 800`
- Rol como pill badge con fondo `#F1F5F9` y texto `#3A506B`
- Email con icono de sobre en `#778DA9`
- Skills como chips grises (`#F1F5F9`), modalidad como pill azul tenue (`#E8F1FF` / `#0066FF`)

#### Rediseño de Navegación Móvil (MainLayout)
- **Top App Bar**: Logo oculto en móvil, título dinámico de pantalla alineado a la izquierda, campana + avatar a la derecha
- **Título dinámico**: `GetScreenTitle()` con suscripción a `NavigationManager.LocationChanged` para actualizar al navegar
- **Bottom Navigation Bar**: Barra fija de 64px con 4 pestañas (Panel, Vacantes, Postulaciones, Mensajes)
- **Settings relocados**: Botón de idioma/tema movido del top bar al dropdown del avatar (solo móvil)
- **Footer oculto** en móvil, padding inferior de 64px para bottom nav
- `viewport-fit=cover` para soporte de notch con `env(safe-area-inset-bottom)`

#### Rediseño de Messages (Bento Inbox)
- Eliminado título duplicado "Mensajes" del sidebar (ya está en top bar)
- Filtros rediseñados como pills sutiles: inactivos transparentes, activos con `#E8F1FF` / `#0066FF`
- Avatares squircle (`border-radius: 14px`) en `#0066FF`
- Conversación seleccionada: borde izquierdo azul 3px + fondo `#F0F7FF`
- Estado vacío: icono en contenedor squircle 80px con fondo `#F0F7FF` y texto descriptivo
- **Móvil**: Lista de conversaciones a pantalla completa → al seleccionar, chat full-screen con botón flecha ← para regresar
- Bubbles: propias `#0066FF`, ajenas `#F1F5F9` con texto `#0B132B`
- Botón enviar: squircle `14px` con hover `#0052CC`

#### Componente VacancyCard Reutilizable
- Creado `Components/Shared/VacancyCard.razor` para evitar duplicación de código
- Usado en `MyVacancies.razor`, `Dashboard.razor`, `Home.razor`, `MyApplications.razor`
- Props: `Vacancy`, `ShowActions`, `OnEdit`

#### Página VacancyManage
- Nueva página para gestión de vacantes (`/my-vacancies/{Id}`)
- Hero banner con pills de estado, columnas asimétricas, lista de candidatos con filtros

#### PWA (Progressive Web App)
- **Icono SVG**: Maletín blanco con siglas "OTW" en azul royal sobre fondo `#0066FF`
- **manifest.json**: `name: OpenToWork`, `short_name: OTW`, `display: standalone`, `theme_color: #0066FF`
- **Service Worker** (`sw.js`): Cache de assets estáticos, cache-first para recursos, network-first para navegación
- **Meta tags**: `apple-mobile-web-app-capable`, `theme-color`, `apple-touch-icon`
- **Program.cs**: MIME types configurados para `.webmanifest`

#### Bug Fixes
- **`GetPermanentVacancyAsync`**: Faltaba `SetAuthHeaderAsync()` → la API devolvía 401 y la página se quedaba cargando indefinidamente
- **VacancyDetail**: Agregado manejo de error con `LoadFailed` y estado visual centrado (icono grande + mensaje + botón volver)
- **`MainLayout`**: Suscripción a `LocationChanged` para que el título dinámico se actualice al navegar entre páginas

#### Traducciones (ES/EN)
- `common.nav.myVacancies` — "Mis Vacantes" / "My Vacancies"
- `common.messages.selectConversation` — "Selecciona una conversación de la lista para comenzar a chatear" / "Select a conversation from the list to start chatting"
- `common.buttons.back` — "Volver" / "Back"
- `vacancies.notFound` — "No se pudo cargar la vacante..." / "Could not load the vacancy..."
- `vacancies.edit` / `vacancies.backToMyVacancies`

#### Documentación
- Creado `DESIGN-SYSTEM.md` con:
  - Regla de autorización de diseño (Iluna autoriza, Darwin supervisa)
  - Paleta de colores completa con tokens hex
  - Especificaciones de tipografía, componentes UI, navegación móvil, PWA
  - Reglas para nuevos componentes (reutilizar, no duplicar, usar tokens)

#### Archivos modificados/creados
- **Modificados**: `MainLayout.razor`, `App.razor`, `Program.cs`, `ApiAuthService.cs`, `Messages.razor`, `VacancyDetail.razor`, `Profile.razor`, `Dashboard.razor`, `Home.razor`, `MyApplications.razor`, `MyVacancies.razor`, `Vacancies.razor`, `_Imports.razor`, `components.css`, `portal-nav.css`, `wizard-profile.css`, traducciones ES/EN
- **Creados**: `DESIGN-SYSTEM.md`, `VacancyManage.razor`, `VacancyCard.razor`, `icon.svg`, `manifest.json`, `sw.js`

---

### Sesión 14-Ago-2026 — Rediseño de Dashboard, About, VacancyDetail, Navbar y Messages

> **Nota:** Este Ivan se esmero. Dale el premio.

#### Dashboard (`Dashboard.razor`)
- Reemplazado el saludo suelto por **Bento Welcome Banner Card** con avatar de iniciales, rol del usuario, saludo y boton de acceso directo al perfil.
- Agregado **grafico circular de completion de perfil** que ocupa 2 columnas del grid.
- Agregado **card de indicadores** con 3 metricas: Solicitudes, Postulaciones, Publicaciones.
- Agregadas **acciones rapidas** como cards con iconos y texto centrado: Subir CV, Grabar Video, Buscar Empleo, Completar Perfil.
- Agregada seccion de **vacantes recomendadas**.
- Corregido truncamiento de texto en "Completar perfil" (layout flex column, sin nowrap).

#### About Page (`About.razor` — nuevo)
- Creada pagina `/about` con hero header centrado.
- **Fila 1**: Card izquierda con SVG transparente (personas conectadas) + Card derecha con texto "Nosotros".
- **Fila 2**: Card izquierda con texto "Que hacemos" + Card derecha con SVG transparente (maletin, lupa, documento, video).
- **Fila 3**: Dos cards de texto lado a lado — "Mision" (icono target) y "Valores" (icono capas).
- Cards de imagen **sin fondo, sin borde, sin sombra**. SVGs con strokes transparentes/accent.
- Layout responsive: a 768px las filas se apilan en una columna.
- Traducciones agregadas en `common.json` (es + en) bajo seccion `about`.

#### Vacancy Detail (`VacancyDetail.razor`)
- **Eliminado el card dentro de card** (ot-card anidado).
- Rediseño a layout plano con:
  - Header con titulo grande + empresa + badge de verificacion (pill verde).
  - Badges con iconos SVG: ubicacion, tipo de contrato, modalidad, salario (accent), categoria, nivel de experiencia, nivel de ingles.
  - Secciones de descripcion y requisitos con separadores `border-top` sutiles.
  - Formulario de postulacion separado con `border-top` accent (2px), sin card envolvente.
- Corregido el uso de `ot-input` → `ot-input-field` (clase CSS correcta con estilos definidos).

#### Navbar (`MainLayout.razor`)
- Reorganizado en **3 bloques equilibrados con Flexbox**:
  - **Izquierda**: Logo OTW + OpenToWork.
  - **Centro**: 4 pestañas de navegacion con iconos SVG e indicador de estado activo:
    - Panel (dashboard), Mis Postulaciones, Buscar Empleos, Mensajes.
  - **Derecha**: Settings pill (boton compacto con engranaje + idioma, dropdown con tema e idioma agrupados) + User cluster (campana + avatar, separados por `border-left`).
- Agregada deteccion de pagina activa (`CurrentPage`) basada en `NavigationManager.Uri`.
- Eliminados los enlaces centrales anteriores (Inicio, Vacantes, Sobre Nosotros).

#### Messages Page (`Messages.razor` — nuevo)
- Creada pagina `/messages` con layout de 2 columnas (340px sidebar + 1fr chat).
- **Sidebar**: Bandeja de conversaciones con:
  - Filtros tipo pestaña: Todos | No leidos (con badge) | Leidos.
  - Buscador de conversaciones por nombre.
  - Lista con avatar, nombre, vacante asociada, preview, tiempo relativo, badge de no leidos.
- **Panel de chat**: Header con avatar, nombre, vacante, indicador "En linea". Burbujas alternadas (mias accent derecha, suyas gris izquierda). Input redondo + boton circular de enviar. Enter para enviar.
- Al seleccionar conversacion no leida, se marca como leida automaticamente.
- Responsive: a 768px se apila en una columna.

#### Backend — Messages
- **`MessageDto.cs`** (Shared): DTOs `ConversationDto`, `MessageDto`, `SendMessageDto`.
- **`MessagesController.cs`** (API): Endpoints `GET conversations`, `GET messages/{id}`, `POST send`, `PUT read`. Datos mock por ahora.
- **`ApiAuthService.cs`** (WEB): Metodos `GetConversationsAsync`, `GetMessagesAsync`, `SendMessageAsync`, `MarkConversationReadAsync`.

#### Traducciones (`common.json` es + en)
- Seccion `about`: titulos, descripciones, mision, valores.
- Seccion `nav`: `messages`, `searchJobs`, `panel`.
- Seccion `messages`: title, all, unread, read, noConversations, noMessages, typeMessage, send, vacancy, online, offline, search.

#### CSS (`components.css`)
- Estilos `.dash-banner-card` y relacionados del dashboard.
- Estilos `.about-*` para About page.
- Estilos `.vacancy-detail-*` para VacancyDetail.
- Estilos `.nav-settings-pill`, `.nav-settings-btn`, `.nav-settings-dropdown`, `.nav-user-cluster`, `.nav-link` con iconos.
- Estilos `.messages-*` y `.chat-*` para Messages page.
- Cache-buster actualizado a `v=11`.

#### Archivos nuevos
- `src/OpenToWork.WEB/Components/Pages/About.razor`
- `src/OpenToWork.WEB/Components/Pages/Messages.razor`
- `src/OpenToWork.Shared/DTOs/MessageDto.cs`
- `src/OpenToWork.API/Controllers/MessagesController.cs`

#### Archivos modificados
- `src/OpenToWork.WEB/Components/Pages/Dashboard.razor`
- `src/OpenToWork.WEB/Components/Pages/VacancyDetail.razor`
- `src/OpenToWork.WEB/Components/Layout/MainLayout.razor`
- `src/OpenToWork.WEB/Components/App.razor`
- `src/OpenToWork.WEB/Services/ApiAuthService.cs`
- `src/OpenToWork.WEB/wwwroot/css/components.css`
- `src/OpenToWork.WEB/wwwroot/config/language/es/common.json`
- `src/OpenToWork.WEB/wwwroot/config/language/en/common.json`

---

### Sesión 14-Ago-2026 — Suite de Pruebas de Integración (OpenToWork.Tests)

> **QA (Sr. Smith):** Pruebas automatizadas de integración contra la API real (localhost:5000) con xUnit.

#### Proyecto creado
- `src/OpenToWork.Tests/OpenToWork.Tests.csproj` — xUnit, .NET 10, referencia a `OpenToWork.Shared`.

#### Arquitectura de pruebas
- **`BaseTest.cs`** — Clase base abstracta que crea un `HttpClient` propio por test, hace login automático con `juan.perez@gmail.com` y setea el Bearer token. Cada test es independiente.
- Cada clase de test hereda de `BaseTest` y tiene su propio `HttpClient` aislado.

#### Pruebas de Auth (`AuthTests.cs`) — 10 pruebas
| Test | Descripción | Resultado |
|------|-------------|-----------|
| `Login_ConCredencialesValidas_RetornaTokenYUsuario` | Login con juan.perez@gmail.com valida token, refresh y email | ✅ |
| `Login_ConPasswordIncorrecta_RetornaUnauthorized` | Password incorrecta retorna 401 | ✅ |
| `Login_ConEmailInexistente_RetornaUnauthorized` | Email inexistente retorna 401 | ✅ |
| `Login_ConEmailVacio_RetornaUnauthorized` | Email vacío retorna 401 (ver bug #4) | ✅ |
| `Login_ConPasswordVacia_RetornaUnauthorized` | Password vacía retorna 401 (ver bug #4) | ✅ |
| `Refresh_ConTokenValido_RetornaNuevoToken` | Refresh token genera nuevo JWT | ✅ |
| `CheckDevice_SinAutenticar_RetornaUnauthorized` | Endpoint protegido sin token retorna 401 | ✅ |
| `Login_ConMariaGonzalez_RetornaTokenValido` | Login con segundo usuario de prueba | ✅ |
| `Login_ConCarlosRodriguez_RetornaTokenValido` | Login con tercer usuario de prueba | ✅ |

#### Pruebas de Profile (`ProfileTests.cs`) — 8 pruebas
| Test | Descripción | Resultado |
|------|-------------|-----------|
| `GetProfile_ConTokenValido_RetornaPerfil` | GET /api/profile retorna datos del candidato | ✅ |
| `GetProfile_SinToken_RetornaUnauthorized` | Sin token retorna 401 | ✅ |
| `UpdateProfile_ConDatosValidos_RetornaPerfilActualizado` | PUT /api/profile actualiza título | ✅ |
| `AddExperience_ConDatosValidos_RetornaExperienciaCreada` | POST experience crea y retorna | ✅ |
| `AddExperience_ConCompanyNameVacio_LoCreaSinValidar` | CompanyName vacío aceptado (ver bug #2) | ✅ |
| `AddEducation_ConDatosValidos_RetornaEducacionCreada` | POST education crea y retorna | ✅ |
| `AddCertification_ConDatosValidos_RetornaCertificacionCreada` | POST certification crea y retorna | ✅ |
| `DeleteExperience_ConIdInexistente_RetornaNotFound` | Delete con GUID inexistente retorna 404 | ✅ |
| `DeleteEducation_ConIdInexistente_RetornaNotFound` | Delete con GUID inexistente retorna 404 | ✅ |

#### Pruebas de Vacancies (`VacancyTests.cs`) — 9 pruebas
| Test | Descripción | Resultado |
|------|-------------|-----------|
| `Search_Vacantes_RetornaListaYTotal` | GET /search retorna items y total | ✅ |
| `Search_ConFiltroTexto_RetornaResultadosFiltrados` | Filtro por query=desarrollador | ✅ |
| `Search_ConPaginaGrande_RetornaResultados` | PageSize=100 funciona | ✅ |
| `GetById_ConIdInexistente_RetornaNotFound` | GUID inexistente retorna 404 | ✅ |
| `GetById_ConIdValido_RetornaVacante` | Búsqueda + GET por ID real | ✅ |
| `GetMyCompanyVacancies_SinToken_RetornaUnauthorized` | Sin token retorna 401 | ✅ |
| `Create_SinToken_RetornaUnauthorized` | POST sin token retorna 401 | ✅ |
| `Create_ConTokenValido_RetornaCreatedOBadRequest` | POST con token (Created si es empresa, BadRequest si candidato) | ✅ |
| `Create_ConTituloVacio_RetornaBadRequest` | Título vacío retorna 400 | ✅ |

#### Pruebas de Applications (`ApplicationTests.cs`) — 6 pruebas
| Test | Descripción | Resultado |
|------|-------------|-----------|
| `GetMyApplications_ConTokenValido_RetornaLista` | GET /my retorna lista de postulaciones | ✅ |
| `GetMyApplications_SinToken_RetornaUnauthorized` | Sin token retorna 401 | ✅ |
| `Apply_ConVacancyIdInexistente_RetornaError` | Vacancy inexistente retorna 500 (ver bug #1) | ✅ |
| `Apply_SinToken_RetornaUnauthorized` | POST sin token retorna 401 | ✅ |
| `Apply_ConDatosValidos_RetornaCreatedOConflict` | Postulación real (Created o Conflict si ya aplicó) | ✅ |
| `Apply_DosVecesALaMismaVacante_RetornaConflict` | Doble postulación retorna 409 | ✅ |
| `UpdateStatus_ConIdInexistente_RetornaNotFound` | Update status con GUID inexistente retorna 404 | ✅ |

#### Pruebas de Messages (`MessagesTests.cs`) — 11 pruebas
| Test | Descripción | Resultado |
|------|-------------|-----------|
| `GetConversations_ConTokenValido_RetornaLista` | GET conversations retorna lista no vacía | ✅ |
| `GetConversations_SinToken_RetornaUnauthorized` | Sin token retorna 401 | ✅ |
| `GetConversations_RetornaDatosConEstructuraCorrecta` | Valida ParticipantName, Avatar, LastMessage | ✅ |
| `GetMessages_ConConversationIdValido_RetornaMensajes` | GET messages por conversación retorna mensajes | ✅ |
| `GetMessages_ConIdInexistente_RetornaListaVacia` | ID inexistente retorna lista vacía | ✅ |
| `SendMessage_ConDatosValidos_RetornaMensajeCreado` | POST send crea mensaje con IsMine=true | ✅ |
| `SendMessage_SinToken_RetornaUnauthorized` | Sin token retorna 401 | ✅ |
| `SendMessage_ConContenidoVacio_LoAceptaSinValidar` | Content vacío aceptado (ver bug #3) | ✅ |
| `MarkAsRead_ConConversationIdValido_RetornaOk` | PUT read marca conversación como leída | ✅ |
| `MarkAsRead_SinToken_RetornaUnauthorized` | Sin token retorna 401 | ✅ |

#### Bugs encontrados por QA (4 items)

1. **`POST /api/applications` con VacancyId inexistente** — Retorna `500 InternalServerError` en lugar de `404 NotFound`. El `ApplicationService` no valida que la vacante exista antes de crear la postulación.
   - **Fix:** Agregar validación `if (vacancy == null) return NotFound()` en `ApplicationsController.Apply` o en `ApplicationService.ApplyAsync`.

2. **`POST /api/profile/experience` con CompanyName vacío** — La API no valida campos requeridos. Acepta experiencia sin empresa.
   - **Fix:** Agregar `[Required]` en `CreateExperienceDto.CompanyName` y `JobTitle`, o validación manual en `ProfileService`.

3. **`POST /api/messages/send` con Content vacío** — El controller mock no valida contenido vacío.
   - **Fix:** Agregar validación `if (string.IsNullOrWhiteSpace(dto.Content)) return BadRequest()` en `MessagesController.Send`.

4. **`POST /api/auth/login` con email/password vacío** — Retorna `401 Unauthorized` en lugar de `400 BadRequest`. No hay validación de modelo.
   - **Fix:** Agregar `[Required]` en `LoginDto.Email` y `LoginDto.Password`, o validación manual en `AuthController.Login`.

#### Cómo ejecutar las pruebas

```bash
# 1. Asegurar que la API esté corriendo en localhost:5000
dotnet run --project src/OpenToWork.API

# 2. Ejecutar todas las pruebas
dotnet test src/OpenToWork.Tests/OpenToWork.Tests.csproj --verbosity normal

# 3. Ejecutar solo una clase de tests
dotnet test src/OpenToWork.Tests/OpenToWork.Tests.csproj --filter "FullyQualifiedName~AuthTests"
```

#### Resultado final
```
Pruebas totales: 44
     Correcto: 44
 Tiempo total: ~5s
```

---

### Sesión 31-Ago-2026 — Portal de Empresa, Análisis de CV con IA y Evaluación de Perfil IA

#### Portal de Empresa — Dashboard corporativo (`CompanyDashboard.razor` — nuevo)
- Página `/company-dashboard` con panel de comando estilo IA: input con placeholder "Preguntale a la IA o escribe un comando..." y sugerencias clickeables (Crear vacante, Ver vacantes, Postulantes, Mensajes).
- Badges de estadísticas: Vacantes (azul), Postulantes (verde), Borradores (ámbar) — clickeables, navegan a las páginas correspondientes.
- Hero slider de publicidad (70%) + lista de postulantes recientes (30%) en grid responsive.
- Slider con slides de ejemplo: navegación con flechas prev/next, dots indicadores, auto-rotación.
- Lista "Postulantes recientes": avatar con iniciales, nombre, vacante, anillo circular de % perfil completado. Click navega al perfil completo del candidato.
- Eliminada la sección "Mis solicitudes recientes" del dashboard.
- Iniciales y nombre del usuario extraídos del JWT (`given_name`).

#### Análisis de CV con IA — Evaluación de perfil
- `ApplicationDto` extendido con `ProfileCompletionPercentage`.
- `ApplicationService.CalculateProfileCompletion`: calcula el porcentaje de completitud del perfil del candidato basado en 15 campos (nombre, apellido, teléfono, identificación, fecha nacimiento, país, ciudad, título, resumen, años de experiencia, LinkedIn, portfolio, disponibilidad, autorización de trabajo, CV).
- `MapToDtoAsync` actualizado para incluir el porcentaje en cada aplicación mapeada.

#### Evaluación de Perfil IA — Página de perfil completo del candidato (`ApplicantProfile.razor` — nuevo)
- Página `/applicant-profile/{CandidateId}` con diseño estilo CV en modo lectura.
- **Card header 100%**: avatar con iniciales, nombre completo, título profesional, ubicación, años de experiencia, botón "Ver CV", resumen profesional, enlaces de contacto (teléfono, LinkedIn, portfolio).
- **Sección 70/30**:
  - **Columna 70%**: Experiencia laboral (timeline con dots azules), Educación (timeline con dots verdes), Certificaciones (cards con nombre, emisor, fecha).
  - **Columna 30%**: Habilidades con barra de progreso (`ProficiencyLevel`), Información personal (identificación, nacimiento, país, ciudad, disponibilidad, autorización), Nivel por categoría (skills agrupados por categoría en pills azules).
- Responsive: columnas se apilan en móvil.

#### Backend — API de perfil de candidato por ID
- `IProfileService.GetCandidateByIdAsync(Guid candidateId)` — nuevo método en la interfaz.
- `ProfileService.GetCandidateByIdAsync` — busca por `Id` del candidato con includes de experiences, educations, certifications y candidateSkills.
- `ProfileController` — nuevo endpoint `GET api/profile/candidate/{candidateId}` devuelve el perfil completo del candidato.
- `CandidateProfileDto` extendido con `List<CandidateSkillDto> Skills` (Name, Category, ProficiencyLevel).
- `MapToProfileDto` actualizado para mapear skills desde `CandidateSkills` con include de `Skill`.
- `ApiAuthService.GetCandidateProfileByIdAsync(Guid candidateId)` — método cliente en WEB que llama al endpoint.

#### Postulantes verificados — Rediseño con lista y % de perfil (`VerifiedApplicants.razor` — rediseñado)
- Página `/verified-applicants` rediseñada con formato de lista de cards.
- **Lista de vacantes**: cards con título, badge de estado (pill), icono de vistas, número grande de postulantes + label. Click navega a los postulantes de esa vacante.
- **Lista de postulantes**: cards con avatar (iniciales), nombre, título profesional, badge de estado (Pendiente/En revisión/Rechazado/Aceptado), fecha de postulación, anillo circular de % perfil completado (conic-gradient verde), flecha chevron animada al hover. Click navega al perfil completo del candidato.
- Estado vacío cuando una vacante no tiene postulantes.
- Hover: borde azul + shadow suave + flecha animada.

#### CSS (`components.css`)
- Estilos para hero slider, 70/30 grid, applicant list con progress ring.
- Estilos para modal (eliminado posteriormente al migrar a página completa).
- Estilos CV: `.cv-card`, `.cv-header-card`, `.cv-avatar`, `.cv-name`, `.cv-title`, `.cv-header-meta`, `.cv-header-summary`, `.cv-header-contact`, `.cv-content-grid` (70/30), `.cv-section-title`, `.cv-timeline-*`, `.cv-cert-*`, `.cv-skills-list`, `.cv-skill-bar`, `.cv-skill-fill`, `.cv-info-list`, `.cv-category-*`, `.cv-skill-pill`.
- Estilos Verified Applicants: `.va-back-bar`, `.va-applicant-list`, `.va-applicant-card`, `.va-applicant-avatar`, `.va-applicant-body`, `.va-applicant-status--*`, `.va-progress-ring` (conic-gradient), `.va-applicant-arrow`, `.va-vacancy-list`, `.va-vacancy-card`, `.va-vacancy-status--*`, `.va-vacancy-views`, `.va-vacancy-applicants`, `.va-vacancy-count`.
- Media queries responsive para todas las nuevas secciones.

#### Archivos nuevos
- `src/OpenToWork.WEB/Components/Pages/ApplicantProfile.razor`
- `src/OpenToWork.WEB/Components/Pages/CompanyDashboard.razor`
- `src/OpenToWork.WEB/Components/Pages/VerifiedApplicants.razor`

#### Archivos modificados
- `src/OpenToWork.API/Controllers/ProfileController.cs` — endpoint `GET candidate/{candidateId}`
- `src/OpenToWork.Core/Interfaces/IProfileService.cs` — `GetCandidateByIdAsync`
- `src/OpenToWork.Core/Services/ProfileService.cs` — implementación + mapping de skills
- `src/OpenToWork.Core/Services/ApplicationService.cs` — `CalculateProfileCompletion`
- `src/OpenToWork.Shared/DTOs/ApplicationDto.cs` — `ProfileCompletionPercentage`
- `src/OpenToWork.Shared/DTOs/CandidateProfileDto.cs` — `Skills` + `CandidateSkillDto`
- `src/OpenToWork.WEB/Services/ApiAuthService.cs` — `GetCandidateProfileByIdAsync`
- `src/OpenToWork.WEB/wwwroot/css/components.css` — todos los estilos nuevos
