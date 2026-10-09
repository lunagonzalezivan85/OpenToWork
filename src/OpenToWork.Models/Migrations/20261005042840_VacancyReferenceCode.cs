using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class VacancyReferenceCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferenceCode",
                table: "PT_Vacancies",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            // Revision Dsiezar 9-Oct: aqui habia DELETE FROM SY_DocumentTypes / SY_WizardSteps y la
            // reinsercion de los seeds. SY_DocumentTypes tiene ON DELETE CASCADE hacia
            // PT_RecruitmentDocuments: en produccion borraba todos los documentos de seleccion. Se
            // quitan los DeleteData/InsertData/UpdateData de seeds (incluidos los de PT_Plans): sin
            // reinsercion ya no hay choque con el indice unico por nombre.

            // Backfill antes del indice unico: el default "" duplicado en filas existentes
            // violaria la unicidad. UUID() genera un valor distinto por fila.
            migrationBuilder.Sql(
                "UPDATE PT_Vacancies SET ReferenceCode = CONCAT('TD-', UPPER(SUBSTRING(REPLACE(UUID(), '-', ''), 1, 8))) WHERE ReferenceCode = ''");

            migrationBuilder.CreateIndex(
                name: "IX_PT_Vacancies_ReferenceCode",
                table: "PT_Vacancies",
                column: "ReferenceCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PT_Vacancies_ReferenceCode",
                table: "PT_Vacancies");

            migrationBuilder.DropColumn(
                name: "ReferenceCode",
                table: "PT_Vacancies");
        }
    }
}
