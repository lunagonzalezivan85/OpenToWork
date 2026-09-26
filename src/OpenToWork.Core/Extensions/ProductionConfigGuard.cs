using Microsoft.Extensions.Configuration;

namespace OpenToWork.Core.Extensions;

/// <summary>
/// Comprobacion al arrancar los API fuera de Development (despliegue en el servidor, 26-Sep): el repositorio
/// es publico, asi que las claves JWT y la cadena de conexion de appsettings.json son de desarrollo y nunca
/// deben usarse en produccion. Los valores reales van en appsettings.Production.json, que solo existe en el
/// servidor (ignorado por git). Si falta algo, el API no arranca y el error dice que corregir.
/// </summary>
public static class ProductionConfigGuard
{
    private static readonly string[] DevJwtKeys =
    {
        "OpenToWorkPortalSecretKey2026Min256Bits!!",
        "OpenToWorkAdminSecretKey2026Min256Bits!!"
    };

    public static void Validate(IConfiguration config, bool isDevelopment)
    {
        if (isDevelopment) return;

        var errors = new List<string>();

        var jwtKey = config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32 || DevJwtKeys.Contains(jwtKey))
            errors.Add("Jwt:Key debe ser una clave propia de al menos 32 caracteres (no la del repositorio).");

        if (string.IsNullOrWhiteSpace(config["Storage:Root"]))
            errors.Add("Storage:Root debe apuntar a la carpeta privada de CV y fotos (fuera del sitio web).");

        var cs = config.GetConnectionString("DefaultConnection") ?? "";
        if (cs.Contains("User=root", StringComparison.OrdinalIgnoreCase) || cs.Contains("Password=;", StringComparison.OrdinalIgnoreCase))
            errors.Add("ConnectionStrings:DefaultConnection no puede usar root ni una contraseña vacia.");

        if (errors.Count > 0)
            throw new InvalidOperationException(
                "Configuracion de produccion incompleta (appsettings.Production.json):\n - " + string.Join("\n - ", errors));
    }
}
