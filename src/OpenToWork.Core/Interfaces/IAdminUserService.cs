using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IAdminUserService
{
    Task<List<AdminUserDto>> GetUsersAsync(int page, int pageSize, int? role, bool? isActive);
    Task<AdminUserDto?> GetUserByIdAsync(Guid id);
    Task<AdminUserProfileDto?> GetUserProfileAsync(Guid id);
    Task<bool> ActivateAsync(Guid id, Guid adminId, string? ipAddress);
    Task<bool> DeactivateAsync(Guid id, Guid adminId, string? ipAddress);
    Task<bool> DeleteAsync(Guid id, Guid adminId, string? ipAddress);
}
