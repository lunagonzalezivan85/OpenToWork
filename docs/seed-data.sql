-- ============================================
-- OpenToWork - Seed Data para pruebas
-- Ejecutar despues de aplicar todas las migraciones
-- ============================================

USE OpenToWorkDb;

-- ============================================
-- 1. Empresas (necesitan usuario SC_Users primero)
-- ============================================

-- Usuario empresa 1
INSERT INTO SC_Users (Id, Email, PasswordHash, PrimaryRole, IsActive, EmailVerified, CreatedAt, IsDeleted)
VALUES (
    'a1111111-1111-1111-1111-111111111111',
    'empresa@techcorp.com',
    '$2a$11$N7qV8x2Y3Z5wQ1rT6uI9pO0lKjHgFEdCbAsDfGhJkLmN0pQ1rSt',
    1, -- Company role
    1, 1, NOW(), 0
);

-- Usuario empresa 2
INSERT INTO SC_Users (Id, Email, PasswordHash, PrimaryRole, IsActive, EmailVerified, CreatedAt, IsDeleted)
VALUES (
    'a2222222-2222-2222-2222-222222222222',
    'contacto@innovatelabs.com',
    '$2a$11$N7qV8x2Y3Z5wQ1rT6uI9pO0lKjHgFEdCbAsDfGhJkLmN0pQ1rSt',
    1,
    1, 1, NOW(), 0
);

-- Usuario empresa 3
INSERT INTO SC_Users (Id, Email, PasswordHash, PrimaryRole, IsActive, EmailVerified, CreatedAt, IsDeleted)
VALUES (
    'a3333333-3333-3333-3333-333333333333',
    'rrhh@globalsoft.com',
    '$2a$11$N7qV8x2Y3Z5wQ1rT6uI9pO0lKjHgFEdCbAsDfGhJkLmN0pQ1rSt',
    1,
    1, 1, NOW(), 0
);

-- ============================================
-- 2. Perfiles de empresa
-- ============================================

INSERT INTO PT_Companies (Id, SCUserId, Name, Description, Website, Country, City, Industry, CompanySize, ContactEmail, ContactPhone, IsVerified, CreatedAt, IsDeleted)
VALUES
(
    'b1111111-1111-1111-1111-111111111111',
    'a1111111-1111-1111-1111-111111111111',
    'TechCorp Solutions',
    'Empresa lider en desarrollo de software y consultoria tecnologica. Especializados en soluciones cloud y transformacion digital.',
    'https://techcorp.com',
    'Colombia', 'Bogota',
    'Tecnologia / Software',
    250,
    'empresa@techcorp.com',
    '+57 320 123 4567',
    1, NOW(), 0
),
(
    'b2222222-2222-2222-2222-222222222222',
    'a2222222-2222-2222-2222-222222222222',
    'Innovate Labs',
    'Startup enfocada en inteligencia artificial y machine learning. Creamos productos que transforman industrias.',
    'https://innovatelabs.com',
    'Mexico', 'Ciudad de Mexico',
    'IA / Machine Learning',
    50,
    'contacto@innovatelabs.com',
    '+52 55 9876 5432',
    1, NOW(), 0
),
(
    'b3333333-3333-3333-3333-333333333333',
    'a3333333-3333-3333-3333-333333333333',
    'GlobalSoft Inc.',
    'Corporacion multinacional de desarrollo de software empresarial. Presencia en 15 paises.',
    'https://globalsoft.com',
    'Argentina', 'Buenos Aires',
    'Software Empresarial',
    1000,
    'rrhh@globalsoft.com',
    '+54 11 5555 4444',
    0, NOW(), 0
);

-- ============================================
-- 3. Vacantes permanentes (pt_vacancies)
-- Status: 0=Draft, 1=Active, 2=Closed
-- ContractType: 0=FullTime, 1=PartTime, 2=Contract, 3=Freelance
-- WorkMode: 0=OnSite, 1=Hybrid, 2=Remote
-- ExperienceLevel: 0=Junior, 1=Mid, 2=Senior, 3=Lead
-- EnglishLevel: 0=None, 1=Basic, 2=Intermediate, 3=Advanced, 4=Native
-- ============================================

