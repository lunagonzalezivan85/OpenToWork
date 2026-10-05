using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

/// <summary>Noticias (docs/dsiezar/noticias-terminos-de-referencia.md). Admin: todo el staff crea y publica.
/// Portal: solo lo publicado.</summary>
public interface INewsService
{
    // --- Admin ---
    Task<List<NewsPostAdminListDto>> GetAllAsync(int? status, string? search);
    Task<NewsPostAdminDto?> GetAsync(Guid id);
    Task<NewsOpResult> CreateAsync(SaveNewsPostDto dto, Guid adminId);
    Task<NewsOpResult> UpdateAsync(Guid id, SaveNewsPostDto dto, Guid adminId);
    /// <summary>Foto ya comprobada (tipo y tamano) por el controller.</summary>
    Task<NewsOpResult> SetCoverAsync(Guid id, byte[] content, string extension, Guid adminId);
    Task<NewsOpResult> PublishAsync(Guid id, Guid adminId);
    Task<NewsOpResult> UnpublishAsync(Guid id, Guid adminId);
    Task<NewsOpResult> ArchiveAsync(Guid id, Guid adminId);
    /// <summary>Solo borradores y archivadas; borra tambien sus fotos.</summary>
    Task<NewsOpResult> DeleteAsync(Guid id, Guid adminId);
    /// <summary>Vista previa del texto tal como saldra en el portal.</summary>
    string RenderBody(string? markdown);

    // --- Portal ---
    Task<NewsPageDto> GetPublishedAsync(int? type, int page, int pageSize);
    Task<NewsPostDetailDto?> GetPublishedBySlugAsync(string slug);
    /// <summary>true si la foto pertenece a una publicacion publicada (el portal no sirve fotos de borradores).</summary>
    Task<bool> IsPublishedImageAsync(string fileName);
}
