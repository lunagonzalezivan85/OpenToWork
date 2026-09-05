using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class StaffRolesAndNegotiations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StaffRole",
                table: "SC_Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PT_Negotiations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_VacancyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedStaffId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PresentedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WinningApplicationId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_PT_Negotiations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_Negotiations_PT_Applications_WinningApplicationId",
                        column: x => x.WinningApplicationId,
                        principalTable: "PT_Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PT_Negotiations_PT_Vacancies_PT_VacancyId",
                        column: x => x.PT_VacancyId,
                        principalTable: "PT_Vacancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_Negotiations_SC_Users_AssignedStaffId",
                        column: x => x.AssignedStaffId,
                        principalTable: "SC_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_NegotiationCandidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_NegotiationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_ApplicationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_PT_NegotiationCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_NegotiationCandidates_PT_Applications_PT_ApplicationId",
                        column: x => x.PT_ApplicationId,
                        principalTable: "PT_Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_NegotiationCandidates_PT_Negotiations_PT_NegotiationId",
                        column: x => x.PT_NegotiationId,
                        principalTable: "PT_Negotiations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PT_NegotiationCandidates_PT_ApplicationId",
                table: "PT_NegotiationCandidates",
                column: "PT_ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_NegotiationCandidates_PT_NegotiationId_PT_ApplicationId_I~",
                table: "PT_NegotiationCandidates",
                columns: new[] { "PT_NegotiationId", "PT_ApplicationId", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_Negotiations_AssignedStaffId",
                table: "PT_Negotiations",
                column: "AssignedStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_Negotiations_PT_VacancyId_IsDeleted",
                table: "PT_Negotiations",
                columns: new[] { "PT_VacancyId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_Negotiations_Status_IsDeleted",
                table: "PT_Negotiations",
                columns: new[] { "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_Negotiations_WinningApplicationId",
                table: "PT_Negotiations",
                column: "WinningApplicationId");

            migrationBuilder.Sql("UPDATE SC_Users SET StaffRole = 0 WHERE PrimaryRole = 2;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_NegotiationCandidates");

            migrationBuilder.DropTable(
                name: "PT_Negotiations");

            migrationBuilder.DropColumn(
                name: "StaffRole",
                table: "SC_Users");
        }
    }
}
