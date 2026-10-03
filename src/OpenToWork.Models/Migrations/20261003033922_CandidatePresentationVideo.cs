using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <summary>
    /// Video de presentacion del candidato (1 minuto, solo lo ve el equipo de Trato Directo).
    /// Recortada a mano: se quito el ruido de datos semilla (SY_WizardSteps, SY_DocumentTypes, PT_Plans)
    /// que EF regenera.
    /// </summary>
    public partial class CandidatePresentationVideo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PresentationVideoUploadedAt",
                table: "PT_Candidates",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PresentationVideoUrl",
                table: "PT_Candidates",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PresentationVideoUploadedAt",
                table: "PT_Candidates");

            migrationBuilder.DropColumn(
                name: "PresentationVideoUrl",
                table: "PT_Candidates");
        }
    }
}
