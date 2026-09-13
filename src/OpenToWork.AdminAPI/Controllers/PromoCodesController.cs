using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/promo-codes")]
[RequireStaffRole(AdminStaffRole.Comercial)]
public class PromoCodesController : AdminControllerBase
{
    private readonly IPromoCodeService _promoCodes;

    public PromoCodesController(IPromoCodeService promoCodes)
    {
        _promoCodes = promoCodes;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _promoCodes.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SavePromoCodeDto dto)
    {
        try
        {
            return Ok(await _promoCodes.CreateAsync(dto, AdminId, ClientIp));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SavePromoCodeDto dto)
    {
        try
        {
            var result = await _promoCodes.UpdateAsync(id, dto, AdminId, ClientIp);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _promoCodes.DeleteAsync(id, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }
}
