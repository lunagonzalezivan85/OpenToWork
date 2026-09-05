using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/staff")]
[RequireStaffRole]
public class StaffController : AdminControllerBase
{
    private readonly IStaffService _staffService;
    private readonly IAdminUserService _userService;

    public StaffController(IStaffService staffService, IAdminUserService userService)
    {
        _staffService = staffService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStaff()
    {
        var result = await _staffService.GetStaffAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffDto dto)
    {
        if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            return BadRequest(new { message = "Email y contraseña son obligatorios" });
        if (dto.Password.Length < 6)
            return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });

        var result = await _staffService.CreateStaffAsync(dto, AdminId, ClientIp);
        if (result == null)
            return BadRequest(new { message = "No se pudo crear el usuario (email ya registrado o rol inválido)" });
        return Ok(result);
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> ChangeStaffRole(Guid id, [FromBody] ChangeStaffRoleDto dto)
    {
        var result = await _staffService.ChangeStaffRoleAsync(id, dto.StaffRole, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPut("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _userService.ActivateAsync(id, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _userService.DeactivateAsync(id, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id)
    {
        var result = await _staffService.ResetPasswordAsync(id, AdminId, ClientIp);
        return result == null ? NotFound() : Ok(result);
    }
}
