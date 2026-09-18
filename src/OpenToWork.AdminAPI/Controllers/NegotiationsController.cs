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
    private readonly IWarrantyReplacementService _warrantyReplacementService;

    public NegotiationsController(INegotiationService negotiationService, IWarrantyReplacementService warrantyReplacementService)
    {
        _negotiationService = negotiationService;
        _warrantyReplacementService = warrantyReplacementService;
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

    /// <summary>Registra la fecha de incorporacion del candidato ganador (paso 17). Solo permitido
    /// en una negociacion cerrada; habilita el calculo de garantia (paso 19).</summary>
    [HttpPut("{id}/incorporation")]
    public async Task<IActionResult> SetIncorporationDate(Guid id, [FromBody] SetIncorporationDateDto dto)
    {
        try
        {
            var result = await _negotiationService.SetIncorporationDateAsync(id, dto.IncorporationDate, AdminId);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Activa una reposicion de garantia (paso 20) sobre esta negociacion cerrada.</summary>
    [HttpPost("{id}/warranty-replacement")]
    public async Task<IActionResult> ActivateWarrantyReplacement(Guid id, [FromBody] ActivateWarrantyReplacementDto dto)
    {
        try
        {
            var result = await _warrantyReplacementService.ActivateFromNegotiationAsync(id, dto, AdminId, ClientIp);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Cierre administrativo del proceso post-garantia (paso 21).</summary>
    [HttpPost("{id}/close-process")]
    public async Task<IActionResult> CloseProcess(Guid id, [FromBody] CloseProcessDto dto)
    {
        try
        {
            var result = await _negotiationService.CloseProcessAsync(id, dto, AdminId);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Registra feedback de mejora continua (paso 22) sobre un proceso ya cerrado.</summary>
    [HttpPost("{id}/feedback")]
    public async Task<IActionResult> RecordFeedback(Guid id, [FromBody] RecordFeedbackDto dto)
    {
        try
        {
            var result = await _negotiationService.RecordFeedbackAsync(id, dto, AdminId);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
