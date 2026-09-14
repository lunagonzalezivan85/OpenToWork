using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/users")]
[RequireStaffRole(AdminStaffRole.SuperAdmin, AdminStaffRole.Reclutador, AdminStaffRole.Comercial)]
public class UsersController : AdminControllerBase
{
    private readonly IAdminUserService _userService;

    public UsersController(IAdminUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? role = null, [FromQuery] bool? isActive = null)
    {
        var users = await _userService.GetUsersAsync(page, pageSize, role, isActive);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpGet("{id}/profile")]
    public async Task<IActionResult> GetUserProfile(Guid id)
    {
        var profile = await _userService.GetUserProfileAsync(id);
        return profile == null ? NotFound() : Ok(profile);
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
        if (id == AdminId) return Conflict(new { message = "You cannot deactivate your own account." });

        var result = await _userService.DeactivateAsync(id, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == AdminId) return Conflict(new { message = "You cannot delete your own account." });

        var result = await _userService.DeleteAsync(id, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeRoleDto dto)
    {
        if (id == AdminId) return Conflict(new { message = "You cannot change your own role." });
        if (!Enum.IsDefined(typeof(OpenToWork.Shared.Enums.UserRole), dto.Role))
            return BadRequest(new { message = "Invalid role." });

        var result = await _userService.ChangeRoleAsync(id, dto.Role, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            return BadRequest(new { message = "Email y contraseña son obligatorios" });
        if (dto.Password.Length < 6)
            return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });
        if (!Enum.IsDefined(typeof(OpenToWork.Shared.Enums.UserRole), dto.PrimaryRole))
            return BadRequest(new { message = "Rol inválido" });

        var result = await _userService.CreateUserAsync(dto, AdminId, ClientIp);
        if (result == null)
            return BadRequest(new { message = "No se pudo crear el usuario (email ya registrado)" });
        return Ok(result);
    }
}
