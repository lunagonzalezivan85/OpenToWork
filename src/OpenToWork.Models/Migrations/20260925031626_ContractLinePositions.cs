using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    /// <remarks>Posiciones y total por linea de contrato (tarifa por posicion = precio x posiciones,
    /// clausula 6.2). Las lineas existentes quedan con 1 posicion y LineTotal = FinalPrice: se respeta
    /// el importe historico de contratos ya enviados/aceptados (y de sus tramos de pago); los que haya
    /// que corregir se reabren como nueva version. Recortada a mano del ruido de seed (ver Bitacora 21-Sep).</remarks>
    public partial class ContractLinePositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LineTotal",
                table: "PT_ContractVacancies",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Positions",
                table: "PT_ContractVacancies",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("UPDATE PT_ContractVacancies SET LineTotal = FinalPrice WHERE LineTotal IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LineTotal",
                table: "PT_ContractVacancies");

            migrationBuilder.DropColumn(
                name: "Positions",
                table: "PT_ContractVacancies");
        }
    }
}
