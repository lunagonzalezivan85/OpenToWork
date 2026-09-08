-- ============================================
-- OpenToWork - Seed Data Hosteleria (v2)
-- Ejecutar despues de aplicar todas las migraciones
-- Password para TODOS los usuarios: Empresa123!
-- (hash BCrypt valido generado via API)
-- ============================================

USE OpenToWorkDb;

-- ============================================
-- 1. Usuarios empresa (hosteleria)
-- ============================================

INSERT INTO SC_Users (Id, Email, PasswordHash, PrimaryRole, IsActive, EmailVerified, CreatedAt, IsDeleted)
VALUES
('a1111111-1111-1111-1111-111111111111', 'rrhh@hotelsolcaribe.com', '$2a$11$FyRm9WWY1JbDgBjFTqv8UuzkGgJiKgRwrMnaHJhMGnC43I2t2GZ7i', 1, 1, 1, NOW(), 0),
('a2222222-2222-2222-2222-222222222222', 'rrhh@lapaella.com',       '$2a$11$FyRm9WWY1JbDgBjFTqv8UuzkGgJiKgRwrMnaHJhMGnC43I2t2GZ7i', 1, 1, 1, NOW(), 0),
('a3333333-3333-3333-3333-333333333333', 'rrhh@cateringdelmar.com', '$2a$11$FyRm9WWY1JbDgBjFTqv8UuzkGgJiKgRwrMnaHJhMGnC43I2t2GZ7i', 1, 1, 1, NOW(), 0);

-- ============================================
-- 2. Perfiles de empresa (hosteleria)
-- ============================================

INSERT INTO PT_Companies (Id, SCUserId, Name, LegalName, Description, Website, Country, City, Address, Industry, CompanySize, ContactName, ContactPosition, ContactEmail, ContactPhone, IsVerified, Status, CreatedAt, IsDeleted)
VALUES
(
    'b1111111-1111-1111-1111-111111111111',
    'a1111111-1111-1111-1111-111111111111',
    'Hotel Sol Caribe',
    'Hoteles Sol Caribe S.A.S.',
    'Cadena hotelera de 4 estrellas con 3 propiedades en el Caribe colombiano. 180 habitaciones, restaurante gourmet, spa y centro de convenciones. Famosos por nuestra hospitalidad y servicio al cliente.',
    'https://hotelsolcaribe.com',
    'Colombia', 'Cartagena', 'Bocagrande, Av. San Martin 6-120',
    'Hosteleria / Hoteles', 220,
    'Carolina Restrepo', 'Directora de RRHH',
    'rrhh@hotelsolcaribe.com', '+57 300 123 4567',
    1, 3, NOW(), 0
),
(
    'b2222222-2222-2222-2222-222222222222',
    'a2222222-2222-2222-2222-222222222222',
    'Grupo La Paella',
    'Grupo Gastronomico La Paella S.L.',
    'Grupo de restauracion espanol con 8 restaurantes en Madrid y Barcelona. Especializados en cocina mediterranea, arroces y mariscos frescos. Mas de 25 anos de tradicion.',
    'https://grupolapaella.es',
    'Espana', 'Madrid', 'Calle de la Cava Baja 24',
    'Hosteleria / Restaurantes', 160,
    'Javier Moreno', 'Gerente de Personal',
    'rrhh@grupolapaella.es', '+34 910 555 123',
    1, 3, NOW(), 0
),
(
    'b3333333-3333-3333-3333-333333333333',
    'a3333333-3333-3333-3333-333333333333',
    'Catering Del Mar',
    'Catering Del Mar Barcelona S.L.',
    'Empresa lider en catering de eventos en Barcelona. Bodas, eventos corporativos y convenciones. Equipo de 90 profesionales entre cocina, sala y logistica.',
    'https://cateringdelmar.es',
    'Espana', 'Barcelona', 'Passeig de Gracia 45',
    'Hosteleria / Catering', 90,
    'Marta Puig', 'Coordinadora de RRHH',
    'rrhh@cateringdelmar.com', '+34 930 222 888',
    0, 2, NOW(), 0
);

-- ============================================
-- 3. Vacantes permanentes (hosteleria)
-- Status: 0=Draft, 1=Active, 2=Closed
-- ContractType: 0=FullTime, 1=PartTime, 2=Contract, 3=Freelance
-- WorkMode: 0=OnSite, 1=Hybrid, 2=Remote
-- ExperienceLevel: 0=Junior, 1=Mid, 2=Senior, 3=Lead
-- EnglishLevel: 0=None, 1=Basic, 2=Intermediate, 3=Advanced, 4=Native
-- ============================================

