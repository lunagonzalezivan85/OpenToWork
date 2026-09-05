namespace OpenToWork.Shared.DTOs;

public class StaffUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public int StaffRole { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? PasswordExpiresAt { get; set; }
}

public class CreateStaffDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public int StaffRole { get; set; }
}

public class ChangeStaffRoleDto
{
    public int StaffRole { get; set; }
}

public class ResetStaffPasswordResultDto
{
    public string TempPassword { get; set; } = string.Empty;
    public DateTime PasswordExpiresAt { get; set; }
}
