namespace OpenToWork.Shared;

/// <summary>
/// Version de los textos legales que el usuario acepta al registrarse; se guarda junto al
/// consentimiento (RGPD art. 7.1: hay que poder demostrar que acepto y que texto). Al cambiar
/// /privacy, actualizar aqui la fecha de "Ultima actualizacion" de esa pagina.
/// </summary>
public static class LegalVersions
{
    public const string Privacy = "2026-09-25";
}
