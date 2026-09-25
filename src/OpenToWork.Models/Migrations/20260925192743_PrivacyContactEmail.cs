using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    /// <remarks>Solo datos: clave de configuracion company_privacy_email (correo que publica la Politica de
    /// Privacidad del portal; editable en admin > Datos de la Empresa, que solo actualiza claves existentes).
    /// Recortada a mano del ruido de seed (ver Bitacora 21-Sep).</remarks>
    public partial class PrivacyContactEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "INSERT INTO SY_SystemConfig (Id, `Key`, Value, Category, Description, IsActive, CreatedAt, IsDeleted) " +
                "SELECT UUID(), 'company_privacy_email', '', 'CompanyIdentity', " +
                "'Correo para ejercer derechos de proteccion de datos (Politica de Privacidad)', 1, UTC_TIMESTAMP(6), 0 " +
                "FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM SY_SystemConfig WHERE `Key` = 'company_privacy_email');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM SY_SystemConfig WHERE `Key` = 'company_privacy_email';");
        }
    }
}
