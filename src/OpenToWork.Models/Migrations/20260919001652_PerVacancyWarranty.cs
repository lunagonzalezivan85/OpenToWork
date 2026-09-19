using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class PerVacancyWarranty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarrantyDays",
                table: "PT_ContractVacancies",
                type: "int",
                nullable: true);

            // Backfill: las lineas de contratos ya existentes heredan el valor que tenia el
            // contrato (compartido) hasta ahora, para no perder el dato ya cargado. De aca en
            // adelante cada linea tiene su propia garantia (ver AdminContractService).
            migrationBuilder.Sql(@"
                UPDATE PT_ContractVacancies cv
                INNER JOIN PT_VacancyContracts c ON cv.PT_ContractId = c.Id
                SET cv.WarrantyDays = c.WarrantyDays
                WHERE cv.WarrantyDays IS NULL AND c.WarrantyDays IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WarrantyDays",
                table: "PT_ContractVacancies");
        }
    }
}
