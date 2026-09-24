using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/recruitment-deliveries")]
[RequireStaffRole(AdminStaffRole.Reclutador)]
public class DeliveriesController : AdminControllerBase
{
    private readonly IDeliveryService _deliveryService;
    private readonly IWarrantyReplacementService _warrantyReplacementService;

    public DeliveriesController(IDeliveryService deliveryService, IWarrantyReplacementService warrantyReplacementService)
    {
        _deliveryService = deliveryService;
        _warrantyReplacementService = warrantyReplacementService;
    }

    [HttpPost]
    public async Task<IActionResult> Deliver([FromBody] DeliverCandidateDto dto)
    {
        try
        {
            var result = await _deliveryService.DeliverCandidateAsync(dto, AdminId, ClientIp);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("recruitment/{recruitmentId}")]
    public async Task<IActionResult> ByRecruitment(Guid recruitmentId)
    {
        var result = await _deliveryService.GetDeliveriesByRecruitmentAsync(recruitmentId);
        return Ok(result);
    }

    /// <summary>Registra la respuesta de la empresa (Interesado/Contratado/Descartado) en su nombre,
    /// cuando la empresa responde fuera del portal (telefono, WhatsApp, correo).</summary>
    [HttpPut("{id}/company-response")]
    public async Task<IActionResult> RecordCompanyResponse(Guid id, [FromBody] RespondDeliveryDto dto)
    {
        try
        {
            var result = await _deliveryService.RecordCompanyResponseByAdminAsync(id, dto.Status, dto.Feedback, AdminId, ClientIp);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>"Liberar candidato": el candidato dejo su puesto fuera de garantia (renuncia,
    /// despido, fin de contrato) y vuelve a estar disponible para otras plazas.</summary>
    [HttpPost("release-candidate/{userId}")]
    public async Task<IActionResult> ReleaseCandidate(Guid userId, [FromBody] ReleaseCandidateDto dto)
    {
        try
        {
            var released = await _deliveryService.ReleaseCandidateAsync(userId, dto, AdminId, ClientIp);
            return Ok(new { released });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Registra la fecha de incorporacion del candidato (paso 17). Solo permitido cuando
    /// la entrega esta en estado Contratado; habilita el calculo de garantia (paso 19).</summary>
    [HttpPut("{id}/incorporation")]
    public async Task<IActionResult> SetIncorporationDate(Guid id, [FromBody] SetIncorporationDateDto dto)
    {
        try
        {
            var result = await _deliveryService.SetIncorporationDateAsync(id, dto.IncorporationDate, AdminId);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Paso 16: fecha en que la empresa contrata formalmente al candidato.</summary>
    [HttpPut("{id}/hiring-date")]
    public async Task<IActionResult> SetHiringDate(Guid id, [FromBody] SetHiringDateDto dto)
    {
        try
        {
            var result = await _deliveryService.SetHiringDateAsync(id, dto.HiringDate, AdminId);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Activa una reposicion de garantia (paso 20) sobre esta entrega Contratada.</summary>
    [HttpPost("{id}/warranty-replacement")]
    public async Task<IActionResult> ActivateWarrantyReplacement(Guid id, [FromBody] ActivateWarrantyReplacementDto dto)
    {
        try
        {
            var result = await _warrantyReplacementService.ActivateFromDeliveryAsync(id, dto, AdminId, ClientIp);
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
            var result = await _deliveryService.CloseProcessAsync(id, dto, AdminId);
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
            var result = await _deliveryService.RecordFeedbackAsync(id, dto, AdminId);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
