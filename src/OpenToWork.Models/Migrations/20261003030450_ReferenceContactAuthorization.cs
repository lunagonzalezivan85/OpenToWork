using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <summary>
    /// Traza de la declaracion del candidato al aportar una referencia: informo a esa persona y
    /// cuenta con su autorizacion para que Trato Directo la contacte. Recortada a mano: se quito
    /// el ruido de datos semilla (SY_WizardSteps, SY_DocumentTypes, PT_Plans) que EF regenera.
    /// </summary>
    public partial class ReferenceContactAuthorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContactAuthorizationAt",
                table: "PT_CandidateReferences",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactAuthorizationIp",
                table: "PT_CandidateReferences",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactAuthorizationAt",
                table: "PT_CandidateReferences");

            migrationBuilder.DropColumn(
                name: "ContactAuthorizationIp",
                table: "PT_CandidateReferences");
        }
    }
}
