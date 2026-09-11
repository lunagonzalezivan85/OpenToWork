using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class FixOrphanContractVacancyColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La migracion 20260911002220_ContractVacancyOneToMany introdujo la
            // tabla puente PT_ContractVacancies (1 contrato : N vacantes) pero
            // nunca elimino la columna PT_VacancyId (NOT NULL + FK) que quedo de
            // el diseno original 1:1 en PT_VacancyContracts. Como el entity
            // PTVacancyContract ya no tiene esa propiedad, EF inserta sin ella y
            // MySQL rechaza el INSERT por la FK -> "Generar contrato" siempre
            // fallaba con "No se pudo guardar el contrato" (todo contrato nuevo).
            migrationBuilder.DropForeignKey(
                name: "FK_PT_VacancyContracts_PT_Vacancies_PT_VacancyId",
                table: "PT_VacancyContracts");

            migrationBuilder.DropIndex(
                name: "IX_PT_VacancyContracts_PT_VacancyId_IsDeleted",
                table: "PT_VacancyContracts");

            migrationBuilder.DropColumn(
                name: "PT_VacancyId",
                table: "PT_VacancyContracts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PT_VacancyId",
                table: "PT_VacancyContracts",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_PT_VacancyContracts_PT_VacancyId_IsDeleted",
                table: "PT_VacancyContracts",
                columns: new[] { "PT_VacancyId", "IsDeleted" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PT_VacancyContracts_PT_Vacancies_PT_VacancyId",
                table: "PT_VacancyContracts",
                column: "PT_VacancyId",
                principalTable: "PT_Vacancies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
