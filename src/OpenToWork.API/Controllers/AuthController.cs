using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IGoogleTokenValidator _google;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;

    public AuthController(IAuthService authService, IGoogleTokenValidator google, IMemoryCache cache, IConfiguration config)
    {
        _authService = authService;
        _google = google;
        _cache = cache;
        _config = config;
    }

    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto, consentIp: HttpContext.Connection.RemoteIpAddress?.ToString());
            SetRefreshCookie(result.RefreshToken);
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

    /// <summary>Registro de candidato, paso 1: envia el codigo al correo. La cuenta se crea en POST register con ese codigo.</summary>
    [EnableRateLimiting("auth")]
    [HttpPost("register/send-code")]
    public async Task<IActionResult> SendRegistrationCode([FromBody] RegistrationCodeRequestDto dto)
    {
        return await _authService.SendRegistrationCodeAsync(dto.Email, dto.FirstName, dto.RecaptchaToken) switch
        {
            SendVerificationCodeResult.Sent => NoContent(),
            SendVerificationCodeResult.EmailAlreadyRegistered => Conflict(new { message = "email_exists" }),
            SendVerificationCodeResult.InvalidEmail => BadRequest(new { message = "invalid_email" }),
            SendVerificationCodeResult.CaptchaFailed => BadRequest(new { message = "captcha_required" }),
            SendVerificationCodeResult.TooSoon => StatusCode(StatusCodes.Status429TooManyRequests, new { message = "too_soon" }),
            _ => StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "send_failed" })
        };
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
            SetRefreshCookie(result.RefreshToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshTokenDto? dto)
    {
        // El refresh puede venir en el cuerpo (clientes antiguos) o en la cookie HttpOnly
        // td_refresh (portal WASM desde Fase 1 de seguridad, auditoria 08-Oct H-04). Ojo:
        // un cuerpo "{}" trae RefreshToken="" (no null) - hay que mirar vacio, no solo null.
        var refreshToken = string.IsNullOrEmpty(dto?.RefreshToken) ? Request.Cookies[RefreshCookieName] : dto!.RefreshToken;
        if (string.IsNullOrEmpty(refreshToken)) return Unauthorized();

        try
        {
            var result = await _authService.RefreshTokenAsync(new RefreshTokenDto { RefreshToken = refreshToken });
            SetRefreshCookie(result.RefreshToken); // rotacion: renueva token y cookie a la vez
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshTokenDto? dto)
    {
        var refreshToken = string.IsNullOrEmpty(dto?.RefreshToken) ? Request.Cookies[RefreshCookieName] : dto!.RefreshToken;
        if (!string.IsNullOrEmpty(refreshToken))
            await _authService.RevokeTokenAsync(refreshToken);
        DeleteRefreshCookie();
        return NoContent();
    }

    /// <summary>"Cerrar sesion en todos los dispositivos": revoca todos los refresh tokens del
    /// usuario autenticado (este incluido - borra tambien la cookie actual).</summary>
    [Authorize]
    [HttpPost("revoke-all")]
    public async Task<IActionResult> RevokeAll()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var revoked = await _authService.RevokeAllTokensAsync(userId);
        DeleteRefreshCookie();
        return Ok(new { revoked });
    }

    // --- Refresh token como cookie HttpOnly (auditoria 08-Oct H-04: antes vivia en localStorage,
    // al alcance de cualquier XSS). Path /api/auth: solo viaja a estos endpoints. SameSite=Lax
    // basta: en dev el portal y la API comparten sitio (localhost) y en prod comparten dominio
    // (tratodirecto.es y tratodirecto.es/api); en ambos casos la cookie acompana las llamadas XHR. ---
    private const string RefreshCookieName = "td_refresh";

    private void SetRefreshCookie(string refreshToken)
    {
        var days = _config.GetValue<int>("Jwt:RefreshTokenExpireDays", 7);
        Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(days)
        });
    }

    private void DeleteRefreshCookie() =>
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/auth" });

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

    // --- Entrar / registrarse con Google (solo candidatos), flujo por redireccion ---
    // start -> Google -> callback -> portal /auth/google con un codigo de un solo uso (nunca el JWT en la URL).
    // ponytail: codigos en IMemoryCache, vale con una sola instancia de la API; si se reinicia a mitad, el
    // candidato vuelve a pulsar el boton. Con varias instancias habria que pasarlos a la base de datos.

    private const string GoogleStateCookie = "td_google_state";

    [HttpGet("google/enabled")]
    public IActionResult GoogleEnabled() => Ok(new { enabled = _google.IsEnabled });

    [EnableRateLimiting("auth")]
    [HttpGet("google/start")]
    public IActionResult GoogleStart([FromQuery] string? returnUrl)
    {
        if (!_google.IsEnabled) return RedirectToPortal("error=unavailable", null);

        var state = NewOneTimeCode();
        _cache.Set("google-state:" + state, SafeReturnUrl(returnUrl) ?? "", TimeSpan.FromMinutes(10));
        // La cookie ata el state a este navegador: sin ella, alguien podria hacer que otro entrara con su cuenta.
        Response.Cookies.Append(GoogleStateCookie, state, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth/google",
            MaxAge = TimeSpan.FromMinutes(10)
        });
        return Redirect(_google.BuildAuthorizeUrl(GoogleRedirectUri, state));
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error)
    {
        var cookieState = Request.Cookies[GoogleStateCookie];
        Response.Cookies.Delete(GoogleStateCookie, new CookieOptions { Path = "/api/auth/google" });

        if (string.IsNullOrEmpty(state) || state != cookieState || !_cache.TryGetValue("google-state:" + state, out string? returnUrl))
            return RedirectToPortal("error=failed", null);
        _cache.Remove("google-state:" + state);

        if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
            return RedirectToPortal(error == "access_denied" ? "error=cancelled" : "error=failed", returnUrl);

        var identity = await _google.ExchangeCodeAsync(code, GoogleRedirectUri);
        if (identity == null) return RedirectToPortal("error=failed", returnUrl);

        var (status, auth) = await _authService.GoogleSignInAsync(identity);
        switch (status)
        {
            case GoogleSignInStatus.SignedIn:
                var loginCode = NewOneTimeCode();
                _cache.Set("google-login:" + loginCode, auth, TimeSpan.FromMinutes(2));
                return RedirectToPortal("login=" + loginCode, returnUrl);
            case GoogleSignInStatus.NeedsSignup:
                var ticket = NewOneTimeCode();
                _cache.Set("google-signup:" + ticket, identity, TimeSpan.FromMinutes(30));
                return RedirectToPortal("signup=" + ticket, returnUrl);
            case GoogleSignInStatus.NotCandidate:
                return RedirectToPortal("error=not_candidate", returnUrl);
            case GoogleSignInStatus.Inactive:
                return RedirectToPortal("error=inactive", returnUrl);
            default:
                return RedirectToPortal("error=email_not_verified", returnUrl);
        }
    }

    [EnableRateLimiting("auth")]
    [HttpPost("google/exchange")]
    public IActionResult GoogleExchange([FromBody] GoogleExchangeDto dto)
    {
        var key = "google-login:" + dto.Code;
        if (string.IsNullOrEmpty(dto.Code) || !_cache.TryGetValue(key, out AuthResponseDto? auth)) return Unauthorized();
        _cache.Remove(key);
        SetRefreshCookie(auth!.RefreshToken);
        return Ok(auth);
    }

    [HttpGet("google/signup/{ticket}")]
    public IActionResult GoogleSignupInfo(string ticket)
    {
        if (!_cache.TryGetValue("google-signup:" + ticket, out GoogleIdentity? identity) || identity == null) return NotFound();
        return Ok(new GoogleSignupInfoDto { Email = identity.Email, FirstName = identity.GivenName, LastName = identity.FamilyName });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("google/signup")]
    public async Task<IActionResult> GoogleSignup([FromBody] GoogleSignupDto dto)
    {
        var key = "google-signup:" + dto.Ticket;
        if (string.IsNullOrEmpty(dto.Ticket) || !_cache.TryGetValue(key, out GoogleIdentity? identity) || identity == null)
            return NotFound(new { message = "expired" });

        try
        {
            var result = await _authService.RegisterAsync(dto, consentIp: HttpContext.Connection.RemoteIpAddress?.ToString(), google: identity);
            _cache.Remove(key);
            SetRefreshCookie(result.RefreshToken);
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

    // En produccion API y portal comparten dominio (tratodirecto.es/api), pero detras del proxy el Host que
    // llega puede ser 127.0.0.1: por eso se puede fijar con GoogleOAuth:RedirectUri (el mismo que en Google Cloud).
    private string GoogleRedirectUri => _config["GoogleOAuth:RedirectUri"] is { Length: > 0 } configured
        ? configured
        : $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/auth/google/callback";

    private IActionResult RedirectToPortal(string query, string? returnUrl)
    {
        var portal = (_config["Portal:BaseUrl"] ?? "http://localhost:5100/").TrimEnd('/');
        var url = $"{portal}/auth/google?{query}";
        if (!string.IsNullOrEmpty(returnUrl)) url += "&returnUrl=" + Uri.EscapeDataString(returnUrl);
        return Redirect(url);
    }

    // Solo rutas internas del portal, igual que returnUrl en login/registro.
    private static string? SafeReturnUrl(string? url) =>
        !string.IsNullOrEmpty(url) && url.StartsWith('/') && !url.StartsWith("//") && !url.StartsWith("/\\") ? url : null;

    private static string NewOneTimeCode() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

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
