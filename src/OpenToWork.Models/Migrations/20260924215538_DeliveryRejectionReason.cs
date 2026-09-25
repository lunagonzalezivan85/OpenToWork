using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    /// <remarks>Motivo estructurado del descarte de una entrega (DeliveryRejectionReason), para el
    /// historial del candidato y la deteccion de candidatos "quemados". Recortada a mano: el scaffold
    /// arrastraba el ruido de seed de SY_DocumentTypes/SY_WizardSteps/PT_Plans (ver Bitacora 21-Sep).</remarks>
    public partial class DeliveryRejectionReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RejectionReason",
                table: "PT_CandidateDeliveries",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "PT_CandidateDeliveries");
        }
    }
}
