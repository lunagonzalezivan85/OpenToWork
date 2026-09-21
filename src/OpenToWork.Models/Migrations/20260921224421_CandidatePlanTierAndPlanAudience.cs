using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class CandidatePlanTierAndPlanAudience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Audience",
                table: "PT_Plans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlanTier",
                table: "PT_Candidates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "PT_Plans",
                columns: new[] { "Id", "Audience", "CreatedAt", "CreatedBy", "Currency", "DeletedAt", "DeletedBy", "Description", "Features", "IsActive", "IsDeleted", "IsFeatured", "Name", "Price", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("b1111111-1111-1111-1111-111111111111"), 1, new DateTime(2026, 9, 21, 22, 44, 20, 661, DateTimeKind.Utc), null, "EUR", null, null, "Lo que ya tiene cualquier candidato al registrarse.", "Postulación ilimitada a vacantes\nPerfil profesional visible para empresas\nMensajería con empresas", true, false, false, "Free", 0.00m, 1, null, null },
                    { new Guid("b2222222-2222-2222-2222-222222222222"), 1, new DateTime(2026, 9, 21, 22, 44, 20, 661, DateTimeKind.Utc), null, "EUR", null, null, "Más visibilidad frente a las empresas.", "Todo lo del plan Free\nMayor prioridad en el matching\nVerificación por Trato Directo", true, false, true, "Basic", 5.99m, 2, null, null },
                    { new Guid("b3333333-3333-3333-3333-333333333333"), 1, new DateTime(2026, 9, 21, 22, 44, 20, 661, DateTimeKind.Utc), null, "EUR", null, null, "Acompañamiento experto para destacar.", "Todo lo del plan Basic\nAsesoría en construcción de CV con un especialista", true, false, false, "Premium", 9.99m, 3, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DropColumn(
                name: "Audience",
                table: "PT_Plans");

            migrationBuilder.DropColumn(
                name: "PlanTier",
                table: "PT_Candidates");
        }
    }
}
