using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    /// <remarks>"Liberar candidato": fin manual de una colocacion (PT_CandidateDeliveries y
    /// PT_Negotiations). Recortada a mano: el scaffold arrastraba borrados/inserciones de
    /// SY_DocumentTypes/SY_WizardSteps y UpdateData de PT_Plans.CreatedAt (seed con
    /// Guid.NewGuid()/DateTime.UtcNow, ver Bitacora 21-Sep).</remarks>
    public partial class CandidatePlacementRelease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlacementEndNotes",
                table: "PT_Negotiations",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PlacementEndReason",
                table: "PT_Negotiations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlacementEndedAt",
                table: "PT_Negotiations",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlacementEndedByUserId",
                table: "PT_Negotiations",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "PlacementEndNotes",
                table: "PT_CandidateDeliveries",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PlacementEndReason",
                table: "PT_CandidateDeliveries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlacementEndedAt",
                table: "PT_CandidateDeliveries",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlacementEndedByUserId",
                table: "PT_CandidateDeliveries",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlacementEndNotes",
                table: "PT_Negotiations");

            migrationBuilder.DropColumn(
                name: "PlacementEndReason",
                table: "PT_Negotiations");

            migrationBuilder.DropColumn(
                name: "PlacementEndedAt",
                table: "PT_Negotiations");

            migrationBuilder.DropColumn(
                name: "PlacementEndedByUserId",
                table: "PT_Negotiations");

            migrationBuilder.DropColumn(
                name: "PlacementEndNotes",
                table: "PT_CandidateDeliveries");

            migrationBuilder.DropColumn(
                name: "PlacementEndReason",
                table: "PT_CandidateDeliveries");

            migrationBuilder.DropColumn(
                name: "PlacementEndedAt",
                table: "PT_CandidateDeliveries");

            migrationBuilder.DropColumn(
                name: "PlacementEndedByUserId",
                table: "PT_CandidateDeliveries");
        }
    }
}
