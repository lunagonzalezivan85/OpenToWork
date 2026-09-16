using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class WarrantyReplacements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PT_WarrantyReplacements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_VacancyContractId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_VacancyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OriginalNegotiationId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    OriginalDeliveryId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsExclusion = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReplacementNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RequestedByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RequestedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReplacementNegotiationId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ReplacementDeliveryId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ChargeTrancheId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_PT_WarrantyReplacements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_WarrantyReplacements_PT_CandidateDeliveries_OriginalDeliv~",
                        column: x => x.OriginalDeliveryId,
                        principalTable: "PT_CandidateDeliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PT_WarrantyReplacements_PT_CandidateDeliveries_ReplacementDe~",
                        column: x => x.ReplacementDeliveryId,
                        principalTable: "PT_CandidateDeliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PT_WarrantyReplacements_PT_ContractPayments_ChargeTrancheId",
                        column: x => x.ChargeTrancheId,
                        principalTable: "PT_ContractPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PT_WarrantyReplacements_PT_Negotiations_OriginalNegotiationId",
                        column: x => x.OriginalNegotiationId,
                        principalTable: "PT_Negotiations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PT_WarrantyReplacements_PT_Negotiations_ReplacementNegotiati~",
                        column: x => x.ReplacementNegotiationId,
                        principalTable: "PT_Negotiations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PT_WarrantyReplacements_PT_Vacancies_PT_VacancyId",
                        column: x => x.PT_VacancyId,
                        principalTable: "PT_Vacancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_WarrantyReplacements_PT_VacancyContracts_PT_VacancyContra~",
                        column: x => x.PT_VacancyContractId,
                        principalTable: "PT_VacancyContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_WarrantyReplacements_SC_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PT_WarrantyReplacements_ChargeTrancheId",
                table: "PT_WarrantyReplacements",
                column: "ChargeTrancheId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_WarrantyReplacements_OriginalDeliveryId",
                table: "PT_WarrantyReplacements",
                column: "OriginalDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_WarrantyReplacements_OriginalNegotiationId",
                table: "PT_WarrantyReplacements",
                column: "OriginalNegotiationId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_WarrantyReplacements_PT_VacancyContractId",
                table: "PT_WarrantyReplacements",
                column: "PT_VacancyContractId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_WarrantyReplacements_PT_VacancyId_IsDeleted",
                table: "PT_WarrantyReplacements",
                columns: new[] { "PT_VacancyId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_WarrantyReplacements_ReplacementDeliveryId",
                table: "PT_WarrantyReplacements",
                column: "ReplacementDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_WarrantyReplacements_ReplacementNegotiationId",
                table: "PT_WarrantyReplacements",
                column: "ReplacementNegotiationId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_WarrantyReplacements_RequestedByUserId",
                table: "PT_WarrantyReplacements",
                column: "RequestedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_WarrantyReplacements");
        }
    }
}
