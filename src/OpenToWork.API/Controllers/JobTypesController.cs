using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;

namespace OpenToWork.API.Controllers;

/// <summary>Catalogo de tipos de puesto (solo lectura) para el selector de "Nueva Vacante" en el
/// portal de empresa - cada opcion trae sus skills predeterminados.</summary>
[ApiController]
[Route("api/job-types")]
[Authorize]
public class JobTypesController : ControllerBase
{
    private readonly IJobPricingService _pricing;

    public JobTypesController(IJobPricingService pricing)
    {
        _pricing = pricing;
    }

    [HttpGet]
    public async Task<IActionResult> GetJobTypes() => Ok(await _pricing.GetActiveJobTypeOptionsAsync());
}
