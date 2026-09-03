using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/users")]
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
}
