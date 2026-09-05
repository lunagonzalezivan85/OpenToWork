using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IStaffService
{
    Task<List<StaffUserDto>> GetStaffAsync();
    Task<StaffUserDto?> CreateStaffAsync(CreateStaffDto dto, Guid adminId, string? ipAddress);
    Task<bool> ChangeStaffRoleAsync(Guid id, int newRole, Guid adminId, string? ipAddress);
    Task<ResetStaffPasswordResultDto?> ResetPasswordAsync(Guid id, Guid adminId, string? ipAddress);
}