INSERT INTO PT_Vacancies (Id, PT_CompanyId, Title, Description, Requirements, SalaryMin, SalaryMax, Location, ContractType, WorkMode, Category, ExperienceLevel, EnglishLevel, Status, PublishedAt, ViewsCount, CreatedAt, IsDeleted)
VALUES
(
    'e1111111-1111-1111-1111-111111111111', 'b1111111-1111-1111-1111-111111111111',
    'Chef de Parte - Cocina Internacional',
    'Buscamos Chef de Parte para nuestro restaurante gourmet frente al mar. Te encargaras de la estacion caliente elaborando platos de cocina internacional y caribeña con producto local fresco.',
    '- 3+ anos de experiencia como chef de parte\n- Conocimiento de HACCP y normas de sanidad\n- Cocina internacional y caribena\n- Trabajo bajo presion en servicio de alto volumen\n- Disponibilidad para turnos rotativos',
    3500000, 5000000, 'Cartagena, Colombia', 0, 0, 'Cocina', 1, 2, 1, NOW(), 87, NOW(), 0
),
(
    'e2222222-2222-2222-2222-222222222222', 'b1111111-1111-1111-1111-111111111111',
    'Recepcionista de Hotel Bilingue',
    'Seras la primera imagen de nuestro hotel. Atencion a huespedes en espanol e ingles, check-in/check-out, gestion de reservas y resolucion de incidencias con excelencia en servicio.',
    '- 2+ anos de experiencia en recepcion hotelera\n- Ingles avanzado (B2/C1) imprescindible\n- Manejo de PMS (Opera, Sihot o similar)\n- Excelente presencia y trato al cliente\n- Turnos rotativos incluyendo fines de semana',
    2800000, 3800000, 'Cartagena, Colombia', 0, 0, 'Recepcion', 1, 3, 1, NOW(), 132, NOW(), 0
),
(
    'e3333333-3333-3333-3333-333333333333', 'b1111111-1111-1111-1111-111111111111',
    'Gobernante/a de Hotel (Housekeeping)',
    'Dirigiras el equipo de gobernanta de nuestras 180 habitaciones. Planificacion de limpiezas, control de calidad, gestion de inventario de amenidades y coordinacion con mantenimiento.',
    '- 3+ anos en departamento de pisos hotelero\n- Experiencia liderando equipos de 10+ personas\n- Gestion de inventarios y consumos\n- Conocimiento de estandares de calidad hotelera\n- Portugues o ingles valorable',
    3200000, 4200000, 'Cartagena, Colombia', 0, 0, 'Housekeeping', 2, 1, 1, NOW(), 45, NOW(), 0
),
(
    'e4444444-4444-4444-4444-444444444444', 'b2222222-2222-2222-2222-222222222222',
    'Camarero/a de Sala - Restaurante Gourmet',
    'Buscamos camareros/as profesionales para nuestro restaurante de cocina mediterranea en el centro de Madrid. Servicio de sala, maridaje, recomendacion de carta y atencion personalizada.',
    '- 2+ anos de experiencia en sala en restauracion\n- Conocimiento de vinos y maridaje\n- Manejo de TPV digital\n- Orientacion al cliente y trabajo en equipo\n- Incorporacion inmediata',
    1800000, 2400000, 'Madrid, Espana', 0, 0, 'Sala', 1, 1, 1, NOW(), 156, NOW(), 0
),
(
    'e5555555-5555-5555-5555-555555555555', 'b2222222-2222-2222-2222-222222222222',
    'Segundo/a de Cocina (Sous Chef)',
    'Apoya al Jefe de Cocina en la gestion diaria de nuestra cocina de arroces y pescado. Supervision de equipo, control de costes, elaboracion de menus y garantia de calidad.',
    '- 5+ anos de experiencia en cocina de restaurante\n- 2+ anos como segundo de cocina\n- Especialidad en arroces y producto mediterraneo\n- Gestion de equipos de 8+ personas\n- Control de costes y escandallos',
    2200000, 3000000, 'Madrid, Espana', 0, 0, 'Cocina', 2, 2, 1, NOW(), 98, NOW(), 0
),
(
    'e6666666-6666-6666-6666-666666666666', 'b3333333-3333-3333-3333-333333333333',
    'Camarero/a de Eventos y Banquetes',
    'Forma parte de nuestro equipo de banquetes para bodas y eventos corporativos en Barcelona. Montaje de salones, servicio emplatado, buffet y cocktail en eventos de hasta 500 invitados.',
    '- 1+ ano de experiencia en banquetes o eventos\n- Servicio emplatado y desbarasado\n- Disponibilidad fines de semana (alta temporada)\n- Vehiculo propio valorable\n- Actitud positiva y dinamismo',
    1600000, 2100000, 'Barcelona, Espana', 1, 0, 'Banquetes', 0, 1, 1, NOW(), 203, NOW(), 0
),
(
    'e7777777-7777-7777-7777-777777777777', 'b3333333-3333-3333-3333-333333333333',
    'Pastelero/a - Produccion de Reposteria',
    'Elaboracion de postres y dulces para eventos: tartas de boda, mesas dulces, petit fours. Trabajaras en nuestra central de produccion con producto de temporada.',
    '- 2+ anos de experiencia en pasteleria/reposteria\n- Conocimiento de tecnicas de pasteleria moderna\n- Higiene y manipulacion de alimentos\n- Creatividad y gusto por el detalle',
    1700000, 2300000, 'Barcelona, Espana', 0, 0, 'Pasteleria', 1, 1, 0, NULL, 0, NOW(), 0
),
(
    'e8888888-8888-8888-8888-888888888888', 'b2222222-2222-2222-2222-222222222222',
    'Jefe/a de Sala (Cerrada)',
    'Posicion cerrada. Buscabamos jefe de sala con experiencia en restauracion de alta gama.',
    '- 5+ anos de experiencia\n- Gestion de equipos\n- Ingles avanzado',
    2600000, 3400000, 'Madrid, Espana', 0, 0, 'Sala', 3, 3, 2, DATE_SUB(NOW(), INTERVAL 30 DAY), 289, DATE_SUB(NOW(), INTERVAL 60 DAY), 0
);

