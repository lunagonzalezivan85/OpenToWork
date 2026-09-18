-- Seed de SY_SystemConfig, categoria CompanyIdentity (18-Sep-2026, Dsiezar).
--
-- Datos de identidad legal de Trato Directo usados en el Contrato Marco
-- (VacancyContractDocument.razor). Antes fijos en el Razor (deploy para cambiar
-- cualquiera de estos valores), ahora editables desde /settings/company-profile
-- sin tocar codigo. Los valores de abajo son los mismos que estaban hardcodeados,
-- para que no cambie nada hasta que un SuperAdmin los edite deliberadamente.

INSERT INTO SY_SystemConfig (Id, `Key`, Value, Category, Description, IsActive, CreatedAt, IsDeleted) VALUES
(UUID(), 'company_legal_name', 'TRATO DIRECTO HUMAN SERVICES, S.L.', 'CompanyIdentity', 'Razon social de Trato Directo', 1, UTC_TIMESTAMP(), 0),
(UUID(), 'company_tax_id', '1235567', 'CompanyIdentity', 'CIF de Trato Directo', 1, UTC_TIMESTAMP(), 0),
(UUID(), 'company_address', 'Calle de Lugo 15, Alcobendas', 'CompanyIdentity', 'Domicilio social de Trato Directo', 1, UTC_TIMESTAMP(), 0),
(UUID(), 'company_mercantile_registry', 'Registro Mercantil de Madrid, Tomo 1, Folio 1, Hoja 1', 'CompanyIdentity', 'Inscripcion registral de Trato Directo', 1, UTC_TIMESTAMP(), 0),
(UUID(), 'legal_rep_name', 'Luis Alejandro Velasquez', 'CompanyIdentity', 'Nombre del representante legal', 1, UTC_TIMESTAMP(), 0),
(UUID(), 'legal_rep_position', 'Representante Legal', 'CompanyIdentity', 'Cargo del representante legal', 1, UTC_TIMESTAMP(), 0),
(UUID(), 'legal_rep_dni', '3212312345', 'CompanyIdentity', 'DNI/NIE del representante legal', 1, UTC_TIMESTAMP(), 0),
(UUID(), 'jurisdiction_city', 'Madrid', 'CompanyIdentity', 'Ciudad de firma y jurisdiccion del contrato', 1, UTC_TIMESTAMP(), 0);
