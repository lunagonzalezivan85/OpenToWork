using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class CandidateDeliveries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PT_CandidateDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CandidateRecruitmentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CandidateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_VacancyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CompanyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DeliveredByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DeliveredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminNote = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompanyFeedback = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ViewedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_CandidateDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_CandidateDeliveries_PT_CandidateRecruitments_PT_Candidate~",
                        column: x => x.PT_CandidateRecruitmentId,
                        principalTable: "PT_CandidateRecruitments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_CandidateDeliveries_PT_Candidates_PT_CandidateId",
                        column: x => x.PT_CandidateId,
                        principalTable: "PT_Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_CandidateDeliveries_PT_Companies_PT_CompanyId",
                        column: x => x.PT_CompanyId,
                        principalTable: "PT_Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_CandidateDeliveries_PT_Vacancies_PT_VacancyId",
                        column: x => x.PT_VacancyId,
                        principalTable: "PT_Vacancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_CandidateDeliveries_SC_Users_DeliveredByUserId",
                        column: x => x.DeliveredByUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateDeliveries_DeliveredByUserId",
                table: "PT_CandidateDeliveries",
                column: "DeliveredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateDeliveries_PT_CandidateId",
                table: "PT_CandidateDeliveries",
                column: "PT_CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateDeliveries_PT_CandidateRecruitmentId_IsDeleted",
                table: "PT_CandidateDeliveries",
                columns: new[] { "PT_CandidateRecruitmentId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateDeliveries_PT_CompanyId_IsDeleted",
                table: "PT_CandidateDeliveries",
                columns: new[] { "PT_CompanyId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateDeliveries_PT_VacancyId_IsDeleted",
                table: "PT_CandidateDeliveries",
                columns: new[] { "PT_VacancyId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateDeliveries_Status_IsDeleted",
                table: "PT_CandidateDeliveries",
                columns: new[] { "Status", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_CandidateDeliveries");
        }
    }
}
