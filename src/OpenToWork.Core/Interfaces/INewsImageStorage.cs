namespace OpenToWork.Core.Interfaces;

/// <summary>Fotos de Noticias (portada y miniatura de video) en la carpeta privada compartida por las dos APIs
/// (storage/news). Se guardan y se piden por nombre de archivo, nunca por ruta.</summary>
public interface INewsImageStorage
{
    Task<string> SaveAsync(Guid postId, byte[] content, string extension);

    /// <summary>Ruta fisica y tipo, o null si el nombre no es valido o el archivo no existe.</summary>
    (string Path, string ContentType)? Resolve(string? fileName);

    void Delete(string? fileName);
}
