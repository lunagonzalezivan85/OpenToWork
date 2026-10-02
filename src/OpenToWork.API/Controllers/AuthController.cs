using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto, consentIp: HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("email-verification")]
    public async Task<IActionResult> GetEmailVerificationStatus()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var status = await _authService.GetEmailVerificationStatusAsync(userId.Value);
        return status != null ? Ok(status) : NotFound();
    }

    [Authorize]
    [EnableRateLimiting("auth")]
    [HttpPost("email-verification/send")]
    public async Task<IActionResult> SendEmailVerificationCode()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        return await _authService.SendEmailVerificationCodeAsync(userId.Value) switch
        {
            SendVerificationCodeResult.Sent or SendVerificationCodeResult.AlreadyVerified => NoContent(),
            SendVerificationCodeResult.TooSoon => StatusCode(StatusCodes.Status429TooManyRequests, new { message = "too_soon" }),
            SendVerificationCodeResult.SendFailed => StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "send_failed" }),
            _ => NotFound()
        };
    }

    // Rate limit por IP ademas del maximo de intentos por codigo: 6 digitos no aguantan fuerza bruta sin limites.
    [Authorize]
    [EnableRateLimiting("auth")]
    [HttpPost("email-verification/verify")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        return await _authService.VerifyEmailAsync(userId.Value, dto.Code) switch
        {
            EmailVerificationResult.Verified or EmailVerificationResult.AlreadyVerified => NoContent(),
            EmailVerificationResult.Invalid => BadRequest(new { message = "invalid" }),
            EmailVerificationResult.Expired => BadRequest(new { message = "expired" }),
            EmailVerificationResult.TooManyAttempts => BadRequest(new { message = "too_many_attempts" }),
            _ => NotFound()
        };
    }

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenDto dto)
    {
        await _authService.RevokeTokenAsync(dto.RefreshToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("check-device")]
    public async Task<IActionResult> CheckDevice([FromQuery] string deviceHash)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var isKnown = await _authService.IsDeviceKnownAsync(userId.Value, deviceHash);
        return Ok(new { isKnown, requiresCaptcha = !isKnown });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.RequestPasswordResetAsync(dto.Email);
        return Ok(new { message = "If the email exists, a reset link has been sent." });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
        return result ? Ok(new { message = "Password reset successfully." }) : BadRequest(new { message = "Invalid or expired token." });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        var result = await _authService.GoogleLoginAsync(dto.Token);
        return result != null ? Ok(result) : Unauthorized(new { message = "Invalid Google token." });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("verify-recaptcha")]
    public async Task<IActionResult> VerifyRecaptcha([FromBody] VerifyRecaptchaDto dto)
    {
        var isValid = await _authService.VerifyRecaptchaAsync(dto.Response);
        return Ok(new { success = isValid });
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
