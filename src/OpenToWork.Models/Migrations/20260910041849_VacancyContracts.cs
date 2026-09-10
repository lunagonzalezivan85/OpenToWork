using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class VacancyContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PT_VacancyContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_VacancyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CompanyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ContractNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ScopeServices = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetCandidates = table.Column<int>(type: "int", nullable: true),
                    JobTypeCategory = table.Column<int>(type: "int", nullable: false),
                    TargetCoverageDays = table.Column<int>(type: "int", nullable: true),
                    WarrantyDays = table.Column<int>(type: "int", nullable: true),
                    FeeAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "EUR")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FeeApplicationType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PaymentOpeningPct = table.Column<decimal>(type: "decimal(65,30)", nullable: false, defaultValue: 30m),
                    PaymentValidationPct = table.Column<decimal>(type: "decimal(65,30)", nullable: false, defaultValue: 50m),
                    PaymentConsolidationPct = table.Column<decimal>(type: "decimal(65,30)", nullable: false, defaultValue: 20m),
                    FeeExceptions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AcceptedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RejectionReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
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
                    table.PrimaryKey("PK_PT_VacancyContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_VacancyContracts_PT_Companies_PT_CompanyId",
                        column: x => x.PT_CompanyId,
                        principalTable: "PT_Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_VacancyContracts_PT_Vacancies_PT_VacancyId",
                        column: x => x.PT_VacancyId,
                        principalTable: "PT_Vacancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PT_VacancyContracts_PT_CompanyId_IsDeleted",
                table: "PT_VacancyContracts",
                columns: new[] { "PT_CompanyId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_VacancyContracts_PT_VacancyId_IsDeleted",
                table: "PT_VacancyContracts",
                columns: new[] { "PT_VacancyId", "IsDeleted" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_VacancyContracts");
        }
    }
}
