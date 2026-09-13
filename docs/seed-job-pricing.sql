-- Seed inicial de niveles/tipos de puesto, lista de precios y un codigo promocional
-- de ejemplo, para el modulo de precios B2B por vacante (12-Sep-2026, Dsiezar).
--
-- Los 3 niveles y sus tipos son un punto de partida editable desde
-- /pricing/job-levels y /pricing/job-types en el AdminWEB - no son un limite fijo.
-- Los precios base son PLACEHOLDER (calcados de forma proporcional al dummy de
-- 1500 EUR/vacante que existia antes de este modulo) y deben reemplazarse por
-- las tarifas reales de Trato Directo antes de facturar contratos de verdad.

-- ===== Niveles de puesto =====
INSERT INTO PT_JobLevels (Id, Name, ReferenceCoverageDays, WarrantyDays, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
('b1111111-1111-1111-1111-111111111111', 'Personal Operativo', 10, 30, 1, 1, UTC_TIMESTAMP(), 0),
('b2222222-2222-2222-2222-222222222222', 'Encargados y Tecnicos', 15, 45, 2, 1, UTC_TIMESTAMP(), 0),
('b3333333-3333-3333-3333-333333333333', 'Responsables y Cualificados', 20, 60, 3, 1, UTC_TIMESTAMP(), 0);

-- ===== Tipos de puesto (mapeados desde PT_Vacancies.Category existente) =====
INSERT INTO PT_JobTypes (Id, Name, PT_JobLevelId, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
('c1000001-0000-0000-0000-000000000001', 'Camarero/a', 'b1111111-1111-1111-1111-111111111111', 1, 1, UTC_TIMESTAMP(), 0),
('c1000002-0000-0000-0000-000000000002', 'Ayudante de camarero/a', 'b1111111-1111-1111-1111-111111111111', 2, 1, UTC_TIMESTAMP(), 0),
('c1000003-0000-0000-0000-000000000003', 'Ayudante de cocina', 'b1111111-1111-1111-1111-111111111111', 3, 1, UTC_TIMESTAMP(), 0),
('c1000004-0000-0000-0000-000000000004', 'Ayudante de barra', 'b1111111-1111-1111-1111-111111111111', 4, 1, UTC_TIMESTAMP(), 0),
('c2000001-0000-0000-0000-000000000001', 'Cocinero/a', 'b2222222-2222-2222-2222-222222222222', 1, 1, UTC_TIMESTAMP(), 0),
('c2000002-0000-0000-0000-000000000002', 'Barman / Bartender', 'b2222222-2222-2222-2222-222222222222', 2, 1, UTC_TIMESTAMP(), 0),
('c2000003-0000-0000-0000-000000000003', 'Recepcionista de hotel', 'b2222222-2222-2222-2222-222222222222', 3, 1, UTC_TIMESTAMP(), 0),
('c3000001-0000-0000-0000-000000000001', 'Jefe/a de sala', 'b3333333-3333-3333-3333-333333333333', 1, 1, UTC_TIMESTAMP(), 0),
('c3000002-0000-0000-0000-000000000002', 'Responsable de local', 'b3333333-3333-3333-3333-333333333333', 2, 1, UTC_TIMESTAMP(), 0);

-- ===== Precios base vigentes (placeholder, EUR sin IVA por posicion) =====
INSERT INTO PT_JobTypePrices (Id, PT_JobTypeId, BasePrice, Currency, EffectiveFrom, CreatedAt, IsDeleted) VALUES
(UUID(), 'c1000001-0000-0000-0000-000000000001', 900.00, 'EUR', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 0),
(UUID(), 'c1000002-0000-0000-0000-000000000002', 800.00, 'EUR', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 0),
(UUID(), 'c1000003-0000-0000-0000-000000000003', 800.00, 'EUR', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 0),
(UUID(), 'c1000004-0000-0000-0000-000000000004', 850.00, 'EUR', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 0),
(UUID(), 'c2000001-0000-0000-0000-000000000001', 1400.00, 'EUR', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 0),
(UUID(), 'c2000002-0000-0000-0000-000000000002', 1200.00, 'EUR', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 0),
(UUID(), 'c2000003-0000-0000-0000-000000000003', 1100.00, 'EUR', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 0),
(UUID(), 'c3000001-0000-0000-0000-000000000001', 2200.00, 'EUR', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 0),
(UUID(), 'c3000002-0000-0000-0000-000000000002', 2500.00, 'EUR', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 0);

-- ===== Backfill: PT_Vacancies.Category (texto libre) -> PT_JobTypeId =====
UPDATE PT_Vacancies SET PT_JobTypeId = 'c1000001-0000-0000-0000-000000000001' WHERE Category = 'Camarero' AND PT_JobTypeId IS NULL;
UPDATE PT_Vacancies SET PT_JobTypeId = 'c1000002-0000-0000-0000-000000000002' WHERE Category = 'AyudanteCamarero' AND PT_JobTypeId IS NULL;
UPDATE PT_Vacancies SET PT_JobTypeId = 'c1000003-0000-0000-0000-000000000003' WHERE Category = 'AyudanteCocina' AND PT_JobTypeId IS NULL;
UPDATE PT_Vacancies SET PT_JobTypeId = 'c1000004-0000-0000-0000-000000000004' WHERE Category = 'AyudanteBarra' AND PT_JobTypeId IS NULL;
UPDATE PT_Vacancies SET PT_JobTypeId = 'c2000001-0000-0000-0000-000000000001' WHERE Category = 'Cocinero' AND PT_JobTypeId IS NULL;
UPDATE PT_Vacancies SET PT_JobTypeId = 'c2000002-0000-0000-0000-000000000002' WHERE Category = 'Barman' AND PT_JobTypeId IS NULL;
UPDATE PT_Vacancies SET PT_JobTypeId = 'c2000003-0000-0000-0000-000000000003' WHERE Category = 'RecepcionistaHotel' AND PT_JobTypeId IS NULL;
UPDATE PT_Vacancies SET PT_JobTypeId = 'c3000001-0000-0000-0000-000000000001' WHERE Category = 'JefeSala' AND PT_JobTypeId IS NULL;
UPDATE PT_Vacancies SET PT_JobTypeId = 'c3000002-0000-0000-0000-000000000002' WHERE Category = 'ResponsableLocal' AND PT_JobTypeId IS NULL;

-- ===== Codigo promocional de ejemplo: 10% en vacantes de nivel Operativo =====
INSERT INTO PT_PromoCodes (Id, Code, Description, DiscountType, DiscountValue, PT_JobLevelId, PT_JobTypeId, ValidFrom, MaxUses, UsesCount, IsActive, CreatedAt, IsDeleted) VALUES
(UUID(), 'ALEJO26', '10% en vacantes de nivel Personal Operativo', 0, 10.00, 'b1111111-1111-1111-1111-111111111111', NULL, UTC_TIMESTAMP(), NULL, 0, 1, UTC_TIMESTAMP(), 0);
