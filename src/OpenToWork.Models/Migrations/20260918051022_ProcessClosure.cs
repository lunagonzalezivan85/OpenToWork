using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class ProcessClosure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessClosedAt",
                table: "PT_Negotiations",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProcessClosedByUserId",
                table: "PT_Negotiations",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "ProcessClosureNotes",
                table: "PT_Negotiations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessClosedAt",
                table: "PT_CandidateDeliveries",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProcessClosedByUserId",
                table: "PT_CandidateDeliveries",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "ProcessClosureNotes",
                table: "PT_CandidateDeliveries",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PT_Negotiations_ProcessClosedByUserId",
                table: "PT_Negotiations",
                column: "ProcessClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateDeliveries_ProcessClosedByUserId",
                table: "PT_CandidateDeliveries",
                column: "ProcessClosedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PT_CandidateDeliveries_SC_Users_ProcessClosedByUserId",
                table: "PT_CandidateDeliveries",
                column: "ProcessClosedByUserId",
                principalTable: "SC_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PT_Negotiations_SC_Users_ProcessClosedByUserId",
                table: "PT_Negotiations",
                column: "ProcessClosedByUserId",
                principalTable: "SC_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PT_CandidateDeliveries_SC_Users_ProcessClosedByUserId",
                table: "PT_CandidateDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_PT_Negotiations_SC_Users_ProcessClosedByUserId",
                table: "PT_Negotiations");

            migrationBuilder.DropIndex(
                name: "IX_PT_Negotiations_ProcessClosedByUserId",
                table: "PT_Negotiations");

            migrationBuilder.DropIndex(
                name: "IX_PT_CandidateDeliveries_ProcessClosedByUserId",
                table: "PT_CandidateDeliveries");

            migrationBuilder.DropColumn(
                name: "ProcessClosedAt",
                table: "PT_Negotiations");

            migrationBuilder.DropColumn(
                name: "ProcessClosedByUserId",
                table: "PT_Negotiations");

            migrationBuilder.DropColumn(
                name: "ProcessClosureNotes",
                table: "PT_Negotiations");

            migrationBuilder.DropColumn(
                name: "ProcessClosedAt",
                table: "PT_CandidateDeliveries");

            migrationBuilder.DropColumn(
                name: "ProcessClosedByUserId",
                table: "PT_CandidateDeliveries");

            migrationBuilder.DropColumn(
                name: "ProcessClosureNotes",
                table: "PT_CandidateDeliveries");
        }
    }
}
