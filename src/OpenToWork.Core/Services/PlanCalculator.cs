namespace OpenToWork.Core.Services;

/// <summary>Calculo puro (sin acceso a datos) de si un plan de mejora (candidato o empresa) sigue
/// vigente. Sin BackgroundService: igual que WarrantyCalculator, el vencimiento se calcula en vivo
/// cada vez que se consulta - nada "revierte" el PlanTier en la base, simplemente se ignora si vencio.</summary>
public static class PlanCalculator
{
    /// <summary>Meses de vigencia al asignar/renovar un plan manualmente (hasta que este Stripe,
    /// que va a fijar PlanExpiresAt con el current_period_end real de la suscripcion).</summary>
    public const int ValidityMonths = 1;

    public static bool IsActive(DateTime? planExpiresAt) =>
        planExpiresAt.HasValue && planExpiresAt.Value > DateTime.UtcNow;

    public static DateTime NewExpirationFromNow() => DateTime.UtcNow.AddMonths(ValidityMonths);
}
