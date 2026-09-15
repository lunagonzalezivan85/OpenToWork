namespace OpenToWork.Shared.Enums;

/// <summary>Motivo por el que se activa una reposicion de garantia. Los primeros 6 estan
/// cubiertos por la garantia (TD debe reponer); los ultimos 4 son exclusiones contractuales
/// (la empresa incumplio, TD no esta obligado a reponer sin costo).</summary>
public enum WarrantyReplacementReason
{
    BajoDesempeno = 0,
    ProblemasAsistencia = 1,
    FaltaGraveORobo = 2,
    BajaMedica = 3,
    RenunciaVoluntaria = 4,
    Otro = 5,
    ImpagoDeNomina = 6,
    CambioSustancialDeCondiciones = 7,
    CierreDelNegocio = 8,
    IncumplimientoNormativo = 9
}

public static class WarrantyReplacementReasonExtensions
{
    public static bool IsExclusion(this WarrantyReplacementReason reason) => reason >= WarrantyReplacementReason.ImpagoDeNomina;
}

public enum WarrantyReplacementStatus
{
    EnCurso = 0,
    Completada = 1,
    Cancelada = 2,
    ExcluidaDeGarantia = 3
}
