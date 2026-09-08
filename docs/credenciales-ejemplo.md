# Credenciales de ejemplo — OpenToWork (seed Hostelería)

> BD recreada el 08-Sep-2026 con datos de hostelería. Script: `docs/seed-hosteleria.sql`

## Admin (portal AdminWEB — http://localhost:5101)

| Email | Password | Rol |
|---|---|---|
| `admin@opentowork.com` | `Admin123!` | Admin (TD) |

## Empresas (portal WEB — password: `Empresa123!`)

| Email | Password | Empresa | Ciudad |
|---|---|---|---|
| `rrhh@hotelsolcaribe.com` | `Empresa123!` | Hotel Sol Caribe (4★, verificada) | Cartagena, Colombia |
| `rrhh@lapaella.com` | `Empresa123!` | Grupo La Paella (restaurantes) | Madrid, España |
| `rrhh@cateringdelmar.com` | `Empresa123!` | Catering Del Mar (eventos) | Barcelona, España |

## Candidatos (portal WEB — password: `Empresa123!`)

| Email | Password | Perfil | Ciudad |
|---|---|---|---|
| `ana.martinez@gmail.com` | `Empresa123!` | Chef de Parte, 5 años | Madrid |
| `luis.fernandez@hotmail.com` | `Empresa123!` | Recepcionista hotelero bilingüe, 4 años | Cartagena |
| `sofia.torres@outlook.com` | `Empresa123!` | Camarera / Sumiller, 6 años | Barcelona |
| `javier.moreno@outlook.com` | `Empresa123!` | Cocinero, 3 años | Cartagena |

## Datos de prueba incluidos

- **8 vacantes permanentes** (6 activas, 1 draft, 1 cerrada): Chef de Parte, Recepcionista Bilingüe, Gobernante/a, Camarero/a de Sala, Sous Chef, Camarero/a de Eventos, Pastelero/a, Jefe de Sala
- **2 vacantes temporales**: Extra de Sala (temporada alta), Cocinero/a de refuerzo eventos
- **15 skills** de hostelería (HACCP, maridaje, PMS Opera/Sihot, coctelería, etc.)
- **5 postulaciones** con cartas de presentación
- Experiencias laborales y formación académica para los 4 candidatos

## Notas

- Todos los hashes de password son BCrypt válidos (generados vía `POST /api/auth/register`).
- El admin se crea aparte del seed: registrar vía API y luego `UPDATE SC_Users SET PrimaryRole = 2 WHERE Email = 'admin@opentowork.com';`
- Para regenerar el seed: aplicar migraciones y ejecutar `docs/seed-hosteleria.sql` (el admin se crea por API).
