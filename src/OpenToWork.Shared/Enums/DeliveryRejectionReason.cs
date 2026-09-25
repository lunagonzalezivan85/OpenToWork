namespace OpenToWork.Shared.Enums;

/// <summary>Motivo estructurado por el que la empresa descarto una entrega (DeliveryStatus.RejectedByCompany).
/// Permite contar rechazos por motivo en el historial del candidato y detectar candidatos "quemados".</summary>
public enum DeliveryRejectionReason
{
    PerfilNoEncaja = 0,
    Salario = 1,
    ExperienciaInsuficiente = 2,
    NoSePresento = 3,
    ActitudEnEntrevista = 4,
    VacanteCubiertaOCancelada = 5,
    Otro = 6
}
