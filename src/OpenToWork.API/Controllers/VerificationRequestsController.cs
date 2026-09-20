using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VerificationRequestsController : ControllerBase
{
    private readonly IVerificationRequestService _service;
    private readonly IWebHostEnvironment _env;

    public VerificationRequestsController(IVerificationRequestService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    /// <summary>Ultima solicitud de verificacion del candidato autenticado.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetLatest()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _service.GetLatestAsync(userId.Value);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>Crea una solicitud de verificacion con documentos adjuntos (base64).</summary>
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitVerificationRequestDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (dto.AdminSituation < 0 || dto.AdminSituation > 3)
            return BadRequest("AdminSituation invalido");

        var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads");
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _service.SubmitAsync(userId.Value, dto, uploadsRoot, ip);
        return result != null ? Ok(result) : NotFound("Perfil de candidato no encontrado");
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