-- ============================================
-- 3. Vacantes temporales (hosteleria)
-- ============================================

INSERT INTO PT_TempVacancies (Id, SCUserId, Title, Description, Requirements, SalaryMin, SalaryMax, Location, ContractType, ExpiresAt, IsPublished, Category, ExperienceLevel, EnglishLevel, WorkMode, CreatedAt, IsDeleted)
VALUES
(
    'f1111111-1111-1111-1111-111111111111', 'a1111111-1111-1111-1111-111111111111',
    'Extra de Sala - Temporada Alta',
    'Refuerzo de personal de sala para temporada de diciembre y enero. Turnos de noche y fines de semana en hotel de playa.',
    '- Experiencia previa en sala o barra\n- Disponibilidad inmediata\n- Trato excelente al cliente',
    1500000, 2000000, 'Cartagena, Colombia', 2, DATE_ADD(NOW(), INTERVAL 45 DAY), 1, 'Sala', 0, 1, 0, NOW(), 0
),
(
    'f2222222-2222-2222-2222-222222222222', 'a2222222-2222-2222-2222-222222222222',
    'Cocinero/a de Refuerzo - Eventos',
    'Cocinero/a para refuerzo en eventos privados y catering de fin de semana. Elaboracion de menus cerrados para 50-150 comensales.',
    '- 2+ anos de experiencia en cocina\n- Elaboracion de menus en volumen\n- Incorporacion inmediata',
    1800000, 2500000, 'Madrid, Espana', 2, DATE_ADD(NOW(), INTERVAL 30 DAY), 1, 'Cocina', 1, 1, 0, NOW(), 0
);

-- ============================================
-- 4. Skills (hosteleria)
-- ============================================

