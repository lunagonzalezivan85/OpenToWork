using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/system-config")]
[RequireStaffRole]
public class SystemConfigController : AdminControllerBase
{
    private readonly ISystemConfigService _configService;

    public SystemConfigController(ISystemConfigService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _configService.GetAllAsync();
        return Ok(items);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateBulk([FromBody] UpdateSystemConfigDto dto)
    {
        await _configService.UpdateBulkAsync(dto, AdminId);
        return Ok();
    }
}
