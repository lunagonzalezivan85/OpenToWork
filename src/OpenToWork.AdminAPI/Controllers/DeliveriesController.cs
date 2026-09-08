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

    public DeliveriesController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
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
}
