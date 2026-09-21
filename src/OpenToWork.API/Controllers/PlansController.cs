using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.Enums;

namespace OpenToWork.API.Controllers;

/// <summary>Endpoint público de planes: "Mejora tu plan" en el portal corporativo (empresas) y en el
/// dashboard del candidato. Cada audiencia tiene su propio flag independiente: feature_company_plans_enabled
/// (encendido por defecto) y feature_candidate_priority_plan_enabled (apagado por defecto).</summary>
[ApiController]
[Route("api/[controller]")]
public class PlansController : ControllerBase
{
    private readonly ICompanyCrmService _crmService;
    private readonly ISystemConfigService _configService;

    public PlansController(ICompanyCrmService crmService, ISystemConfigService configService)
    {
        _crmService = crmService;
        _configService = configService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPlans([FromQuery] PlanAudience audience = PlanAudience.Company)
    {
        var enabled = audience == PlanAudience.Candidate
            ? await _configService.GetCandidatePriorityPlanEnabledAsync()
            : await _configService.GetCompanyPlansEnabledAsync();

        if (!enabled)
            return Ok(new List<object>());

        var result = await _crmService.GetPlansAsync(audience);
        return Ok(result);
    }

    [HttpGet("candidate-enabled")]
    public async Task<IActionResult> GetCandidatePlanEnabled()
    {
        var enabled = await _configService.GetCandidatePriorityPlanEnabledAsync();
        return Ok(new { enabled });
    }

    [HttpGet("company-enabled")]
    public async Task<IActionResult> GetCompanyPlanEnabled()
    {
        var enabled = await _configService.GetCompanyPlansEnabledAsync();
        return Ok(new { enabled });
    }
}
