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
            // Table PT_Plans already exists from a previous partial migration
            // Only insert seed data
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO PT_Plans (Id, Name, Description, Price, Currency, SortOrder, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted, DeletedAt, DeletedBy)
                VALUES
                    ('a1111111-1111-1111-1111-111111111111', 'Basic', 'Plan básico con funcionalidades esenciales para empezar.', 49.00, 'EUR', 1, 1, UTC_TIMESTAMP(), NULL, NULL, NULL, 0, NULL, NULL),
                    ('a2222222-2222-2222-2222-222222222222', 'Premium', 'Plan premium con herramientas avanzadas de gestión y soporte prioritario.', 99.00, 'EUR', 2, 1, UTC_TIMESTAMP(), NULL, NULL, NULL, 0, NULL, NULL),
                    ('a3333333-3333-3333-3333-333333333333', 'Platinum', 'Plan platinum con todas las funcionalidades, soporte dedicado y personalización total.', 199.00, 'EUR', 3, 1, UTC_TIMESTAMP(), NULL, NULL, NULL, 0, NULL, NULL);
            ");

            // Ensure index exists
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS IX_PT_Plans_IsActive_IsDeleted ON PT_Plans (IsActive, IsDeleted);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PT_Plans");
        }
    }
}
