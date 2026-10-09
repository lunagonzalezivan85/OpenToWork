using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class CandidateVisibilityConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "VisibilityConsentAt",
                table: "PT_Candidates",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VisibilityConsentRevokedAt",
                table: "PT_Candidates",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisibilityConsentVersion",
                table: "PT_Candidates",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisibilityConsentAt",
                table: "PT_Candidates");

            migrationBuilder.DropColumn(
                name: "VisibilityConsentRevokedAt",
                table: "PT_Candidates");

            migrationBuilder.DropColumn(
                name: "VisibilityConsentVersion",
                table: "PT_Candidates");
        }
    }
}
