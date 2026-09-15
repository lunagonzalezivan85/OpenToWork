using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;

namespace OpenToWork.API.Controllers;

/// <summary>Catalogo de skills (solo lectura) para que la empresa agregue requisitos adicionales
/// a los skills predeterminados del tipo de puesto al crear una vacante.</summary>
[ApiController]
[Route("api/skills")]
[Authorize]
public class SkillsController : ControllerBase
{
    private readonly IAdminSkillService _skillService;

    public SkillsController(IAdminSkillService skillService)
    {
        _skillService = skillService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSkills() => Ok(await _skillService.GetSkillsAsync());
}
