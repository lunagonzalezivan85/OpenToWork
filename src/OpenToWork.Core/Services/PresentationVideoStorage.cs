using OpenToWork.Core.Interfaces;

namespace OpenToWork.Core.Services;

/// <summary>Videos de presentacion en una carpeta privada compartida por el API del portal y el del
/// admin (ver IPresentationVideoStorage).</summary>
public class PresentationVideoStorage : IPresentationVideoStorage
{
    /// <summary>
    /// 95 MB: un minuto grabado con el movil ronda 60-130 MB en 1080p, y Cloudflare corta las peticiones
    /// a 100 MB. Tambien hay que permitirlo en IIS (requestLimits en los web.config del WEB y del API).
    /// </summary>
    public const long MaxBytes = 95_000_000;

    /// <summary>Bytes del principio del archivo que hacen falta para reconocer el formato.</summary>
    public const int HeaderBytes = 16;

    private readonly string _dir;

    public PresentationVideoStorage(string storageRoot)
    {
        _dir = Path.Combine(storageRoot, "videos");
        Directory.CreateDirectory(_dir);
    }

    public (string Extension, string ContentType)? Detect(byte[] h)
    {
        // WebM / Matroska (lo que graba el navegador): cabecera EBML 1A 45 DF A3.
        if (h.Length >= 4 && h[0] == 0x1A && h[1] == 0x45 && h[2] == 0xDF && h[3] == 0xA3)
            return (".webm", "video/webm");

        // MP4 y MOV (moviles): caja "ftyp" en el byte 4. "qt  " es QuickTime (iPhone).
        if (h.Length >= 12 && h[4] == 'f' && h[5] == 't' && h[6] == 'y' && h[7] == 'p')
        {
            var brand = System.Text.Encoding.ASCII.GetString(h, 8, 4);
            return brand == "qt  " ? (".mov", "video/quicktime") : (".mp4", "video/mp4");
        }

        return null;
    }

    public async Task<string> SaveAsync(Guid userId, Stream content, string extension)
    {
        var fileName = $"video_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        await using (var file = File.Create(Path.Combine(_dir, fileName)))
        {
            await content.CopyToAsync(file);
        }
        return $"/uploads/videos/{fileName}";
    }

    public (string Path, string ContentType)? ResolveForOwner(string? videoUrl, Guid ownerUserId)
    {
        if (string.IsNullOrWhiteSpace(videoUrl)) return null;

        // Solo el nombre (sin rutas: evita salir de la carpeta con "../") y del dueño.
        var fileName = Path.GetFileName(videoUrl);
        if (!fileName.StartsWith($"video_{ownerUserId}_", StringComparison.OrdinalIgnoreCase)) return null;

        var path = Path.Combine(_dir, fileName);
        if (!File.Exists(path)) return null;

        var contentType = Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".webm" => "video/webm",
            ".mov" => "video/quicktime",
            _ => "video/mp4"
        };
        return (path, contentType);
    }

    public void Delete(string? videoUrl, Guid ownerUserId)
    {
        var resolved = ResolveForOwner(videoUrl, ownerUserId);
        if (resolved != null) File.Delete(resolved.Value.Path);
    }
}
