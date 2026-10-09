using OpenToWork.Core.Interfaces;

namespace OpenToWork.Core.Services;

public class NewsImageStorage : INewsImageStorage
{
    /// <summary>La foto llega ya reducida desde el navegador (1600 px); 2 MB sobra.</summary>
    public const int MaxBytes = 2_000_000;

    private readonly string _dir;

    public NewsImageStorage(string storageRoot)
    {
        _dir = Path.Combine(storageRoot, "news");
        Directory.CreateDirectory(_dir);
    }

    public async Task<string> SaveAsync(Guid postId, byte[] content, string extension)
    {
        var fileName = $"news_{postId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        await File.WriteAllBytesAsync(Path.Combine(_dir, fileName), content);
        return fileName;
    }

    public (string Path, string ContentType)? Resolve(string? fileName)
    {
        // Solo el nombre (sin "../") y con nuestro prefijo.
        if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName || !fileName.StartsWith("news_")) return null;

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

    public void Delete(string? fileName)
    {
        var resolved = Resolve(fileName);
        if (resolved != null) File.Delete(resolved.Value.Path);
    }
}
