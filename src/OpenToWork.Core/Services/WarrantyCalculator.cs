using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

/// <summary>Calculo puro (sin acceso a datos) del estado de garantia de una colocacion.
/// Devuelve null cuando falta un dato (sin fecha de incorporacion, o el contrato no
/// tiene periodo de garantia definido) - en ese caso no hay nada que mostrar.</summary>
public static class WarrantyCalculator
{
    /// <summary>Dias antes del vencimiento en que el estado pasa a "Por vencer".</summary>
    public const int ExpiringThresholdDays = 7;

    public static (DateTime? EndsAt, WarrantyStatus? Status) Calculate(DateTime? incorporationDate, int? warrantyDays)
    {
        if (incorporationDate == null || warrantyDays == null) return (null, null);

        var endsAt = incorporationDate.Value.Date.AddDays(warrantyDays.Value);
        var daysLeft = (endsAt - DateTime.UtcNow.Date).TotalDays;

        var status = daysLeft < 0
            ? WarrantyStatus.Vencida
            : daysLeft <= ExpiringThresholdDays
                ? WarrantyStatus.PorVencer
                : WarrantyStatus.Activa;

        return (endsAt, status);
    }
}
