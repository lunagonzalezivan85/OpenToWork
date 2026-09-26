using OpenToWork.Core.Interfaces;

namespace OpenToWork.Core.Services;

/// <summary>Fotos de perfil en una carpeta privada compartida por el API del portal y el del admin
/// (ver IProfilePhotoStorage).</summary>
public class ProfilePhotoStorage : IProfilePhotoStorage
{
    public const int MaxBytes = 2_000_000;

    private readonly string _dir;

    public ProfilePhotoStorage(string storageRoot)
    {
        _dir = Path.Combine(storageRoot, "photos");
        Directory.CreateDirectory(_dir);
    }

    public (string Extension, string ContentType)? Detect(byte[] c)
    {
        if (c.Length > 3 && c[0] == 0xFF && c[1] == 0xD8 && c[2] == 0xFF) return (".jpg", "image/jpeg");
        if (c.Length > 8 && c[0] == 0x89 && c[1] == 0x50 && c[2] == 0x4E && c[3] == 0x47) return (".png", "image/png");
        if (c.Length > 12 && c[0] == 'R' && c[1] == 'I' && c[2] == 'F' && c[3] == 'F'
            && c[8] == 'W' && c[9] == 'E' && c[10] == 'B' && c[11] == 'P') return (".webp", "image/webp");
        return null;
    }

    public async Task<string> SaveAsync(Guid userId, byte[] content, string extension)
    {
        var fileName = $"photo_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        await File.WriteAllBytesAsync(Path.Combine(_dir, fileName), content);
        return $"/uploads/photos/{fileName}";
    }

    public (string Path, string ContentType)? ResolveForOwner(string? photoUrl, Guid ownerUserId)
    {
        if (string.IsNullOrWhiteSpace(photoUrl)) return null;

        // Solo el nombre (sin rutas: evita salir de la carpeta con "../") y del dueño.
        var fileName = Path.GetFileName(photoUrl);
        if (!fileName.StartsWith($"photo_{ownerUserId}_", StringComparison.OrdinalIgnoreCase)) return null;

        var path = Path.Combine(_dir, fileName);
        if (!File.Exists(path)) return null;

        var contentType = Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
        return (path, contentType);
    }

    public void Delete(string? photoUrl, Guid ownerUserId)
    {
        var resolved = ResolveForOwner(photoUrl, ownerUserId);
        if (resolved != null) File.Delete(resolved.Value.Path);
    }
}
