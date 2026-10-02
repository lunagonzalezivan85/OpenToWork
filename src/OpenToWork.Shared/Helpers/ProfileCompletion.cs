using OpenToWork.Shared.DTOs;

namespace OpenToWork.Shared.Helpers;

/// <summary>
/// Porcentaje del perfil del candidato y los datos que le faltan. Una sola fuente para el circulo del
/// panel y la ventana "Completar perfil" (que solo pide lo pendiente, sin repetir la carga desde cero).
/// "Asistente terminado" ya no cuenta: el 100 % se alcanza solo con datos.
/// </summary>
public static class ProfileCompletion
{
    public const string FirstName = "firstName";
    public const string LastName = "lastName";
    public const string Identification = "identification";
    public const string Phone = "phone";
    public const string BirthDate = "birthDate";
    public const string Title = "title";
    public const string Summary = "summary";
    public const string Country = "country";
    public const string City = "city";
    public const string LinkedIn = "linkedIn";
    public const string YearsOfExperience = "yearsOfExperience";
    public const string Cv = "cv";
    public const string Experience = "experience";
    public const string Education = "education";

    /// <summary>Todos los datos que cuentan, en el orden en que se piden en la ventana.</summary>
    public static readonly string[] AllKeys =
    {
        FirstName, LastName, Identification, Phone, BirthDate, Title, Summary,
        Country, City, YearsOfExperience, LinkedIn, Cv, Experience, Education
    };

    public static List<string> Missing(CandidateDto c)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(c.FirstName)) missing.Add(FirstName);
        if (string.IsNullOrWhiteSpace(c.LastName)) missing.Add(LastName);
        if (string.IsNullOrWhiteSpace(c.Identification)) missing.Add(Identification);
        if (string.IsNullOrWhiteSpace(c.Phone)) missing.Add(Phone);
        if (!c.BirthDate.HasValue) missing.Add(BirthDate);
        if (string.IsNullOrWhiteSpace(c.Title)) missing.Add(Title);
        if (string.IsNullOrWhiteSpace(c.Summary)) missing.Add(Summary);
        if (string.IsNullOrWhiteSpace(c.Country)) missing.Add(Country);
        if (string.IsNullOrWhiteSpace(c.City)) missing.Add(City);
        if (!c.YearsOfExperience.HasValue) missing.Add(YearsOfExperience);
        if (string.IsNullOrWhiteSpace(c.LinkedInUrl)) missing.Add(LinkedIn);
        if (string.IsNullOrWhiteSpace(c.CvUrl)) missing.Add(Cv);
        if (c.Experiences == null || c.Experiences.Count == 0) missing.Add(Experience);
        if (c.Educations == null || c.Educations.Count == 0) missing.Add(Education);
        return missing;
    }

    public static int Percent(CandidateDto c) =>
        (int)Math.Round((double)(AllKeys.Length - Missing(c).Count) / AllKeys.Length * 100);
}
