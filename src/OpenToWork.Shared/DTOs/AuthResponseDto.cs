namespace OpenToWork.Shared.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public int PrimaryRole { get; set; }
    public int? StaffRole { get; set; }
    public string? Identification { get; set; }
    public string? Phone { get; set; }
    public bool EmailVerified { get; set; }
    public bool IsActive { get; set; }
    public List<int> Roles { get; set; } = new();
    public string? Theme { get; set; }
    public string? Language { get; set; }
    public int? PreferredRole { get; set; }
    public bool WizardCompleted { get; set; }
    public int WizardStep { get; set; }
}
