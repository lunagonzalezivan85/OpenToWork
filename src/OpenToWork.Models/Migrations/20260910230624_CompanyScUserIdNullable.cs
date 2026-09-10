using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class CompanyScUserIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PT_Companies.SCUserId pasa a ser nullable: una empresa "prospecto"
            // captada desde el CRM administrativo no tiene cuenta de usuario asociada.
            // El indice unico IX_PT_Companies_SCUserId se mantiene: MySQL permite
            // multiples filas con NULL en un indice unico, asi que los prospectos
            // (SCUserId = NULL) no colisionan entre si.
            migrationBuilder.AlterColumn<Guid>(
                name: "SCUserId",
                table: "PT_Companies",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SCUserId",
                table: "PT_Companies",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }
    }
}
