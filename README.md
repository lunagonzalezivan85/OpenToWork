# OpenToWork

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
| **Portal Administrativo** | Verificaciones manuales, moderacion, gestion de usuarios, auditoria, pipeline de reclutamiento | 90% Completado + Pipeline de Reclutamiento (21-Ago) — solo quedan bloqueados 2 items de Fase 3 |
| **Portal Corporativo** | Suscripcion mensual, perfiles evaluados, ranking, filtros avanzados | Pendiente |

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

### 6. Migraciones (solo si se modifican entidades)

Crear nueva migracion:

```bash
dotnet ef migrations add NombreMigracion --project src/OpenToWork.Models --startup-project src/OpenToWork.Models
```

Aplicar migracion:

```bash
dotnet ef database update --project src/OpenToWork.Models --startup-project src/OpenToWork.Models
```

### 7. Estructura de puertos

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

### Fase 3: Motor de Evaluacion y Scoring Automatico - Pendiente

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
> **Checklist original (automatizacion via ValidationService/ScoringService) — sigue sin implementarse:**

- [ ] Entidades de scoring (`PTCandidateScore`, `PTVerification`, `PTCandidateReference`)
- [ ] ValidationService: verificacion automatica (LinkedIn, portafolio, coherencia cronologica)
- [ ] ScoringService: indices de Estabilidad, Confiabilidad, Evidencia
- [ ] CompatibilityService: match candidato-vacante
- [ ] API endpoints: `/api/candidates/{id}/score`, `/api/candidates/{id}/verifications`
- [ ] Dashboard candidato: mostrar scores y verificaciones en el perfil
- [ ] Referencias laborales: CRUD en wizard y perfil
- [ ] Pruebas de habilidades: `PTSkillTest`, `PTCandidateTestResult`

> **Nota (2026-08-24, Dsiezar):** Iluna construyo un **Pipeline de Reclutamiento** (ver Bitacora, sesion 21-Ago) que cubre gran parte del *objetivo* de negocio de Fase 3 (evaluar y verificar candidatos antes de mostrarlos a la empresa), pero con una **arquitectura distinta a la planeada aqui**: es un flujo de **evaluacion manual/asistida por un reclutador** (checklist de investigacion, evaluacion tecnica, entrevista cultural, score general por etapa) en vez de un motor 100% automatico (`ValidationService`/`ScoringService`/`CompatibilityService`). Entidades nuevas: `PTCandidateRecruitment`, `PTInvestigationChecklist`, `PTReferenceCheck`, `PTTechnicalEvaluation`, `PTRecruitmentStageLog`, `PTRecruitmentDismissal` — no `PTCandidateScore`/`PTVerification` como decia el checklist original. Se deja el checklist original sin marcar porque tecnicamente no es lo que se construyo, pero el objetivo de negocio (candidatos evaluados antes de llegar a la empresa) ya tiene una primera version funcionando. Alinea bien con la definicion estrategica consolidada de la sesion 2026-08-15 ("TD revisa candidatos" antes del shortlist).

### Fase 4: Portal Administrativo - 90% COMPLETADA (por Dsiezar) + Pipeline de Reclutamiento (por Iluna)

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
- [ ] Verificaciones manuales (aprobar/rechazar `PTVerification`) — **bloqueado por Fase 3** (checklist original; el Pipeline de Reclutamiento ya cubre una version distinta de esto, ver nota arriba)
- [ ] Revision de validaciones automaticas — **bloqueado por Fase 3** (checklist original)
- [x] Gestion de roles de usuario (cambiar rol, no solo activar/desactivar) (Dsiezar, 29-Ago)

**Deuda tecnica documentada (4 items) — resueltos 29-Ago (Dsiezar):**
- [x] Unificar `AdminAuthService` con `AuthService` (logica duplicada) — extraida a `ITokenCryptoService` compartido en Core
- [x] Optimizar `AdminVacancyService` (carga tablas completas en memoria antes de paginar) — ahora traduce a `UNION ALL` con `Skip/Take` del lado del servidor
- [x] Mover `LocalStorageService`/`LanguageService` de AdminWEB a SharedUI
- [x] Centralizar guard de autenticacion en `AdminLayout` (copiado en 9 paginas; ademas protegia por primera vez las 4 paginas del Pipeline de Reclutamiento, que no tenian guard)

Solo quedan bloqueados los 2 items que dependen de entidades de Fase 3 (`PTVerification`/`ValidationService`, aun no existen).

### Fase 5: Portal Corporativo - Pendiente

