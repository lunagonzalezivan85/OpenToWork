using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class StaffService : IStaffService
{
    /// <summary>
    /// Sin SMTP configurado en el proyecto, el vencimiento de contraseña se resuelve con un
    /// reseteo admin-mediado (ResetPasswordAsync), no con un flujo de auto-servicio por email.
    /// </summary>
    private const int PasswordExpiryDays = 90;

    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;

    public StaffService(AppDbContext context, IAuditLogService auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<List<StaffUserDto>> GetStaffAsync()
    {
        return await _context.SC_Users
            .Where(u => u.PrimaryRole == (int)UserRole.Admin && !u.IsDeleted)
            .OrderBy(u => u.CreatedAt)
            .Select(u => new StaffUserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                StaffRole = u.StaffRole ?? (int)AdminStaffRole.SuperAdmin,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                PasswordExpiresAt = u.PasswordExpiresAt
            })
            .ToListAsync();
    }

    public async Task<StaffUserDto?> CreateStaffAsync(CreateStaffDto dto, Guid adminId, string? ipAddress)
    {
        if (!Enum.IsDefined(typeof(AdminStaffRole), dto.StaffRole)) return null;

        var exists = await _context.SC_Users.AnyAsync(u => u.Email == dto.Email && !u.IsDeleted);
        if (exists) return null;

        var passwordExpiresAt = DateTime.UtcNow.AddDays(PasswordExpiryDays);
        var user = new SCUser
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PrimaryRole = (int)UserRole.Admin,
            StaffRole = dto.StaffRole,
            FullName = dto.FullName,
            Phone = dto.Phone,
            PasswordExpiresAt = passwordExpiresAt,
            EmailVerified = true,
            IsActive = true,
            CreatedBy = adminId
        };
        user.UserRoles.Add(new SCUserRole { Role = (int)UserRole.Admin, SCUserId = user.Id, CreatedBy = adminId });

        _context.SC_Users.Add(user);
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "CreateStaffUser", "SC_Users", user.Id,
            $"{{\"email\":\"{user.Email}\",\"staffRole\":{user.StaffRole}}}", ipAddress);

        return new StaffUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            StaffRole = dto.StaffRole,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            PasswordExpiresAt = passwordExpiresAt
        };
    }

    public async Task<bool> ChangeStaffRoleAsync(Guid id, int newRole, Guid adminId, string? ipAddress)
    {
        if (!Enum.IsDefined(typeof(AdminStaffRole), newRole)) return false;

        var user = await _context.SC_Users
            .FirstOrDefaultAsync(u => u.Id == id && u.PrimaryRole == (int)UserRole.Admin && !u.IsDeleted);
        if (user == null) return false;

        var previousRole = user.StaffRole;
        if (previousRole == newRole) return true;

        user.StaffRole = newRole;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = adminId;
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "ChangeStaffRole", "SC_Users", id,
            $"{{\"from\":{previousRole?.ToString() ?? "null"},\"to\":{newRole}}}", ipAddress);
        return true;
    }

    public async Task<ResetStaffPasswordResultDto?> ResetPasswordAsync(Guid id, Guid adminId, string? ipAddress)
    {
        var user = await _context.SC_Users
            .FirstOrDefaultAsync(u => u.Id == id && u.PrimaryRole == (int)UserRole.Admin && !u.IsDeleted);
        if (user == null) return null;

        var tempPassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(6));
        var passwordExpiresAt = DateTime.UtcNow.AddDays(PasswordExpiryDays);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
        user.PasswordExpiresAt = passwordExpiresAt;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = adminId;
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "ResetStaffPassword", "SC_Users", id, null, ipAddress);

        return new ResetStaffPasswordResultDto
        {
            TempPassword = tempPassword,
            PasswordExpiresAt = passwordExpiresAt
        };
    }
}