INSERT INTO PT_Skills (Id, Name, Category, CreatedAt, IsDeleted)
VALUES
('51111111-1111-1111-1111-111111111111', 'Cocina Internacional', 'Cocina', NOW(), 0),
('52222222-2222-2222-2222-222222222222', 'Cocina Mediterranea', 'Cocina', NOW(), 0),
('53333333-3333-3333-3333-333333333333', 'Reposteria y Pasteleria', 'Cocina', NOW(), 0),
('54444444-4444-4444-4444-444444444444', 'HACCP / Seguridad Alimentaria', 'Cocina', NOW(), 0),
('55555555-5555-5555-5555-555555555555', 'Servicio de Sala', 'Sala', NOW(), 0),
('56666666-6666-6666-6666-666666666666', 'Maridaje de Vinos', 'Sala', NOW(), 0),
('57777777-7777-7777-7777-777777777777', 'Cocteleria', 'Barra', NOW(), 0),
('58888888-8888-8888-8888-888888888888', 'Cafe y Barista', 'Barra', NOW(), 0),
('59999999-9999-9999-9999-999999999999', 'Atencion al Cliente', 'Transversal', NOW(), 0),
('5aaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Gestion de PMS (Opera/Sihot)', 'Recepcion', NOW(), 0),
('5bbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Ingles B2', 'Idiomas', NOW(), 0),
('5ccccccc-cccc-cccc-cccc-cccccccccccc', 'Ingles C1', 'Idiomas', NOW(), 0),
('5ddddddd-dddd-dddd-dddd-dddddddddddd', 'Gestion de Equipos', 'Gestion', NOW(), 0),
('5fffffff-ffff-ffff-ffff-ffffffffffff', 'Control de Costes', 'Gestion', NOW(), 0),
('5eeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'Banquetes y Eventos', 'Sala', NOW(), 0);

-- ============================================
-- 5. Usuarios candidatos
-- Password para todos: Empresa123!
-- ============================================

INSERT INTO SC_Users (Id, Email, PasswordHash, PrimaryRole, IsActive, EmailVerified, CreatedAt, IsDeleted)
VALUES
('c1111111-1111-1111-1111-111111111111', 'ana.martinez@gmail.com',        '$2a$11$FyRm9WWY1JbDgBjFTqv8UuzkGgJiKgRwrMnaHJhMGnC43I2t2GZ7i', 0, 1, 1, NOW(), 0),
('c2222222-2222-2222-2222-222222222222', 'luis.fernandez@hotmail.com',   '$2a$11$FyRm9WWY1JbDgBjFTqv8UuzkGgJiKgRwrMnaHJhMGnC43I2t2GZ7i', 0, 1, 1, NOW(), 0),
('c3333333-3333-3333-3333-333333333333', 'sofia.torres@outlook.com',     '$2a$11$FyRm9WWY1JbDgBjFTqv8UuzkGgJiKgRwrMnaHJhMGnC43I2t2GZ7i', 0, 1, 1, NOW(), 0),
('c4444444-4444-4444-4444-444444444444', 'javier.moreno@outlook.com',    '$2a$11$FyRm9WWY1JbDgBjFTqv8UuzkGgJiKgRwrMnaHJhMGnC43I2t2GZ7i', 0, 1, 1, NOW(), 0);

-- ============================================
-- 6. Perfiles de candidato (hosteleria)
-- ============================================

INSERT INTO PT_Candidates (Id, SCUserId, FirstName, LastName, Phone, Title, Summary, Country, City, WizardCompleted, WizardStep, YearsOfExperience, HasPassport, Nationality, HasTransport, Availability, CreatedAt, IsDeleted)
VALUES
(
    'd1111111-1111-1111-1111-111111111111', 'c1111111-1111-1111-1111-111111111111',
    'Ana', 'Martinez', '+34 611 222 333',
    'Chef de Parte - Cocina Internacional',
    'Chef de parte con 5 anos de experiencia en restaurantes de cocina internacional y hoteles de 4 estrellas. Especialista en estacion caliente, producto fresco y cocina de mercado. Certificada en HACCP.',
    'Espana', 'Madrid', 1, 10, 5, 1, 'Espanola', 1, 2, NOW(), 0
),
(
    'd2222222-2222-2222-2222-222222222222', 'c2222222-2222-2222-2222-222222222222',
    'Luis', 'Fernandez', '+57 301 555 8888',
    'Recepcionista Hotelero Bilingue',
    'Recepcionista de hotel con 4 anos de experiencia en hoteles de 4 y 5 estrellas. Ingles C1 y portugues intermedio. Dominio de Opera PMS. Especialista en fidelizacion de huespedes.',
    'Colombia', 'Cartagena', 1, 10, 4, 1, 'Colombiana', 0, 1, NOW(), 0
),
(
    'd3333333-3333-3333-3333-333333333333', 'c3333333-3333-3333-3333-333333333333',
    'Sofia', 'Torres', '+34 622 444 777',
    'Camarera Profesional y Sumiller',
    'Camarera con 6 anos de experiencia en restauracion de alta gama en Barcelona. Certificada como sumiller por la UECA. Especialista en maridaje y servicio de vinos.',
    'Espana', 'Barcelona', 1, 10, 6, 1, 'Espanola', 1, 1, NOW(), 0
),
(
    'd4444444-4444-4444-4444-444444444444', 'c4444444-4444-4444-4444-444444444444',
    'Javier', 'Morales', '+57 312 999 777',
    'Cocinero - Cocina Mediterranea',
    'Cocinero con 3 anos de experiencia en cocina mediterranea y de mercado. Apasionado por el producto local y la cocina de temporada. En formacion continua.',
    'Colombia', 'Cartagena', 1, 10, 3, 0, 'Colombiana', 1, 1, NOW(), 0
);

-- ============================================
-- 7. Experiencias laborales
-- ============================================

INSERT INTO PT_CandidateExperiences (Id, PT_CandidateId, CompanyName, JobTitle, Description, StartDate, EndDate, IsCurrentJob, Location, CreatedAt, IsDeleted)
VALUES
(UUID(), 'd1111111-1111-1111-1111-111111111111', 'Hotel Riu Plaza Espana', 'Chef de Parte', 'Gestion de la estacion caliente en restaurante de 300 cubiertos diarios. Elaboracion de carta de temporada y supervision de 4 cocineros de linea.', DATE_SUB(NOW(), INTERVAL 36 MONTH), NULL, 1, 'Madrid, Espana', NOW(), 0),
(UUID(), 'd1111111-1111-1111-1111-111111111111', 'Restaurante Casa Mono', 'Cocinera', 'Cocina de mercado y producto. Elaboracion de entrantes y postres. Control de compras y proveedores locales.', DATE_SUB(NOW(), INTERVAL 84 MONTH), DATE_SUB(NOW(), INTERVAL 36 MONTH), 0, 'Barcelona, Espana', NOW(), 0),
(UUID(), 'd2222222-2222-2222-2222-222222222222', 'Hotel Melia Cartagena', 'Recepcionista Turno Manana', 'Check-in/check-out de huespedes VIP, gestion de reservas grupales y resolucion de incidencias. Valoracion media de huespedes 4.8/5.', DATE_SUB(NOW(), INTERVAL 30 MONTH), NULL, 1, 'Cartagena, Colombia', NOW(), 0),
(UUID(), 'd2222222-2222-2222-2222-222222222222', 'Hostal El Viajero', 'Recepcionista Nocturno', 'Atencion al huesped en turno nocturno, facturacion y cierre de caja. Gestion de overbooking en temporada alta.', DATE_SUB(NOW(), INTERVAL 66 MONTH), DATE_SUB(NOW(), INTERVAL 36 MONTH), 0, 'Cartagena, Colombia', NOW(), 0),
(UUID(), 'd3333333-3333-3333-3333-333333333333', 'Restaurante Cinc Sentits', 'Camarera / Sumiller', 'Servicio de sala en restaurante con estrella Michelin. Carta de vinos de 200 referencias y maridaje en menu degustacion.', DATE_SUB(NOW(), INTERVAL 48 MONTH), NULL, 1, 'Barcelona, Espana', NOW(), 0),
(UUID(), 'd4444444-4444-4444-4444-444444444444', 'Restaurante La Mulata', 'Cocinero', 'Cocina caribena y de producto local. Elaboracion de mise en place, fondos y salsas. Apoyo en partida caliente en servicio de 150 cubiertos.', DATE_SUB(NOW(), INTERVAL 24 MONTH), NULL, 1, 'Cartagena, Colombia', NOW(), 0);

-- ============================================
-- 8. Formacion academica
-- ============================================

INSERT INTO PT_CandidateEducations (Id, PT_CandidateId, Institution, Degree, FieldOfStudy, StartDate, EndDate, IsInProgress, CreatedAt, IsDeleted)
VALUES
(UUID(), 'd1111111-1111-1111-1111-111111111111', 'Escuela de Hosteleria de Madrid', 'Tecnico Superior en Direccion de Cocina', 'Gastronomia', DATE_SUB(NOW(), INTERVAL 96 MONTH), DATE_SUB(NOW(), INTERVAL 90 MONTH), 0, NOW(), 0),
(UUID(), 'd2222222-2222-2222-2222-222222222222', 'Universidad de Cartagena', 'Tecnologo en Gestion Hotelera y Turistica', 'Turismo', DATE_SUB(NOW(), INTERVAL 72 MONTH), DATE_SUB(NOW(), INTERVAL 66 MONTH), 0, NOW(), 0),
(UUID(), 'd3333333-3333-3333-3333-333333333333', 'Escuela de Hosteleria de Barcelona', 'Curso de Sumilleria', 'Enologia', DATE_SUB(NOW(), INTERVAL 36 MONTH), DATE_SUB(NOW(), INTERVAL 34 MONTH), 0, NOW(), 0),
(UUID(), 'd4444444-4444-4444-4444-444444444444', 'SENA - Cartagena', 'Tecnico en Cocina', 'Gastronomia', DATE_SUB(NOW(), INTERVAL 60 MONTH), DATE_SUB(NOW(), INTERVAL 54 MONTH), 0, NOW(), 0);

-- ============================================
-- 9. Skills de candidatos
-- ============================================

INSERT INTO PT_CandidateSkills (Id, PT_CandidateId, PT_SkillId, ProficiencyLevel, CreatedAt, IsDeleted)
VALUES
(UUID(), 'd1111111-1111-1111-1111-111111111111', '51111111-1111-1111-1111-111111111111', 4, NOW(), 0),
(UUID(), 'd1111111-1111-1111-1111-111111111111', '52222222-2222-2222-2222-222222222222', 4, NOW(), 0),
(UUID(), 'd1111111-1111-1111-1111-111111111111', '54444444-4444-4444-4444-444444444444', 4, NOW(), 0),
(UUID(), 'd1111111-1111-1111-1111-111111111111', '5bbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 3, NOW(), 0),
(UUID(), 'd2222222-2222-2222-2222-222222222222', '59999999-9999-9999-9999-999999999999', 4, NOW(), 0),
(UUID(), 'd2222222-2222-2222-2222-222222222222', '5aaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 4, NOW(), 0),
(UUID(), 'd2222222-2222-2222-2222-222222222222', '5bbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 4, NOW(), 0),
(UUID(), 'd3333333-3333-3333-3333-333333333333', '55555555-5555-5555-5555-555555555555', 4, NOW(), 0),
(UUID(), 'd3333333-3333-3333-3333-333333333333', '56666666-6666-6666-6666-666666666666', 4, NOW(), 0),
(UUID(), 'd3333333-3333-3333-3333-333333333333', '5ccccccc-cccc-cccc-cccc-cccccccccccc', 4, NOW(), 0),
(UUID(), 'd4444444-4444-4444-4444-444444444444', '52222222-2222-2222-2222-222222222222', 3, NOW(), 0),
(UUID(), 'd4444444-4444-4444-4444-444444444444', '54444444-4444-4444-4444-444444444444', 3, NOW(), 0);

-- ============================================
-- 10. Aplicaciones (postulaciones)
-- Status: 0=Pending, 1=Reviewing, 2=Accepted, 3=Rejected
-- ============================================

INSERT INTO PT_Applications (Id, PT_CandidateId, PT_VacancyId, Status, CoverLetter, CreatedAt, IsDeleted)
VALUES
(UUID(), 'd1111111-1111-1111-1111-111111111111', 'e1111111-1111-1111-1111-111111111111', 1, 'Chef de parte con 5 anos de experiencia en cocina internacional. Me encantaria aportar mi experiencia en producto caribeno a vuestro equipo.', NOW(), 0),
(UUID(), 'd2222222-2222-2222-2222-222222222222', 'e2222222-2222-2222-2222-222222222222', 0, 'Recepcionista hotelera bilingue con 4 anos de experiencia en hoteles de 4 estrellas. Experiencia comprobada con Opera PMS.', NOW(), 0),
(UUID(), 'd3333333-3333-3333-3333-333333333333', 'e4444444-4444-4444-4444-444444444444', 0, 'Camarera y sumiller certificada con experiencia en sala de alta gama. Gran conocimiento de vinos espanoles.', NOW(), 0),
(UUID(), 'd4444444-4444-4444-4444-444444444444', 'e1111111-1111-1111-1111-111111111111', 0, 'Cocinero con 2 anos de experiencia en cocina caribena. Formacion SENA y muchas ganas de crecer.', NOW(), 0),
(UUID(), 'd1111111-1111-1111-1111-111111111111', 'e6666666-6666-6666-6666-666666666666', 2, 'Experiencia en banquetes y eventos para 100+ comensales. Disponibilidad inmediata de fin de semana.', NOW(), 0);
