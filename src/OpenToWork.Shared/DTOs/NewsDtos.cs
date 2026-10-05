namespace OpenToWork.Shared.DTOs;

// Noticias (docs/dsiezar/noticias-terminos-de-referencia.md). Las fotos viajan como nombre de archivo:
// cada cliente arma la URL contra su API (admin: api/admin/news/images/..., portal: api/news/images/...).

/// <summary>Crear o editar desde el admin. Un borrador puede ir incompleto; al publicar (o editar algo ya
/// publicado) se exige todo lo obligatorio.</summary>
public class SaveNewsPostDto
{
    public int Type { get; set; }
    public string Title { get; set; } = string.Empty;
    /// <summary>Vacio = se genera del titulo.</summary>
    public string? Slug { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? CoverImageAlt { get; set; }
    public string? VideoUrl { get; set; }
    public bool IsFeatured { get; set; }
    /// <summary>Opcional: para ordenar contenido antiguo. Vacio = la fecha en que se publica.</summary>
    public DateTime? PublishedAt { get; set; }
}

public class NewsPostAdminListDto
{
    public Guid Id { get; set; }
    public int Type { get; set; }
    public int Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CoverImageFile { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? AuthorName { get; set; }
}

public class NewsPostAdminDto : SaveNewsPostDto
{
    public Guid Id { get; set; }
    public int Status { get; set; }
    public string? CoverImageFile { get; set; }
    public string? VideoThumbnailFile { get; set; }
}

/// <summary>Tarjeta del listado del portal.</summary>
public class NewsPostCardDto
{
    public Guid Id { get; set; }
    public int Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? CoverImageFile { get; set; }
    public string? CoverImageAlt { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime PublishedAt { get; set; }
}

public class NewsPageDto
{
    public List<NewsPostCardDto> Items { get; set; } = new();
    public bool HasMore { get; set; }
}

/// <summary>Publicacion completa en el portal. BodyHtml ya viene saneado desde el servidor.</summary>
public class NewsPostDetailDto : NewsPostCardDto
{
    public string BodyHtml { get; set; } = string.Empty;
    /// <summary>"youtube" o "vimeo"; null si no es video.</summary>
    public string? VideoProvider { get; set; }
    /// <summary>Se carga solo al pulsar Reproducir (youtube-nocookie / vimeo dnt).</summary>
    public string? VideoEmbedUrl { get; set; }
    public string? VideoThumbnailFile { get; set; }
}

/// <summary>Resultado de una accion del admin. Error = mensaje para el equipo; StatusCode para la API.</summary>
public record NewsOpResult(Guid? Id, string? Error = null, int StatusCode = 400)
{
    public bool Success => Error == null;
    public static NewsOpResult Ok(Guid id) => new(id);
    public static NewsOpResult NotFound() => new(null, "La publicación no existe.", 404);
    public static NewsOpResult Fail(string error) => new(null, error);
}
