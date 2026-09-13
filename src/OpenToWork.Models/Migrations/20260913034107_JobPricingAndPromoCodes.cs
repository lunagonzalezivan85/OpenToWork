using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class JobPricingAndPromoCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PT_JobTypeId",
                table: "PT_Vacancies",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "PT_ContractVacancies",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "PT_ContractVacancies",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice",
                table: "PT_ContractVacancies",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsManualOverride",
                table: "PT_ContractVacancies",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OverrideReason",
                table: "PT_ContractVacancies",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "PT_JobTypeId",
                table: "PT_ContractVacancies",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "PT_PromoCodeId",
                table: "PT_ContractVacancies",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "PromoCodeText",
                table: "PT_ContractVacancies",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_JobLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenceCoverageDays = table.Column<int>(type: "int", nullable: true),
                    WarrantyDays = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_PT_JobLevels", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_JobTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PT_JobLevelId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_PT_JobTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_JobTypes_PT_JobLevels_PT_JobLevelId",
                        column: x => x.PT_JobLevelId,
                        principalTable: "PT_JobLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_JobTypePrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_JobTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BasePrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "EUR")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
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
                    table.PrimaryKey("PK_PT_JobTypePrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_JobTypePrices_PT_JobTypes_PT_JobTypeId",
                        column: x => x.PT_JobTypeId,
                        principalTable: "PT_JobTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_PromoCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    PT_JobLevelId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PT_JobTypeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ValidFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MaxUses = table.Column<int>(type: "int", nullable: true),
                    UsesCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_PT_PromoCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_PromoCodes_PT_JobLevels_PT_JobLevelId",
                        column: x => x.PT_JobLevelId,
                        principalTable: "PT_JobLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PT_PromoCodes_PT_JobTypes_PT_JobTypeId",
                        column: x => x.PT_JobTypeId,
                        principalTable: "PT_JobTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_PromoCodeRedemptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_PromoCodeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_ContractVacancyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DiscountAmountApplied = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
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
                    table.PrimaryKey("PK_PT_PromoCodeRedemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_PromoCodeRedemptions_PT_ContractVacancies_PT_ContractVaca~",
                        column: x => x.PT_ContractVacancyId,
                        principalTable: "PT_ContractVacancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_PromoCodeRedemptions_PT_PromoCodes_PT_PromoCodeId",
                        column: x => x.PT_PromoCodeId,
                        principalTable: "PT_PromoCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PT_Vacancies_PT_JobTypeId",
                table: "PT_Vacancies",
                column: "PT_JobTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_ContractVacancies_PT_JobTypeId",
                table: "PT_ContractVacancies",
                column: "PT_JobTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_ContractVacancies_PT_PromoCodeId",
                table: "PT_ContractVacancies",
                column: "PT_PromoCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_JobLevels_Name_IsDeleted",
                table: "PT_JobLevels",
                columns: new[] { "Name", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_JobTypePrices_PT_JobTypeId_EffectiveTo_IsDeleted",
                table: "PT_JobTypePrices",
                columns: new[] { "PT_JobTypeId", "EffectiveTo", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_JobTypes_Name_IsDeleted",
                table: "PT_JobTypes",
                columns: new[] { "Name", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_JobTypes_PT_JobLevelId_IsDeleted",
                table: "PT_JobTypes",
                columns: new[] { "PT_JobLevelId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_PromoCodeRedemptions_PT_ContractVacancyId",
                table: "PT_PromoCodeRedemptions",
                column: "PT_ContractVacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_PromoCodeRedemptions_PT_PromoCodeId_IsDeleted",
                table: "PT_PromoCodeRedemptions",
                columns: new[] { "PT_PromoCodeId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_PromoCodes_Code_IsDeleted",
                table: "PT_PromoCodes",
                columns: new[] { "Code", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_PromoCodes_PT_JobLevelId",
                table: "PT_PromoCodes",
                column: "PT_JobLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_PromoCodes_PT_JobTypeId",
                table: "PT_PromoCodes",
                column: "PT_JobTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PT_ContractVacancies_PT_JobTypes_PT_JobTypeId",
                table: "PT_ContractVacancies",
                column: "PT_JobTypeId",
                principalTable: "PT_JobTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PT_ContractVacancies_PT_PromoCodes_PT_PromoCodeId",
                table: "PT_ContractVacancies",
                column: "PT_PromoCodeId",
                principalTable: "PT_PromoCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PT_Vacancies_PT_JobTypes_PT_JobTypeId",
                table: "PT_Vacancies",
                column: "PT_JobTypeId",
                principalTable: "PT_JobTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PT_ContractVacancies_PT_JobTypes_PT_JobTypeId",
                table: "PT_ContractVacancies");

            migrationBuilder.DropForeignKey(
                name: "FK_PT_ContractVacancies_PT_PromoCodes_PT_PromoCodeId",
                table: "PT_ContractVacancies");

            migrationBuilder.DropForeignKey(
                name: "FK_PT_Vacancies_PT_JobTypes_PT_JobTypeId",
                table: "PT_Vacancies");

            migrationBuilder.DropTable(
                name: "PT_JobTypePrices");

            migrationBuilder.DropTable(
                name: "PT_PromoCodeRedemptions");

            migrationBuilder.DropTable(
                name: "PT_PromoCodes");

            migrationBuilder.DropTable(
                name: "PT_JobTypes");

            migrationBuilder.DropTable(
                name: "PT_JobLevels");

            migrationBuilder.DropIndex(
                name: "IX_PT_Vacancies_PT_JobTypeId",
                table: "PT_Vacancies");

            migrationBuilder.DropIndex(
                name: "IX_PT_ContractVacancies_PT_JobTypeId",
                table: "PT_ContractVacancies");

            migrationBuilder.DropIndex(
                name: "IX_PT_ContractVacancies_PT_PromoCodeId",
                table: "PT_ContractVacancies");

            migrationBuilder.DropColumn(
                name: "PT_JobTypeId",
                table: "PT_Vacancies");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "PT_ContractVacancies");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "PT_ContractVacancies");

            migrationBuilder.DropColumn(
                name: "FinalPrice",
                table: "PT_ContractVacancies");

            migrationBuilder.DropColumn(
                name: "IsManualOverride",
                table: "PT_ContractVacancies");

            migrationBuilder.DropColumn(
                name: "OverrideReason",
                table: "PT_ContractVacancies");

            migrationBuilder.DropColumn(
                name: "PT_JobTypeId",
                table: "PT_ContractVacancies");

            migrationBuilder.DropColumn(
                name: "PT_PromoCodeId",
                table: "PT_ContractVacancies");

            migrationBuilder.DropColumn(
                name: "PromoCodeText",
                table: "PT_ContractVacancies");

        }
    }
}