- [ ] Crear proyecto `OpenToWork.CorporateAPI` (puerto 5002, JWT independiente)
- [ ] Crear proyecto `OpenToWork.CorporateWEB` (puerto 5102)
- [ ] Entidad `COCompany` — Name, Industry, Size, Website, LogoUrl
- [ ] Entidad `COSubscription` — CompanyId, Plan (Basic/Pro/Enterprise), Status, StartDate, EndDate, MonthlyFee
- [ ] Entidad `COSearchHistory` — CompanyId, Filters, ResultCount, SearchedAt
- [ ] Entidad `COCandidateView` — CompanyId, CandidateId, ScoreSnapshot, ViewedAt
- [ ] Registro de empresas + wizard de empresa
- [ ] Sistema de suscripciones (planes: Basic, Pro, Enterprise)
- [ ] Busqueda avanzada con filtros por score, confiabilidad, estabilidad
- [ ] Vista de perfiles evaluados con checkmarks de verificacion
- [ ] Ranking automatico de candidatos por compatibilidad
- [ ] Reportes avanzados
- [ ] Migracion EF Core para entidades corporativas

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

## Tareas Pendientes Resumidas

> **Total: ~45 tareas pendientes** | Prioridad: **Fase 3** (desbloquea verificaciones del portal admin)

| Fase | Tareas pendientes | Bloquea a |
|------|-------------------|-----------|
| **Fase 3** | 15 tareas (entidades, servicios, API, UI) | Fase 4 (verificaciones), Fase 5 (perfiles evaluados) |
| **Fase 4** | 3 tareas + 4 deuda tecnica | — |
| **Fase 5** | 13 tareas (proyecto nuevo, entidades, suscripciones, busqueda) | Fase 6 |
| **Fase 6** | 4 tareas (servicios premium) | — |
| **Fase 7** | 4 tareas (integraciones externas) | — |
| **Fase 8** | 4 tareas (pruebas, despliegue) | — |

**Bugs resueltos en main (fixes de Dsiezar mergeados):**
- [x] `#blazor-error-ui` siempre visible en `OpenToWork.WEB` — corregido con `display: none`
- [x] Google OAuth config en `OpenToWork.API` — corregido: lee `GoogleOAuth:ClientId` y solo registra si hay credenciales

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

1. **Fase 3 - Motor de Evaluacion y Scoring (PRIORIDAD MAXIMA):**
   - Crear entidades: `PTCandidateScore`, `PTVerification`, `PTCandidateReference`, `PTSkillTest`, `PTCandidateTestResult`
   - Migracion EF Core
   - Implementar `ValidationService` (verificacion automatica: LinkedIn, portafolio, coherencia cronologica)
   - Implementar `ScoringService` (indices: Estabilidad, Confiabilidad, Evidencia)
   - Implementar `CompatibilityService` (match candidato-vacante)
   - API endpoints: `/api/candidates/{id}/score`, `/api/candidates/{id}/verifications`
   - UI: scores y verificaciones en el perfil del candidato (bento cards con los 4 indices)
   - Referencias laborales: CRUD en wizard (nuevo paso) y perfil
   - Pruebas de habilidades: UI basica
   - i18n keys para scores, verificaciones, referencias (es/en)
   - **Validacion: ejecutar API + WEB, verificar pantallas funcionen correctamente antes de avanzar**
   - **Validacion: comprobar patron de diseno One UI (squircles, pill buttons, Bento Grid, temas)**

2. **Fase 4 - Portal Administrativo (completado 29-Ago salvo lo bloqueado por Fase 3):**
   - Verificaciones manuales (aprobar/rechazar `PTVerification`) — requiere Fase 3, sigue pendiente
   - Revision de validaciones automaticas — requiere Fase 3, sigue pendiente
   - [x] Gestion de roles de usuario (cambiar rol, no solo activar/desactivar) (Dsiezar, 29-Ago)
   - [x] Resueltos los 4 items de deuda tecnica (Dsiezar, 29-Ago):
     - Unificar `AdminAuthService` con `AuthService`
     - Optimizar `AdminVacancyService` (paginacion en BD, no en memoria)
     - Mover `LocalStorageService`/`LanguageService` a SharedUI
     - Centralizar guard de autenticacion en `AdminLayout`
   - **Validacion: ejecutado AdminAPI + AdminWEB contra MySQL real, pantallas verificadas en navegador**

