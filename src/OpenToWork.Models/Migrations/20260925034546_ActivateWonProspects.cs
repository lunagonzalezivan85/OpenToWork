using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    /// <remarks>Solo datos. Desde el 25-Sep, llegar a Cerrado Ganado pasa la empresa de Prospecto a
    /// Activa (CompanyCrmService.MoveStageAsync); esto corrige las que ya estaban ganadas y seguian como
    /// Prospecto. Recortada a mano del ruido de seed (ver Bitacora 21-Sep).</remarks>
    public partial class ActivateWonProspects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CompanyStatus: Activa = 0, Prospecto = 2. CompanyPipelineStage.CerradoGanado = 5.
            migrationBuilder.Sql(
                "UPDATE PT_Companies c " +
                "JOIN PT_CompanyPipelines p ON p.PT_CompanyId = c.Id AND p.IsDeleted = 0 AND p.IsDismissed = 0 " +
                "SET c.Status = 0, c.UpdatedAt = UTC_TIMESTAMP(6) " +
                "WHERE c.IsDeleted = 0 AND c.Status = 2 AND p.CurrentStage = 5;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sin vuelta atras: no se distingue que empresas activo esta migracion de las activadas a mano.
        }
    }
}
