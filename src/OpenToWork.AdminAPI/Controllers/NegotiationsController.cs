using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/negotiations")]
[RequireStaffRole(AdminStaffRole.Comercial)]
public class NegotiationsController : AdminControllerBase
{
    private readonly INegotiationService _negotiationService;

    public NegotiationsController(INegotiationService negotiationService)
    {
        _negotiationService = negotiationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNegotiationDto dto)
    {
        var result = await _negotiationService.CreateAsync(dto, AdminId);
        if (result == null)
            return BadRequest(new { message = "No se pudo presentar la negociación (vacante o postulaciones inválidas)" });
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateNegotiationStatusDto dto)
    {
        var result = await _negotiationService.UpdateStatusAsync(id, dto.Status);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPut("{id}/close")]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseNegotiationDto dto)
    {
        var result = await _negotiationService.CloseAsync(id, dto.WinningApplicationId, AdminId, ClientIp);
        if (result == null)
            return BadRequest(new { message = "No se pudo cerrar la negociación (postulación ganadora inválida)" });
        return Ok(result);
    }

    [HttpGet("vacancy/{vacancyId}")]
    public async Task<IActionResult> GetByVacancy(Guid vacancyId)
    {
        var result = await _negotiationService.GetByVacancyAsync(vacancyId);
        return Ok(result);
    }
}
