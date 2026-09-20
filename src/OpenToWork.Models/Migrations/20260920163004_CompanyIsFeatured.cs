using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class CompanyIsFeatured : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotente: la columna puede existir ya si una corrida parcial la creo
            migrationBuilder.Sql(@"
                SET @col_exists := (
                    SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'PT_Companies'
                      AND COLUMN_NAME = 'IsFeatured'
                );
                SET @sql := IF(@col_exists = 0,
                    'ALTER TABLE PT_Companies ADD COLUMN IsFeatured tinyint(1) NOT NULL DEFAULT 0',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "PT_Companies");
        }
    }
}
