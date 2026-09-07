using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class CompanyCrmPipeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Create PT_CompanyPipelines ---
            migrationBuilder.CreateTable(
                name: "PT_CompanyPipelines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CompanyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CurrentStage = table.Column<int>(type: "int", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AssignedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    StageEnteredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDismissed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DismissalReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DismissedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DismissedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_PT_CompanyPipelines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_CompanyPipelines_PT_Companies_PT_CompanyId",
                        column: x => x.PT_CompanyId,
                        principalTable: "PT_Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_CompanyPipelines_SC_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PT_CompanyPipelines_SC_Users_DismissedByUserId",
                        column: x => x.DismissedByUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // --- Create PT_CompanyStageLogs ---
            migrationBuilder.CreateTable(
                name: "PT_CompanyStageLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CompanyPipelineId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FromStage = table.Column<int>(type: "int", nullable: false),
                    ToStage = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
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
                    table.PrimaryKey("PK_PT_CompanyStageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_CompanyStageLogs_PT_CompanyPipelines_PT_CompanyPipelineId",
                        column: x => x.PT_CompanyPipelineId,
                        principalTable: "PT_CompanyPipelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_CompanyStageLogs_SC_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // --- Indexes ---
            migrationBuilder.CreateIndex(
                name: "IX_PT_CompanyPipelines_AssignedToUserId_IsDeleted",
                table: "PT_CompanyPipelines",
                columns: new[] { "AssignedToUserId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CompanyPipelines_CurrentStage_IsDeleted",
                table: "PT_CompanyPipelines",
                columns: new[] { "CurrentStage", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CompanyPipelines_DismissedByUserId",
                table: "PT_CompanyPipelines",
                column: "DismissedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_CompanyPipelines_IsDismissed_IsDeleted",
                table: "PT_CompanyPipelines",
                columns: new[] { "IsDismissed", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CompanyPipelines_PT_CompanyId_IsDeleted",
                table: "PT_CompanyPipelines",
                columns: new[] { "PT_CompanyId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CompanyStageLogs_ChangedByUserId",
                table: "PT_CompanyStageLogs",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_CompanyStageLogs_PT_CompanyPipelineId_IsDeleted",
                table: "PT_CompanyStageLogs",
                columns: new[] { "PT_CompanyPipelineId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_CompanyStageLogs");

            migrationBuilder.DropTable(
                name: "PT_CompanyPipelines");
        }
    }
}
