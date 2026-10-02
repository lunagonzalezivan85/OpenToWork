namespace OpenToWork.Shared.DTOs;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int PrimaryRole { get; set; }
    public string? Identification { get; set; }
    public string? Phone { get; set; }

    // Solo candidato: obligatorios y validados en AuthService.RegisterAsync.
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    /// <summary>IdentityDocumentType; el numero va en Identification.</summary>
    public int? DocumentType { get; set; }

    /// <summary>Casilla obligatoria "He leido y acepto la politica de privacidad".</summary>
    public bool AcceptPrivacy { get; set; }
    /// <summary>Casilla opcional de comunicaciones comerciales (desmarcada por defecto).</summary>
    public bool AcceptMarketing { get; set; }

    /// <summary>Candidato: codigo recibido en el correo (POST api/auth/register/send-code). Sin el, no se crea la cuenta.</summary>
    public string? EmailCode { get; set; }
}

public class RegistrationCodeRequestDto
{
    public string Email { get; set; } = string.Empty;
    /// <summary>Opcional: solo para saludar por el nombre en el correo del codigo.</summary>
    public string? FirstName { get; set; }
}

public class VerifyEmailDto
{
    public string Code { get; set; } = string.Empty;
}

public class EmailVerificationStatusDto
{
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
}
