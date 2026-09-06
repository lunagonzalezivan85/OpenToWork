using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.AdminAPI.Controllers;

/// <summary>
/// CRUD del banco de retos tecnicos. Ruta bajo api/admin/skill-tests (no api/skill-tests como
/// el plan literal) para no colisionar con las rutas candidate-facing en OpenToWork.API -
/// mismo criterio ya usado en 3.4 (api/admin/vacancies/{id}/matches).
/// </summary>
[Route("api/admin/skill-tests")]
public class SkillTestsController : AdminControllerBase
{
    private readonly ISkillTestService _skillTestService;

    public SkillTestsController(ISkillTestService skillTestService)
    {
        _skillTestService = skillTestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tests = await _skillTestService.GetAllSkillTestsAsync();
        return Ok(tests);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var test = await _skillTestService.GetSkillTestByIdAsync(id);
        return test == null ? NotFound() : Ok(test);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSkillTestDto dto)
    {
        var result = await _skillTestService.CreateSkillTestAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSkillTestDto dto)
    {
        var result = await _skillTestService.UpdateSkillTestAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _skillTestService.DeleteSkillTestAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