INSERT INTO PT_Vacancies (Id, PT_CompanyId, Title, Description, Requirements, SalaryMin, SalaryMax, Location, ContractType, WorkMode, Category, ExperienceLevel, EnglishLevel, Status, PublishedAt, ViewsCount, CreatedAt, IsDeleted)
VALUES
(
    UUID(), 'b1111111-1111-1111-1111-111111111111',
    'Desarrollador Backend Senior (.NET)',
    'Buscamos un Desarrollador Backend Senior con experiencia en .NET y arquitectura de microservicios. Lideraras el desarrollo de APIs de alto rendimiento y participaras en decisiones arquitectonicas clave.',
    '- 5+ anos de experiencia en C# y .NET\n- Experiencia con Entity Framework Core\n- Conocimiento de microservicios y Docker\n- Experiencia con SQL Server o MySQL\n- Git y CI/CD',
    6000000, 9000000,
    'Bogota, Colombia',
    0, 1, 'Desarrollo', 2, 3,
    1, NOW(), 145, NOW(), 0
),
(
    UUID(), 'b1111111-1111-1111-1111-111111111111',
    'Desarrollador Frontend React',
    'Unete a nuestro equipo para construir interfaces modernas y responsivas con React y TypeScript. Trabajaremos en la nueva plataforma de e-commerce.',
    '- 3+ anos de experiencia en React\n- TypeScript avanzado\n- CSS Modules / Tailwind\n- Experiencia con testing (Jest, RTL)\n- Conocimiento de UX/UI',
    3500000, 5500000,
    'Bogota, Colombia',
    0, 2, 'Desarrollo', 1, 2,
    1, NOW(), 89, NOW(), 0
),
(
    UUID(), 'b1111111-1111-1111-1111-111111111111',
    'Arquitecto de Software',
    'Lideraras el diseno arquitectural de nuestra plataforma SaaS. Definiras estandares, patrones y mejores practicas para todo el equipo de desarrollo.',
    '- 8+ anos de experiencia en desarrollo de software\n- Experiencia como arquitecto\n- Conocimiento de Azure/AWS\n- Microservicios y event-driven architecture\n- Liderazgo tecnico',
    10000000, 15000000,
    'Bogota, Colombia',
    0, 1, 'Arquitectura', 3, 4,
    1, NOW(), 56, NOW(), 0
),
(
    UUID(), 'b2222222-2222-2222-2222-222222222222',
    'Ingeniero de Machine Learning',
    'Desarrollaras modelos de IA para procesamiento de lenguaje natural y vision computacional. Trabajaras con datasets grandes y deployment en produccion.',
    '- 4+ anos de experiencia en ML/DS\n- Python, TensorFlow/PyTorch\n- Experiencia con MLOps\n- Conocimiento de NLP y LLMs\n- Publicaciones cientificas (deseable)',
    7000000, 12000000,
    'Ciudad de Mexico, Mexico',
    0, 2, 'Inteligencia Artificial', 2, 4,
    1, NOW(), 234, NOW(), 0
),
(
    UUID(), 'b2222222-2222-2222-2222-222222222222',
    'Data Scientist Junior',
    'Oportunidad para talentos emergentes en data science. Analizaras datos, crearas dashboards y participaras en proyectos de ML.',
    '- 1+ ano de experiencia o proyectos relevantes\n- Python (Pandas, NumPy, Scikit-learn)\n- SQL\n- Visualizacion de datos (Power BI, Tableau)\n- Ganas de aprender',
    2500000, 4000000,
    'Ciudad de Mexico, Mexico',
    0, 2, 'Data Science', 0, 1,
    1, NOW(), 178, NOW(), 0
),
(
    UUID(), 'b3333333-3333-3333-3333-333333333333',
    'Project Manager IT',
    'Gestionaras proyectos de software empresarial para clientes globales. Metodologia agil (Scrum/Kanban) y gestion de equipos distribuidos.',
    '- 5+ anos de experiencia en gestion de proyectos IT\n- Certificacion PMP o Scrum (deseable)\n- Experiencia con Jira/Confluence\n- Ingles avanzado\n- Gestion de presupuestos',
    5000000, 8000000,
    'Buenos Aires, Argentina',
    0, 1, 'Gestion', 2, 4,
    1, NOW(), 67, NOW(), 0
),
(
    UUID(), 'b3333333-3333-3333-3333-333333333333',
    'QA Automation Engineer',
    'Implementaras estrategias de testing automatizado para nuestra plataforma. Selenium, Cypress y integracion con CI/CD.',
    '- 3+ anos de experiencia en QA automation\n- Selenium, Cypress o Playwright\n- Integracion con CI/CD (GitHub Actions, Azure DevOps)\n- API testing (Postman, RestSharp)\n- Ingles intermedio',
    3000000, 5000000,
    'Buenos Aires, Argentina',
    0, 2, 'Calidad', 1, 2,
    1, NOW(), 112, NOW(), 0
),
(
    UUID(), 'b3333333-3333-3333-3333-333333333333',
    'DevOps Engineer',
    'Gestionaras la infraestructura cloud, pipelines de CI/CD y observabilidad. Kubernetes, Terraform y monitoreo.',
    '- 4+ anos de experiencia en DevOps\n- Kubernetes, Docker\n- Terraform, Ansible\n- Azure o AWS\n- Monitoreo (Prometheus, Grafana)',
    6000000, 9500000,
    'Remoto, Latinoamerica',
    0, 2, 'Infraestructura', 2, 3,
    1, NOW(), 201, NOW(), 0
),
-- Vacante en Draft (no publicada)
(
    UUID(), 'b1111111-1111-1111-1111-111111111111',
    'Especialista en Ciberseguridad',
    'Buscamos un experto en ciberseguridad para auditar y proteger nuestros sistemas. Pentesting, analisis de vulnerabilidades y respuesta a incidentes.',
    '- 5+ anos en ciberseguridad\n- CEH, OSCP o similar\n- Experiencia con SIEM\n- Pentesting y forense\n- Ingles avanzado',
    8000000, 13000000,
    'Bogota, Colombia',
    0, 1, 'Seguridad', 2, 4,
    0, NULL, 0, NOW(), 0
),
-- Vacante Cerrada
(
    UUID(), 'b2222222-2222-2222-2222-222222222222',
    'Backend Developer Python (Cerrada)',
    'Posicion cerrada. Buscabamos desarrollador Python con experiencia en FastAPI.',
    '- 3+ anos Python\n- FastAPI o Django\n- PostgreSQL\n- Docker',
    4000000, 6500000,
    'Ciudad de Mexico, Mexico',
    0, 2, 'Desarrollo', 1, 2,
    2, DATE_SUB(NOW(), INTERVAL 30 DAY), 340, DATE_SUB(NOW(), INTERVAL 60 DAY), 0
);

