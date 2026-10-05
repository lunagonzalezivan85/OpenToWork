using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>Publicacion de la seccion Noticias (docs/dsiezar/noticias-terminos-de-referencia.md).
/// La crea y publica el equipo desde el admin; el portal solo ve las publicadas.</summary>
public class PTNewsPost : BaseEntity
{
    /// <summary>OpenToWork.Shared.Enums.NewsPostType: Article=0, Video=1, Announcement=2.</summary>
    public int Type { get; set; }

    /// <summary>OpenToWork.Shared.Enums.NewsPostStatus: Draft=0, Published=1, Archived=2.</summary>
    public int Status { get; set; }

    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Direccion en el portal (/news/{slug}). Unica.</summary>
    [Required]
    [MaxLength(90)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(280)]
    public string Summary { get; set; } = string.Empty;

    /// <summary>Markdown sencillo; se convierte a HTML seguro en el servidor (NewsMarkdown).</summary>
    [Column(TypeName = "longtext")]
    public string Body { get; set; } = string.Empty;

    /// <summary>Nombre del archivo en storage/news (INewsImageStorage), no una ruta.</summary>
    [MaxLength(120)]
    public string? CoverImageFile { get; set; }

    [MaxLength(200)]
    public string? CoverImageAlt { get; set; }

    [MaxLength(300)]
    public string? VideoUrl { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime? PublishedAt { get; set; }
}
