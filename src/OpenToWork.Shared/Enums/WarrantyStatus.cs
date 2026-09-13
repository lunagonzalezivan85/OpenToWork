namespace OpenToWork.Shared.Enums;

/// <summary>Estado de la garantia de reposicion de una colocacion, calculado en vivo a partir
/// de IncorporationDate + WarrantyDays del contrato. Nunca se persiste.</summary>
public enum WarrantyStatus
{
    Activa = 0,
    PorVencer = 1,
    Vencida = 2
}
