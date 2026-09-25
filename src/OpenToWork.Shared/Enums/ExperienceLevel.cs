namespace OpenToWork.Shared.Enums;

/// <summary>Experiencia requerida por la vacante, en rangos de anos (escala pedida por Darwin, 24-Sep).
/// Los valores enteros se mantienen 0-4; la migracion ExperienceLevelYearRanges tradujo las vacantes
/// que usaban la escala anterior (Entry/Junior/Mid/Senior/Lead) para que conserven su significado.</summary>
public enum ExperienceLevel
{
    None = 0,
    LessThanOneYear = 1,
    OneToThreeYears = 2,
    ThreeToFiveYears = 3,
    MoreThanFiveYears = 4
}
