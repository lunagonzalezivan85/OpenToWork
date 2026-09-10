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
