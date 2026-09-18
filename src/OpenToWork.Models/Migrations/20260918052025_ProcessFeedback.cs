using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class ProcessFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeedbackComments",
                table: "PT_Negotiations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "FeedbackRating",
                table: "PT_Negotiations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FeedbackRecordedAt",
                table: "PT_Negotiations",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FeedbackRecordedByUserId",
                table: "PT_Negotiations",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "FeedbackComments",
                table: "PT_CandidateDeliveries",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "FeedbackRating",
                table: "PT_CandidateDeliveries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FeedbackRecordedAt",
                table: "PT_CandidateDeliveries",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FeedbackRecordedByUserId",
                table: "PT_CandidateDeliveries",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_PT_Negotiations_FeedbackRecordedByUserId",
                table: "PT_Negotiations",
                column: "FeedbackRecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateDeliveries_FeedbackRecordedByUserId",
                table: "PT_CandidateDeliveries",
                column: "FeedbackRecordedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PT_CandidateDeliveries_SC_Users_FeedbackRecordedByUserId",
                table: "PT_CandidateDeliveries",
                column: "FeedbackRecordedByUserId",
                principalTable: "SC_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PT_Negotiations_SC_Users_FeedbackRecordedByUserId",
                table: "PT_Negotiations",
                column: "FeedbackRecordedByUserId",
                principalTable: "SC_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PT_CandidateDeliveries_SC_Users_FeedbackRecordedByUserId",
                table: "PT_CandidateDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_PT_Negotiations_SC_Users_FeedbackRecordedByUserId",
                table: "PT_Negotiations");

            migrationBuilder.DropIndex(
                name: "IX_PT_Negotiations_FeedbackRecordedByUserId",
                table: "PT_Negotiations");

            migrationBuilder.DropIndex(
                name: "IX_PT_CandidateDeliveries_FeedbackRecordedByUserId",
                table: "PT_CandidateDeliveries");

            migrationBuilder.DropColumn(
                name: "FeedbackComments",
                table: "PT_Negotiations");

            migrationBuilder.DropColumn(
                name: "FeedbackRating",
                table: "PT_Negotiations");

            migrationBuilder.DropColumn(
                name: "FeedbackRecordedAt",
                table: "PT_Negotiations");

            migrationBuilder.DropColumn(
                name: "FeedbackRecordedByUserId",
                table: "PT_Negotiations");

            migrationBuilder.DropColumn(
                name: "FeedbackComments",
                table: "PT_CandidateDeliveries");

            migrationBuilder.DropColumn(
                name: "FeedbackRating",
                table: "PT_CandidateDeliveries");

            migrationBuilder.DropColumn(
                name: "FeedbackRecordedAt",
                table: "PT_CandidateDeliveries");

            migrationBuilder.DropColumn(
                name: "FeedbackRecordedByUserId",
                table: "PT_CandidateDeliveries");
        }
    }
}
