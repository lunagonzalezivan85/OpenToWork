using OpenToWork.Core.Interfaces;

namespace OpenToWork.Core.Services;

/// <summary>CVs en una carpeta privada compartida por el API del portal y el del admin (ver ICvStorage).</summary>
public class CvStorage : ICvStorage
{
    private readonly string _cvDir;

    public CvStorage(string storageRoot)
    {
        _cvDir = Path.Combine(storageRoot, "cv");
        Directory.CreateDirectory(_cvDir);
    }

    public string NewFileName(Guid userId) => $"cv_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

    public string ToCvUrl(string fileName) => $"/uploads/cv/{fileName}";

    public Task SaveAsync(string fileName, byte[] content) =>
        File.WriteAllBytesAsync(Path.Combine(_cvDir, Path.GetFileName(fileName)), content);

    public string? ResolveForOwner(string? cvUrl, Guid ownerUserId)
    {
        if (string.IsNullOrWhiteSpace(cvUrl)) return null;

        // Solo el nombre (sin rutas: evita salir de la carpeta con "../") y del dueño.
        var fileName = Path.GetFileName(cvUrl);
        if (!fileName.StartsWith($"cv_{ownerUserId}_", StringComparison.OrdinalIgnoreCase)) return null;

        var path = Path.Combine(_cvDir, fileName);
        return File.Exists(path) ? path : null;
    }
}
