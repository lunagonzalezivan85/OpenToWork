using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class PlanFeaturesAndFeatured : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Features",
                table: "PT_Plans",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "PT_Plans",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Features",
                table: "PT_Plans");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "PT_Plans");
        }
    }
}