3. **Fase 5 - Portal Corporativo (puede iniciar estructura base en paralelo con Fase 3):**
   - Crear `OpenToWork.CorporateAPI` (puerto 5002, JWT independiente)
   - Crear `OpenToWork.CorporateWEB` (puerto 5102)
   - Entidades: `COCompany`, `COSubscription`, `COSearchHistory`, `COCandidateView`
   - Registro de empresas + wizard de empresa
   - Sistema de suscripciones (planes: Basic, Pro, Enterprise)
   - Busqueda avanzada con filtros por score (requiere Fase 3 terminada)
   - Vista de perfiles evaluados con checkmarks
   - Ranking automatico de candidatos por compatibilidad
   - Reportes avanzados
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

## Notas de Actualizacion

> **Regla obligatoria:** Todo desarrollador debe agregar sus notas de cambios en esta seccion cada vez que haga un commit en `main`. El formato es: fecha, nombre del desarrollador, fase, resumen de cambios. Esto mantiene a ambos enterados del progreso sin necesidad de revisar commits uno por uno.

### Estado actual del proyecto

- **Fase 1 (Fundacion):** COMPLETADA
- **Fase 2 (Portal de Candidatos):** 80% completada (Iluna) — funcional pero pendiente de pulido UI/UX y validacion de pantallas
- **Fase 3 (Motor de Evaluacion y Scoring):** el checklist original (ValidationService/ScoringService automaticos) sigue pendiente, pero Iluna ya construyo un **Pipeline de Reclutamiento manual** (21-Ago) que cubre el objetivo de negocio con otra arquitectura — ver nota en la seccion "Fases del Proyecto"
- **Fase 4 (Portal Administrativo):** 85% completada (Dsiezar) + Pipeline de Reclutamiento completo (Iluna, 21-Ago: consola de candidatos, kanban, checklist de investigacion, evaluaciones tecnicas, entrevistas culturales, score general) — faltan gestion de roles y 4 items de deuda tecnica de Dsiezar
- **Fase 5 (Portal Corporativo):** Pendiente — la estructura base puede iniciar en paralelo con Fase 3
- **Fases 6-8:** Pendientes

### Indicaciones para continuar

1. **Culminar las fases pendientes en orden de prioridad.** Fase 3 primero, despues completar Fase 4, luego Fase 5.
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

## Bitacora de Cambios

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

## Tareas Pendientes — Portal Administrativo

> **Contexto:** El portal administrativo está al 90%. Lo que falta está bloqueado por la Fase 3 (Motor de Evaluación); todo lo que no dependía de Fase 3 se completó el 29-Ago (Dsiezar).

### Pendientes bloqueados por Fase 3 (Motor de Scoring)

- [ ] **Verificaciones manuales** — Aprobar/rechazar `PTVerification` desde el panel admin. Requiere que existan entidades de verificación (Fase 3).
- [ ] **Revisión de validaciones automáticas** — Ver el resultado de validaciones automáticas (LinkedIn, portafolio, coherencia cronológica) desde el admin. Requiere `ValidationService` (Fase 3).
- [ ] **Gestión de scores de candidatos** — Ver y gestionar los índices de Estabilidad, Confiabilidad y Evidencia de cada candidato desde el admin.

### Completados 29-Ago (Dsiezar)

- [x] **Gestión de roles de usuario** — El admin ahora puede cambiar el `PrimaryRole` (Candidato/Empresa/Admin) de cualquier usuario desde `/users`, con guardia de auto-bloqueo y validación de valor de rol. Verificado end-to-end contra MySQL real.
- [ ] **Pruebas unitarias para AdminAPI** — Crear `OpenToWork.AdminTests` con pruebas de integración contra `localhost:5001` (login admin, dashboard metrics, users CRUD, vacancies moderation, skills CRUD, audit log, export CSV). *(sigue pendiente, no formaba parte de la deuda técnica original)*
- [ ] **Pruebas de seguridad admin** — Verificar que un candidato no puede acceder a endpoints admin, que el auto-bloqueo funciona, que la paginación no acepta valores negativos. *(sigue pendiente)*

### Deuda técnica documentada (4 items) — resueltos 29-Ago (Dsiezar)

- [x] Unificar `AdminAuthService` con `AuthService` (lógica duplicada) — crypto de tokens extraída a `ITokenCryptoService` en Core
- [x] Optimizar `AdminVacancyService` (carga tablas completas en memoria antes de paginar) — ahora usa `Concat` a nivel de `IQueryable` para traducir a `UNION ALL` con paginación en el servidor
- [x] Mover `LocalStorageService`/`LanguageService` de AdminWEB a SharedUI — `LanguageService` unificado recibiendo el arreglo de secciones por constructor
- [x] Centralizar guard de autenticación en `AdminLayout` (antes copiado en 9 páginas) — como efecto colateral, protege por primera vez las 4 páginas del Pipeline de Reclutamiento que no tenían guard

---

## Bitácora de Cambios

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
