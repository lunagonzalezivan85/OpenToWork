namespace OpenToWork.Core.Interfaces;

/// <summary>
/// Fotos de perfil de candidatos en almacenamiento privado (misma idea que ICvStorage): fuera de wwwroot
/// y servidas solo por endpoints con permiso, para no exponer la cara del candidato (Embudo Ciego).
/// PTCandidate.ProfilePictureUrl guarda la referencia "/uploads/photos/{archivo}".
/// </summary>
public interface IProfilePhotoStorage
{
    /// <summary>Valida el contenido (JPG, PNG o WebP por su firma, no por la extension) y devuelve la
    /// extension y el content-type. Null si no es una imagen admitida.</summary>
    (string Extension, string ContentType)? Detect(byte[] content);

    /// <summary>Guarda la foto del usuario y devuelve la referencia para ProfilePictureUrl.</summary>
    Task<string> SaveAsync(Guid userId, byte[] content, string extension);

    /// <summary>Ruta fisica y content-type si la foto existe y pertenece a ownerUserId. Null si no.</summary>
    (string Path, string ContentType)? ResolveForOwner(string? photoUrl, Guid ownerUserId);

    void Delete(string? photoUrl, Guid ownerUserId);
}
