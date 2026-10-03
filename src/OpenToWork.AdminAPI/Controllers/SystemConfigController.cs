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
    private readonly IPresentationVideoStorage _videoStorage;

    public SystemConfigController(ISystemConfigService configService, IPresentationVideoStorage videoStorage)
    {
        _configService = configService;
        _videoStorage = videoStorage;
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

    [HttpGet("ai")]
    public async Task<IActionResult> GetAiSettings()
    {
        var result = await _configService.GetAiSettingsAsync();
        return Ok(result);
    }

    [HttpPut("ai")]
    public async Task<IActionResult> UpdateAiSettings([FromBody] AiSettingsDto dto)
    {
        await _configService.UpdateAiSettingsAsync(dto, AdminId);
        return Ok(await _configService.GetAiSettingsAsync());
    }

    /// <summary>Interruptor de videos de presentacion y espacio que ocupan los guardados.</summary>
    [HttpGet("presentation-videos")]
    public async Task<IActionResult> GetPresentationVideos()
    {
        var (count, bytes) = _videoStorage.GetUsage();
        return Ok(new PresentationVideoSettingsDto
        {
            Enabled = await _configService.GetPresentationVideosEnabledAsync(),
            VideoCount = count,
            TotalBytes = bytes
        });
    }

    [HttpPut("presentation-videos")]
    public async Task<IActionResult> SetPresentationVideos([FromBody] SetFeatureFlagDto dto)
    {
        await _configService.SetPresentationVideosEnabledAsync(dto.Enabled, AdminId);
        return Ok();
    }

    [HttpGet("candidate-priority-plan")]
    public async Task<IActionResult> GetCandidatePriorityPlanEnabled()
    {
        var enabled = await _configService.GetCandidatePriorityPlanEnabledAsync();
        return Ok(new { enabled });
    }

    [HttpPut("candidate-priority-plan")]
    public async Task<IActionResult> SetCandidatePriorityPlanEnabled([FromBody] SetFeatureFlagDto dto)
    {
        await _configService.SetCandidatePriorityPlanEnabledAsync(dto.Enabled, AdminId);
        return Ok();
    }

    [HttpGet("company-plans")]
    public async Task<IActionResult> GetCompanyPlansEnabled()
    {
        var enabled = await _configService.GetCompanyPlansEnabledAsync();
        return Ok(new { enabled });
    }

    [HttpPut("company-plans")]
    public async Task<IActionResult> SetCompanyPlansEnabled([FromBody] SetFeatureFlagDto dto)
    {
        await _configService.SetCompanyPlansEnabledAsync(dto.Enabled, AdminId);
        return Ok();
    }
}

public class SetFeatureFlagDto
{
    public bool Enabled { get; set; }
}