-- ============================================
-- 4. Vacantes temporales (pt_tempvacancies)
-- ============================================

INSERT INTO PT_TempVacancies (Id, SCUserId, Title, Description, Requirements, SalaryMin, SalaryMax, Location, ContractType, ExpiresAt, IsPublished, Category, ExperienceLevel, EnglishLevel, WorkMode, CreatedAt, IsDeleted)
VALUES
(
    UUID(), '9bac8f5a-d504-4dcb-834b-ebddce3cb6a0',
    'Freelance - Disenador UX/UI',
    'Proyecto freelance para rediseno de dashboard administrativo. 2 semanas de duracion.',
    '- 3+ anos experiencia UX/UI\n- Figma avanzado\n- Portfolio con proyectos SaaS\n- Disponibilidad inmediata',
    1500000, 3000000,
    'Remoto',
    3, DATE_ADD(NOW(), INTERVAL 14 DAY),
    1, 'Diseno', 1, 1, 2, NOW(), 0
),
(
    UUID(), '9bac8f5a-d504-4dcb-834b-ebddce3cb6a0',
    'Contrato - Desarrollador Full Stack 3 meses',
    'Cubrir licencia por 3 meses. Stack: .NET + React. Proyecto en produccion.',
    '- 4+ anos .NET + React\n- SQL Server\n- Experiencia en produccion\n- Disponibilidad inmediata',
    4000000, 6000000,
    'Medellin, Colombia',
    2, DATE_ADD(NOW(), INTERVAL 90 DAY),
    1, 'Desarrollo', 1, 2, 1, NOW(), 0
),
(
    UUID(), '9bac8f5a-d504-4dcb-834b-ebddce3cb6a0',
    'Part-time - Community Manager Tecnologia',
    'Gestion de redes sociales para startup tecnologica. 20 horas semanales.',
    '- 2+ anos experiencia community management\n- Conocimiento del sector tech\n- Ingles intermedio\n- Creatividad',
    1200000, 2000000,
    'Remoto',
    1, DATE_ADD(NOW(), INTERVAL 45 DAY),
    1, 'Marketing', 0, 1, 2, NOW(), 0
);

