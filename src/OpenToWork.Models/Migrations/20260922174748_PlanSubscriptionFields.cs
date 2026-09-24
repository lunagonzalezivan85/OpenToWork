using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class PlanSubscriptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PlanExpiresAt",
                table: "PT_Companies",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlanTier",
                table: "PT_Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "PT_Companies",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionId",
                table: "PT_Companies",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "PlanExpiresAt",
                table: "PT_Candidates",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "PT_Candidates",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionId",
                table: "PT_Candidates",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Candidatos que ya tenian plan de pago antes de existir PlanExpiresAt: sin esto quedarian
            // con null y PlanCalculator.IsActive los trataria como vencidos (perderian la prioridad).
            migrationBuilder.Sql(
                "UPDATE PT_Candidates SET PlanExpiresAt = DATE_ADD(UTC_TIMESTAMP(6), INTERVAL 1 MONTH) " +
                "WHERE PlanTier <> 0 AND PlanExpiresAt IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanExpiresAt",
                table: "PT_Companies");

            migrationBuilder.DropColumn(
                name: "PlanTier",
                table: "PT_Companies");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "PT_Companies");

            migrationBuilder.DropColumn(
                name: "StripeSubscriptionId",
                table: "PT_Companies");

            migrationBuilder.DropColumn(
                name: "PlanExpiresAt",
                table: "PT_Candidates");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "PT_Candidates");

            migrationBuilder.DropColumn(
                name: "StripeSubscriptionId",
                table: "PT_Candidates");
        }
    }
}
