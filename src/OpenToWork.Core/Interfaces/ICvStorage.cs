namespace OpenToWork.Core.Interfaces;

/// <summary>
/// Almacenamiento privado de CVs (revision de seguridad 25-Sep): antes se guardaban en wwwroot/uploads/cv
/// del API, servidos como archivos estaticos a cualquiera y hasta commiteados al repo publico. Ahora viven
/// fuera de lo publicable (Storage:Root, por defecto &lt;repo&gt;/storage) y solo se entregan por endpoints
/// con permiso. PTCandidate.CvUrl conserva el formato "/uploads/cv/{archivo}" como referencia.
/// </summary>
public interface ICvStorage
{
    /// <summary>Nombre de archivo para un CV nuevo del usuario: cv_{userId}_{timestamp}.pdf.</summary>
    string NewFileName(Guid userId);

    /// <summary>Referencia que se guarda en PTCandidate.CvUrl.</summary>
    string ToCvUrl(string fileName);

    Task SaveAsync(string fileName, byte[] content);

    /// <summary>Ruta fisica del CV si existe y pertenece a ownerUserId (el nombre del archivo lleva el
    /// id del dueño: impide servir el CV de otro aunque alguien escriba a mano su CvUrl). Null si no.</summary>
    string? ResolveForOwner(string? cvUrl, Guid ownerUserId);
}