-- ============================================
-- 5. Skills
-- ============================================

INSERT INTO PT_Skills (Id, Name, Category, CreatedAt, IsDeleted)
VALUES
(UUID(), 'C#', 'Programacion', NOW(), 0),
(UUID(), '.NET Core', 'Programacion', NOW(), 0),
(UUID(), 'React', 'Frontend', NOW(), 0),
(UUID(), 'TypeScript', 'Frontend', NOW(), 0),
(UUID(), 'Python', 'Programacion', NOW(), 0),
(UUID(), 'SQL Server', 'Base de Datos', NOW(), 0),
(UUID(), 'MySQL', 'Base de Datos', NOW(), 0),
(UUID(), 'Docker', 'DevOps', NOW(), 0),
(UUID(), 'Kubernetes', 'DevOps', NOW(), 0),
(UUID(), 'Azure', 'Cloud', NOW(), 0),
(UUID(), 'AWS', 'Cloud', NOW(), 0),
(UUID(), 'Git', 'Herramientas', NOW(), 0),
(UUID(), 'Figma', 'Diseno', NOW(), 0),
(UUID(), 'Machine Learning', 'IA', NOW(), 0),
(UUID(), 'TensorFlow', 'IA', NOW(), 0),
(UUID(), 'Scrum', 'Metodologias', NOW(), 0),
(UUID(), 'Jira', 'Herramientas', NOW(), 0),
(UUID(), 'Selenium', 'Testing', NOW(), 0),
(UUID(), 'Cypress', 'Testing', NOW(), 0),
(UUID(), 'Terraform', 'DevOps', NOW(), 0);

-- ============================================
-- 6. Usuarios postulantes (candidates)
-- Password para todos: Candidato123!
-- El hash se debe generar con BCrypt, pero usamos el mismo del admin temporalmente
-- y se actualizara al primer login o desde la API
-- ============================================

-- Postulante 1
INSERT INTO SC_Users (Id, Email, PasswordHash, PrimaryRole, IsActive, EmailVerified, CreatedAt, IsDeleted)
VALUES (
    'c1111111-1111-1111-1111-111111111111',
    'juan.perez@gmail.com',
    '$2a$11$N7qV8x2Y3Z5wQ1rT6uI9pO0lKjHgFEdCbAsDfGhJkLmN0pQ1rSt',
    0, -- Candidate
    1, 1, NOW(), 0
);

-- Postulante 2
INSERT INTO SC_Users (Id, Email, PasswordHash, PrimaryRole, IsActive, EmailVerified, CreatedAt, IsDeleted)
VALUES (
    'c2222222-2222-2222-2222-222222222222',
    'maria.gonzalez@hotmail.com',
    '$2a$11$N7qV8x2Y3Z5wQ1rT6uI9pO0lKjHgFEdCbAsDfGhJkLmN0pQ1rSt',
    0,
    1, 1, NOW(), 0
);

