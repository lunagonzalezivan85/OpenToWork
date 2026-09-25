using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    /// <remarks>Solo datos, sin cambios de esquema. ExperienceLevel paso de la escala
    /// Entry/Junior/Mid/Senior/Lead (min. 0/1/3/5/8 anos) a rangos de anos (Sin experiencia /
    /// Menos de 1 / 1-3 / 3-5 / Mas de 5, pedido de Darwin 24-Sep). Se traduce cada vacante al rango
    /// que conserva su significado: Junior(1+) -> 1-3, Mid(3+) -> 3-5, Senior(5+) y Lead(8+) -> Mas de 5.
    /// El scaffold arrastraba el ruido de seed de SY_DocumentTypes/SY_WizardSteps/PT_Plans (recortado).</remarks>
    public partial class ExperienceLevelYearRanges : Migration
    {
        private const string Forward =
            "SET ExperienceLevel = CASE ExperienceLevel WHEN 1 THEN 2 WHEN 2 THEN 3 WHEN 3 THEN 4 WHEN 4 THEN 4 ELSE ExperienceLevel END " +
            "WHERE ExperienceLevel IS NOT NULL;";

        // Inverso aproximado: "Mas de 5" vuelve a Senior (no se puede distinguir un Lead original).
        // "Menos de 1 ano" (1) no existia antes: se deja como Junior.
        private const string Backward =
            "SET ExperienceLevel = CASE ExperienceLevel WHEN 2 THEN 1 WHEN 3 THEN 2 WHEN 4 THEN 3 ELSE ExperienceLevel END " +
            "WHERE ExperienceLevel IS NOT NULL;";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE PT_Vacancies " + Forward);
            migrationBuilder.Sql("UPDATE PT_TempVacancies " + Forward);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE PT_Vacancies " + Backward);
            migrationBuilder.Sql("UPDATE PT_TempVacancies " + Backward);
        }
    }
}
