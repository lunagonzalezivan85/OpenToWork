using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;

namespace OpenToWork.API.Controllers;

/// <summary>Noticias publicas: sin login, solo lo publicado y solo con la seccion encendida en el admin.</summary>
[ApiController]
[Route("api/news")]
public class NewsController : ControllerBase
{
    private readonly INewsService _news;
    private readonly INewsImageStorage _images;
    private readonly ISystemConfigService _config;

    public NewsController(INewsService news, INewsImageStorage images, ISystemConfigService config)
    {
        _news = news;
        _images = images;
        _config = config;
    }

    [HttpGet("enabled")]
    public async Task<IActionResult> Enabled() => Ok(new { enabled = await _config.GetNewsEnabledAsync() });

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? type, [FromQuery] int page = 1, [FromQuery] int pageSize = 9)
    {
        if (!await _config.GetNewsEnabledAsync()) return NotFound();
        return Ok(await _news.GetPublishedAsync(type, page, pageSize));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Get(string slug)
    {
        if (!await _config.GetNewsEnabledAsync()) return NotFound();
        var post = await _news.GetPublishedBySlugAsync(slug);
        return post == null ? NotFound() : Ok(post);
    }

    /// <summary>Solo fotos de publicaciones publicadas. El nombre lleva fecha, asi que se puede cachear.</summary>
    [HttpGet("images/{fileName}")]
    public async Task<IActionResult> Image(string fileName)
    {
        if (!await _config.GetNewsEnabledAsync() || !await _news.IsPublishedImageAsync(fileName)) return NotFound();
        var image = _images.Resolve(fileName);
        if (image == null) return NotFound();

        Response.Headers.CacheControl = "public, max-age=86400";
        return PhysicalFile(image.Value.Path, image.Value.ContentType);
    }
}
