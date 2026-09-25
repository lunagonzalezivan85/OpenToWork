using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminWEB.Services;

/// <summary>Claves de traduccion compartidas por las pantallas de entregas (detalle del pipeline,
/// historial del candidato), para no repetir el mismo switch en cada una.</summary>
public static class DeliveryLabels
{
    public static readonly DeliveryRejectionReason[] RejectionReasons = Enum.GetValues<DeliveryRejectionReason>();

    public static string RejectionReasonKey(int? reason) => reason switch
    {
        (int)DeliveryRejectionReason.PerfilNoEncaja => "admin.recruitment.rejection.profileMismatch",
        (int)DeliveryRejectionReason.Salario => "admin.recruitment.rejection.salary",
        (int)DeliveryRejectionReason.ExperienciaInsuficiente => "admin.recruitment.rejection.experience",
        (int)DeliveryRejectionReason.NoSePresento => "admin.recruitment.rejection.noShow",
        (int)DeliveryRejectionReason.ActitudEnEntrevista => "admin.recruitment.rejection.interview",
        (int)DeliveryRejectionReason.VacanteCubiertaOCancelada => "admin.recruitment.rejection.vacancyClosed",
        (int)DeliveryRejectionReason.Otro => "admin.recruitment.rejection.other",
        _ => "admin.recruitment.rejection.unspecified"
    };

    public static string StatusKey(int status) => status switch
    {
        (int)DeliveryStatus.Delivered => "admin.recruitment.deliveryDelivered",
        (int)DeliveryStatus.ViewedByCompany => "admin.recruitment.deliveryViewed",
        (int)DeliveryStatus.Interested => "admin.recruitment.deliveryInterested",
        (int)DeliveryStatus.Hired => "admin.recruitment.deliveryHired",
        (int)DeliveryStatus.RejectedByCompany => "admin.recruitment.deliveryRejected",
        _ => "-"
    };

    public static string StatusBadgeClass(int status) => status switch
    {
        (int)DeliveryStatus.Hired => "admin-result-badge--success",
        (int)DeliveryStatus.Interested => "admin-result-badge--info",
        (int)DeliveryStatus.RejectedByCompany => "admin-result-badge--danger",
        _ => "admin-result-badge--warning"
    };
}
