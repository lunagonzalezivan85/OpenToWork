using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class CandidateReferenceToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "PT_CandidateReferences",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiresAt",
                table: "PT_CandidateReferences",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "PT_CandidateReferences",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateReferences_TokenHash",
                table: "PT_CandidateReferences",
                column: "TokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PT_CandidateReferences_TokenHash",
                table: "PT_CandidateReferences");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "PT_CandidateReferences");

            migrationBuilder.DropColumn(
                name: "TokenExpiresAt",
                table: "PT_CandidateReferences");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "PT_CandidateReferences");
        }
    }
}
