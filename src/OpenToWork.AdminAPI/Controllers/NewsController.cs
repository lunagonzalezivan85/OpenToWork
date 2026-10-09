using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Core.Services;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Controllers;

/// <summary>Noticias (docs/dsiezar/noticias-terminos-de-referencia.md). Decision de Darwin (5-Oct): todo el
/// staff (SuperAdmin, Comercial y Reclutador) crea, publica, despublica, archiva y borra.</summary>
[Route("api/admin/news")]
[RequireStaffRole(AdminStaffRole.Comercial, AdminStaffRole.Reclutador)]
public class NewsController : AdminControllerBase
{
    private readonly INewsService _news;
    private readonly INewsImageStorage _images;
    private readonly IConfiguration _config;

    public NewsController(INewsService news, INewsImageStorage images, IConfiguration config)
    {
        _news = news;
        _images = images;
        _config = config;
    }

    /// <summary>Direccion del portal para "Ver en el portal" (misma clave que las invitaciones a empresas).</summary>
    [HttpGet("portal-url")]
    public IActionResult PortalUrl() => Ok(new { url = (_config["Portal:BaseUrl"] ?? "http://localhost:5100/").TrimEnd('/') });

    private IActionResult Map(NewsOpResult r) => r.Success ? Ok(new { id = r.Id }) : StatusCode(r.StatusCode, new { error = r.Error });

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? status, [FromQuery] string? search) => Ok(await _news.GetAllAsync(status, search));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var post = await _news.GetAsync(id);
        return post == null ? NotFound() : Ok(post);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveNewsPostDto dto) => Map(await _news.CreateAsync(dto, AdminId));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveNewsPostDto dto) => Map(await _news.UpdateAsync(id, dto, AdminId));

    /// <summary>Foto de portada. Llega ya reducida desde el navegador; aqui se comprueba tipo real y tamano.</summary>
    [HttpPost("{id:guid}/cover")]
    [RequestSizeLimit(NewsImageStorage.MaxBytes + 100_000)]
    public async Task<IActionResult> UploadCover(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(new { error = "No se recibió ninguna foto." });
        if (file.Length > NewsImageStorage.MaxBytes) return BadRequest(new { error = "La foto no puede superar 2 MB." });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();

        var type = ProfilePhotoStorage.DetectImage(bytes);
        if (type == null) return BadRequest(new { error = "Solo se admiten fotos JPG, PNG o WebP." });

        return Map(await _news.SetCoverAsync(id, bytes, type.Value.Extension, AdminId));
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id) => Map(await _news.PublishAsync(id, AdminId));

    [HttpPost("{id:guid}/unpublish")]
    public async Task<IActionResult> Unpublish(Guid id) => Map(await _news.UnpublishAsync(id, AdminId));

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id) => Map(await _news.ArchiveAsync(id, AdminId));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) => Map(await _news.DeleteAsync(id, AdminId));

    /// <summary>El texto tal como saldra en el portal (mismo conversor que la API publica).</summary>
    [HttpPost("preview")]
    public IActionResult Preview([FromBody] NewsPreviewDto dto) => Ok(new { html = _news.RenderBody(dto.Body) });

    /// <summary>Cualquier foto de Noticias, tambien de borradores (el admin la pide con su token).</summary>
    [HttpGet("images/{fileName}")]
    public IActionResult Image(string fileName)
    {
        var image = _images.Resolve(fileName);
        return image == null ? NotFound() : PhysicalFile(image.Value.Path, image.Value.ContentType);
    }
}

public class NewsPreviewDto
{
    public string? Body { get; set; }
}
