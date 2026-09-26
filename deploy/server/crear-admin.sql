-- Convierte en administrador (SuperAdmin) la cuenta que acabas de registrar en el portal.
-- 1. Registra la cuenta en https://tratodirecto.es/register con tu correo.
-- 2. Cambia el correo de abajo y ejecuta este archivo en MySQL:
--      mysql -uroot -p TratoDirectoDb < server\crear-admin.sql
-- 3. Entra en https://admin.tratodirecto.es con ese correo y contraseña.

SET @email = 'CAMBIAR@tratodirecto.es';

UPDATE SC_Users
SET PrimaryRole = 2,   -- Admin
    StaffRole = 0      -- SuperAdmin
WHERE Email = @email;

-- El registro del portal crea la cuenta como candidato: se retira ese perfil para que no aparezca
-- en las listas de candidatos del admin.
UPDATE PT_Candidates
SET IsDeleted = 1, DeletedAt = UTC_TIMESTAMP()
WHERE SCUserId = (SELECT Id FROM SC_Users WHERE Email = @email);

SELECT Email, PrimaryRole, StaffRole FROM SC_Users WHERE Email = @email;
