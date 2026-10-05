using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class NewsService : INewsService
{
    public const int TitleMax = 120;
    public const int SummaryMax = 280;
    public const int AltMax = 200;
    public const int BodyMax = 50_000;

    private readonly AppDbContext _context;
    private readonly INewsImageStorage _images;
    private readonly IAuditLogService _audit;

    public NewsService(AppDbContext context, INewsImageStorage images, IAuditLogService audit)
    {
        _context = context;
        _images = images;
        _audit = audit;
    }

    // ------------------------------------------------------------------ Admin

    public async Task<List<NewsPostAdminListDto>> GetAllAsync(int? status, string? search)
    {
        var query = _context.PT_NewsPosts.Where(n => !n.IsDeleted);
        if (status != null) query = query.Where(n => n.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(n => n.Title.Contains(s));
        }

        var posts = await query.OrderByDescending(n => n.UpdatedAt ?? n.CreatedAt).ToListAsync();
        var authorIds = posts.Select(p => p.UpdatedBy ?? p.CreatedBy).OfType<Guid>().Distinct().ToList();
        var authors = await _context.SC_Users.Where(u => authorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.Email);

        return posts.Select(p => new NewsPostAdminListDto
        {
            Id = p.Id,
            Type = p.Type,
            Status = p.Status,
            Title = p.Title,
            Slug = p.Slug,
            CoverImageFile = p.CoverImageFile,
            IsFeatured = p.IsFeatured,
            PublishedAt = p.PublishedAt,
            UpdatedAt = p.UpdatedAt ?? p.CreatedAt,
            AuthorName = (p.UpdatedBy ?? p.CreatedBy) is Guid a && authors.TryGetValue(a, out var name) ? name : null
        }).ToList();
    }

    public async Task<NewsPostAdminDto?> GetAsync(Guid id)
    {
        var p = await Find(id);
        return p == null ? null : new NewsPostAdminDto
        {
            Id = p.Id,
            Status = p.Status,
            Type = p.Type,
            Title = p.Title,
            Slug = p.Slug,
            Summary = p.Summary,
            Body = p.Body,
            CoverImageFile = p.CoverImageFile,
            CoverImageAlt = p.CoverImageAlt,
            VideoUrl = p.VideoUrl,
            IsFeatured = p.IsFeatured,
            PublishedAt = p.PublishedAt
        };
    }

    public async Task<NewsOpResult> CreateAsync(SaveNewsPostDto dto, Guid adminId)
    {
        var error = ValidateBasics(dto);
        if (error != null) return NewsOpResult.Fail(error);

        var post = new PTNewsPost { Status = (int)NewsPostStatus.Draft, CreatedBy = adminId };
        var slugError = await ApplyAsync(post, dto);
        if (slugError != null) return NewsOpResult.Fail(slugError);

        _context.PT_NewsPosts.Add(post);
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "NewsPostCreated", "PT_NewsPosts", post.Id, post.Title, null);
        return NewsOpResult.Ok(post.Id);
    }

    public async Task<NewsOpResult> UpdateAsync(Guid id, SaveNewsPostDto dto, Guid adminId)
    {
        var post = await Find(id);
        if (post == null) return NewsOpResult.NotFound();

        var error = ValidateBasics(dto);
        if (error != null) return NewsOpResult.Fail(error);

        var slugError = await ApplyAsync(post, dto);
        if (slugError != null) return NewsOpResult.Fail(slugError);

        // Lo publicado esta a la vista: no se puede dejar incompleto.
        if (post.Status == (int)NewsPostStatus.Published && ValidateForPublish(post) is { } publishError)
            return NewsOpResult.Fail(publishError);

        Touch(post, adminId);
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "NewsPostUpdated", "PT_NewsPosts", post.Id, post.Title, null);
        return NewsOpResult.Ok(post.Id);
    }

    public async Task<NewsOpResult> SetCoverAsync(Guid id, byte[] content, string extension, Guid adminId)
    {
        var post = await Find(id);
        if (post == null) return NewsOpResult.NotFound();

        var previous = post.CoverImageFile;
        post.CoverImageFile = await _images.SaveAsync(post.Id, content, extension);
        Touch(post, adminId);
        await _context.SaveChangesAsync();
        _images.Delete(previous);
        await _audit.LogAsync(adminId, "NewsPostCoverChanged", "PT_NewsPosts", post.Id, post.Title, null);
        return NewsOpResult.Ok(post.Id);
    }

    public async Task<NewsOpResult> PublishAsync(Guid id, Guid adminId)
    {
        var post = await Find(id);
        if (post == null) return NewsOpResult.NotFound();
        if (ValidateForPublish(post) is { } error) return NewsOpResult.Fail(error);

        post.Status = (int)NewsPostStatus.Published;
        post.PublishedAt ??= DateTime.UtcNow;
        if (post.IsFeatured) await UnfeatureOthersAsync(post.Id);
        Touch(post, adminId);
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "NewsPostPublished", "PT_NewsPosts", post.Id, post.Title, null);
        return NewsOpResult.Ok(post.Id);
    }

    public async Task<NewsOpResult> UnpublishAsync(Guid id, Guid adminId)
    {
        var post = await Find(id);
        if (post == null) return NewsOpResult.NotFound();
        if (post.Status != (int)NewsPostStatus.Published) return NewsOpResult.Fail("Solo se puede despublicar una publicación publicada.");

        post.Status = (int)NewsPostStatus.Draft;
        Touch(post, adminId);
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "NewsPostUnpublished", "PT_NewsPosts", post.Id, post.Title, null);
        return NewsOpResult.Ok(post.Id);
    }

    public async Task<NewsOpResult> ArchiveAsync(Guid id, Guid adminId)
    {
        var post = await Find(id);
        if (post == null) return NewsOpResult.NotFound();

        post.Status = (int)NewsPostStatus.Archived;
        post.IsFeatured = false;
        Touch(post, adminId);
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "NewsPostArchived", "PT_NewsPosts", post.Id, post.Title, null);
        return NewsOpResult.Ok(post.Id);
    }

    public async Task<NewsOpResult> DeleteAsync(Guid id, Guid adminId)
    {
        var post = await Find(id);
        if (post == null) return NewsOpResult.NotFound();
        if (post.Status == (int)NewsPostStatus.Published)
            return NewsOpResult.Fail("Despublica o archiva la publicación antes de borrarla.");

        post.IsDeleted = true;
        post.DeletedAt = DateTime.UtcNow;
        post.DeletedBy = adminId;
        await _context.SaveChangesAsync();
        _images.Delete(post.CoverImageFile);
        await _audit.LogAsync(adminId, "NewsPostDeleted", "PT_NewsPosts", post.Id, post.Title, null);
        return NewsOpResult.Ok(post.Id);
    }

    public string RenderBody(string? markdown) => NewsMarkdown.ToHtml(markdown);

    // ------------------------------------------------------------------ Portal

    public async Task<NewsPageDto> GetPublishedAsync(int? type, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 30);

        var query = Published();
        if (type != null) query = query.Where(n => n.Type == type);

        // La destacada va primero; luego de la mas nueva a la mas antigua.
        var rows = await query
            .OrderByDescending(n => n.IsFeatured)
            .ThenByDescending(n => n.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .ToListAsync();

        return new NewsPageDto
        {
            Items = rows.Take(pageSize).Select(ToCard).ToList(),
            HasMore = rows.Count > pageSize
        };
    }

    public async Task<NewsPostDetailDto?> GetPublishedBySlugAsync(string slug)
    {
        var p = await Published().FirstOrDefaultAsync(n => n.Slug == slug);
        if (p == null) return null;

        var video = p.Type == (int)NewsPostType.Video ? NewsVideo.Parse(p.VideoUrl) : null;
        var card = ToCard(p);
        return new NewsPostDetailDto
        {
            Id = card.Id,
            Type = card.Type,
            Title = card.Title,
            Slug = card.Slug,
            Summary = card.Summary,
            CoverImageFile = card.CoverImageFile,
            CoverImageAlt = card.CoverImageAlt,
            IsFeatured = card.IsFeatured,
            PublishedAt = card.PublishedAt,
            BodyHtml = NewsMarkdown.ToHtml(p.Body),
            VideoProvider = video?.Provider,
            VideoEmbedUrl = video is { } v ? NewsVideo.EmbedUrl(v.Provider, v.Id) : null
        };
    }

    public Task<bool> IsPublishedImageAsync(string fileName) =>
        Published().AnyAsync(n => n.CoverImageFile == fileName);

    // ------------------------------------------------------------------ Helpers

    private IQueryable<PTNewsPost> Published() =>
        _context.PT_NewsPosts.Where(n => !n.IsDeleted && n.Status == (int)NewsPostStatus.Published && n.PublishedAt != null);

    private Task<PTNewsPost?> Find(Guid id) => _context.PT_NewsPosts.FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);

    private static NewsPostCardDto ToCard(PTNewsPost p) => new()
    {
        Id = p.Id,
        Type = p.Type,
        Title = p.Title,
        Slug = p.Slug,
        Summary = p.Summary,
        CoverImageFile = p.CoverImageFile,
        CoverImageAlt = p.CoverImageAlt,
        IsFeatured = p.IsFeatured,
        PublishedAt = p.PublishedAt ?? p.CreatedAt
    };

    private static void Touch(PTNewsPost post, Guid adminId)
    {
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = adminId;
    }

    /// <summary>Lo minimo para guardar un borrador.</summary>
    private static string? ValidateBasics(SaveNewsPostDto dto)
    {
        if (!Enum.IsDefined(typeof(NewsPostType), dto.Type)) return "Elige el tipo de publicación.";
        if (string.IsNullOrWhiteSpace(dto.Title)) return "Escribe un título.";
        if (dto.Title.Trim().Length > TitleMax) return $"El título no puede pasar de {TitleMax} caracteres.";
        if ((dto.Summary?.Trim().Length ?? 0) > SummaryMax) return $"El resumen no puede pasar de {SummaryMax} caracteres.";
        if ((dto.CoverImageAlt?.Trim().Length ?? 0) > AltMax) return $"La descripción de la foto no puede pasar de {AltMax} caracteres.";
        if ((dto.Body?.Length ?? 0) > BodyMax) return "El texto es demasiado largo.";
        if (!string.IsNullOrWhiteSpace(dto.VideoUrl) && NewsVideo.Parse(dto.VideoUrl) == null)
            return "El enlace del vídeo debe ser de YouTube o Vimeo.";
        return null;
    }

    /// <summary>Todo lo obligatorio para estar en el portal.</summary>
    private static string? ValidateForPublish(PTNewsPost p)
    {
        if (string.IsNullOrWhiteSpace(p.Summary)) return "Falta el resumen.";
        if (string.IsNullOrEmpty(p.CoverImageFile)) return "Falta la foto de portada.";
        if (string.IsNullOrWhiteSpace(p.CoverImageAlt)) return "Falta describir la foto (texto alternativo).";
        if (p.Type == (int)NewsPostType.Article && string.IsNullOrWhiteSpace(p.Body)) return "Falta el texto del artículo.";
        if (p.Type == (int)NewsPostType.Video && NewsVideo.Parse(p.VideoUrl) == null) return "Falta el enlace del vídeo (YouTube o Vimeo).";
        return null;
    }

    /// <summary>Copia los datos al post. Devuelve un error si no se puede generar una direccion valida.</summary>
    private async Task<string?> ApplyAsync(PTNewsPost post, SaveNewsPostDto dto)
    {
        var slug = NewsSlug.From(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Title : dto.Slug);
        if (slug.Length == 0) return "La dirección debe tener al menos una letra o número.";

        // Unica tambien frente a borradas: el indice unico de la tabla las incluye.
        var candidate = slug;
        for (var i = 2; await _context.PT_NewsPosts.AnyAsync(n => n.Slug == candidate && n.Id != post.Id); i++)
            candidate = $"{slug[..Math.Min(slug.Length, NewsSlug.MaxLength - 4)]}-{i}";

        post.Type = dto.Type;
        post.Title = dto.Title.Trim();
        post.Slug = candidate;
        post.Summary = dto.Summary?.Trim() ?? string.Empty;
        post.Body = dto.Body?.Trim() ?? string.Empty;
        post.CoverImageAlt = string.IsNullOrWhiteSpace(dto.CoverImageAlt) ? null : dto.CoverImageAlt.Trim();
        post.VideoUrl = string.IsNullOrWhiteSpace(dto.VideoUrl) ? null : dto.VideoUrl.Trim();
        if (dto.PublishedAt != null) post.PublishedAt = DateTime.SpecifyKind(dto.PublishedAt.Value, DateTimeKind.Utc);

        if (dto.IsFeatured && !post.IsFeatured && post.Status == (int)NewsPostStatus.Published)
            await UnfeatureOthersAsync(post.Id);
        post.IsFeatured = dto.IsFeatured && post.Status != (int)NewsPostStatus.Archived;
        return null;
    }

    /// <summary>Solo una destacada a la vez.</summary>
    private async Task UnfeatureOthersAsync(Guid keepId)
    {
        var others = await _context.PT_NewsPosts.Where(n => n.IsFeatured && n.Id != keepId).ToListAsync();
        foreach (var o in others) o.IsFeatured = false;
    }
}