-- Postulante 3
INSERT INTO SC_Users (Id, Email, PasswordHash, PrimaryRole, IsActive, EmailVerified, CreatedAt, IsDeleted)
VALUES (
    'c3333333-3333-3333-3333-333333333333',
    'carlos.rodriguez@outlook.com',
    '$2a$11$N7qV8x2Y3Z5wQ1rT6uI9pO0lKjHgFEdCbAsDfGhJkLmN0pQ1rSt',
    0,
    1, 1, NOW(), 0
);

-- ============================================
-- 7. Perfiles de candidato
-- ============================================

INSERT INTO PT_Candidates (Id, SCUserId, FirstName, LastName, Title, Summary, LinkedInUrl, CvUrl, WizardCompleted, WizardStep, Country, City, YearsOfExperience, CreatedAt, IsDeleted)
VALUES
(
    'd1111111-1111-1111-1111-111111111111',
    'c1111111-1111-1111-1111-111111111111',
    'Juan', 'Perez',
    'Desarrollador Backend Senior',
    'Desarrollador Backend con 6 anos de experiencia en C# y .NET. Especializado en APIs de alto rendimiento y microservicios.',
    'https://linkedin.com/in/juanperez',
    'https://drive.google.com/cv/juanperez',
    1, 10, 'Colombia', 'Bogota', 6, NOW(), 0
),
(
    'd2222222-2222-2222-2222-222222222222',
    'c2222222-2222-2222-2222-222222222222',
    'Maria', 'Gonzalez',
    'Frontend Developer',
    'Frontend Developer con 4 anos de experiencia en React y TypeScript. Apasionada por UX y accesibilidad.',
    'https://linkedin.com/in/mariagonzalez',
    NULL,
    1, 10, 'Mexico', 'Ciudad de Mexico', 4, NOW(), 0
),
(
    'd3333333-3333-3333-3333-333333333333',
    'c3333333-3333-3333-3333-333333333333',
    'Carlos', 'Rodriguez',
    'Full Stack Developer',
    'Full Stack Developer con experiencia en .NET, React y Azure. Busco nuevos retos en arquitectura cloud.',
    NULL, NULL,
    0, 5, 'Argentina', 'Buenos Aires', 5, NOW(), 0
);

-- ============================================
-- 8. Aplicaciones (postulaciones)
-- Status: 0=Pending, 1=Reviewing, 2=Accepted, 3=Rejected
-- ============================================

-- Obtener IDs de vacantes dinamicamente
SET @vac1 = (SELECT Id FROM pt_vacancies WHERE Title = 'Desarrollador Backend Senior (.NET)' LIMIT 1);
SET @vac2 = (SELECT Id FROM pt_vacancies WHERE Title = 'Desarrollador Frontend React' LIMIT 1);
SET @vac3 = (SELECT Id FROM pt_vacancies WHERE Title = 'Ingeniero de Machine Learning' LIMIT 1);
SET @vac4 = (SELECT Id FROM pt_vacancies WHERE Title = 'DevOps Engineer' LIMIT 1);

INSERT INTO PT_Applications (Id, PT_CandidateId, PT_VacancyId, Status, CoverLetter, CreatedAt, IsDeleted)
VALUES
(UUID(), 'd1111111-1111-1111-1111-111111111111', @vac1, 1, 'Tengo 6 anos de experiencia en .NET y he liderado migraciones a microservicios. Me entusiasma la oportunidad en TechCorp.', NOW(), 0),
(UUID(), 'd2222222-2222-2222-2222-222222222222', @vac2, 0, 'Como frontend developer con experiencia en React y TypeScript, me encantaria aportar al equipo de e-commerce.', NOW(), 0),
(UUID(), 'd3333333-3333-3333-3333-333333333333', @vac3, 0, 'He trabajado en proyectos de ML con Python y TensorFlow. Tengo experiencia deployando modelos en produccion.', NOW(), 0),
(UUID(), 'd1111111-1111-1111-1111-111111111111', @vac4, 2, 'Mi experiencia en DevOps con Kubernetes y Azure se alinea perfectamente con lo que buscan.', NOW(), 0),
(UUID(), 'd3333333-3333-3333-3333-333333333333', @vac1, 0, 'Full stack developer con fuerte base en backend .NET. Listo para nuevos retos.', NOW(), 0);
