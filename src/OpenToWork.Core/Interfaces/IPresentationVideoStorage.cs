namespace OpenToWork.Core.Interfaces;

/// <summary>
/// Videos de presentacion de candidatos en almacenamiento privado (misma idea que IProfilePhotoStorage):
/// fuera de wwwroot y servidos solo por endpoints con permiso. Solo los ven el candidato y el equipo
/// de Trato Directo. PTCandidate.PresentationVideoUrl guarda la referencia "/uploads/videos/{archivo}".
/// </summary>
public interface IPresentationVideoStorage
{
    /// <summary>Valida el contenido (MP4/MOV o WebM por su firma, no por la extension) y devuelve la
    /// extension y el content-type. Null si no es un video admitido.</summary>
    (string Extension, string ContentType)? Detect(byte[] header);

    /// <summary>Guarda el video del usuario desde un stream y devuelve la referencia para PresentationVideoUrl.</summary>
    Task<string> SaveAsync(Guid userId, Stream content, string extension);

    /// <summary>Ruta fisica y content-type si el video existe y pertenece a ownerUserId. Null si no.</summary>
    (string Path, string ContentType)? ResolveForOwner(string? videoUrl, Guid ownerUserId);

    void Delete(string? videoUrl, Guid ownerUserId);

    /// <summary>Videos guardados y espacio que ocupan en disco (para vigilarlo desde la configuracion del admin).</summary>
    (int Count, long Bytes) GetUsage();
}
