using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTA (Dsiezar, merge 07-Sep): la migracion original de Iluna asumia que PT_Plans ya
            // existia (creada en su entorno por una migracion previa que despues se descarto/aplano),
            // por lo que solo insertaba el seed. En una base nueva (o en la de otro desarrollador) la
            // tabla nunca se creaba y esta migracion fallaba con "Table 'pt_plans' doesn't exist".
            // Se agrega el CreateTable real (mismas columnas que CompanyCrmPipeline crea para las
            // demas tablas del CRM) antes del seed.
            migrationBuilder.CreateTable(
                name: "PT_Plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Currency = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_PT_Plans", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PT_Plans_IsActive_IsDeleted",
                table: "PT_Plans",
                columns: new[] { "IsActive", "IsDeleted" });

            migrationBuilder.Sql(@"
                INSERT IGNORE INTO PT_Plans (Id, Name, Description, Price, Currency, SortOrder, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted, DeletedAt, DeletedBy)
                VALUES
                    ('a1111111-1111-1111-1111-111111111111', 'Basic', 'Plan básico con funcionalidades esenciales para empezar.', 49.00, 'EUR', 1, 1, UTC_TIMESTAMP(), NULL, NULL, NULL, 0, NULL, NULL),
                    ('a2222222-2222-2222-2222-222222222222', 'Premium', 'Plan premium con herramientas avanzadas de gestión y soporte prioritario.', 99.00, 'EUR', 2, 1, UTC_TIMESTAMP(), NULL, NULL, NULL, 0, NULL, NULL),
                    ('a3333333-3333-3333-3333-333333333333', 'Platinum', 'Plan platinum con todas las funcionalidades, soporte dedicado y personalización total.', 199.00, 'EUR', 3, 1, UTC_TIMESTAMP(), NULL, NULL, NULL, 0, NULL, NULL);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PT_Plans");
        }
    }
}
