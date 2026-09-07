using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTA (Dsiezar, QA 07-Sep): PTCompany.cs ya tenia estas 5 propiedades (LegalName,
            // TaxId, ContactName, ContactPosition, Status) desde el commit del CRM de Iluna, pero
            // ninguna migracion las agregaba realmente a PT_Companies - CompanyCrmPipeline.cs solo
            // crea las tablas nuevas, no altera PT_Companies. Encontrado en QA porque
            // GetCompaniesAsync/GetPipelineAsync tiraban 500 (MySqlException: Unknown column
            // 'p.LegalName'/'p0.ContactName' in 'field list').
            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "PT_Companies",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContactPosition",
                table: "PT_Companies",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                table: "PT_Companies",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PT_Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "PT_Companies",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PT_Companies_Status_IsDeleted",
                table: "PT_Companies",
                columns: new[] { "Status", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PT_Companies_Status_IsDeleted",
                table: "PT_Companies");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "PT_Companies");

            migrationBuilder.DropColumn(
                name: "ContactPosition",
                table: "PT_Companies");

            migrationBuilder.DropColumn(
                name: "LegalName",
                table: "PT_Companies");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PT_Companies");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "PT_Companies");
        }
    }
}
