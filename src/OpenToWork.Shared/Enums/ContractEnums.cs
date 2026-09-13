namespace OpenToWork.Shared.Enums;

/// <summary>
/// Estado del anexo de contrato por vacante. Solo Draft es editable; Accepted queda
/// bloqueado (contrato aceptado por la empresa), Rejected puede reabrirse como borrador.
/// </summary>
public enum ContractStatus
{
    Draft = 0,
    Sent = 1,
    Accepted = 2,
    Rejected = 3,
    Cancelled = 4
}

/// <summary>
/// Tipo de puesto para plazos de cobertura de referencia (Anexo seccion 5):
/// Operativo 10, Encargados y Tecnicos 15, Responsables de Negocio 20, Perfiles Cualificados 30 dias habiles.
/// </summary>
public enum ContractJobType
{
    Operational = 0,
    SupervisorsTechnicians = 1,
    BusinessManagers = 2,
    QualifiedProfiles = 3
}

/// <summary>Aplicacion de la tarifa del anexo (seccion 7): por posicion, por proceso o precio global.</summary>
public enum FeeApplicationType
{
    PerPosition = 0,
    PerProcess = 1,
    GlobalPrice = 2
}

/// <summary>Tipo de descuento de un codigo promocional (PTPromoCode).</summary>
public enum PromoDiscountType
{
    Percentage = 0,
    FixedAmount = 1
}

/// <summary>Tramo de pago del anexo (30/50/20): Apertura al firmar, Validacion al elegir
/// candidato del shortlist, Consolidacion 30 dias tras la incorporacion.</summary>
public enum PaymentTrancheType
{
    Apertura = 0,
    Validacion = 1,
    Consolidacion = 2
}

/// <summary>Estado de un tramo de pago (PTContractPayment). Simple pagado/pendiente por ahora,
/// sin pasarela de pago integrada - lo marca un admin a mano.</summary>
public enum PaymentTrancheStatus
{
    Pendiente = 0,
    Pagado = 1
}
