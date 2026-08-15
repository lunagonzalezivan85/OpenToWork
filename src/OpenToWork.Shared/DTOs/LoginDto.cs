using System.ComponentModel.DataAnnotations;

namespace OpenToWork.Shared.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? DeviceHash { get; set; }
    public string? DeviceName { get; set; }
    public string? RecaptchaToken { get; set; }
}
