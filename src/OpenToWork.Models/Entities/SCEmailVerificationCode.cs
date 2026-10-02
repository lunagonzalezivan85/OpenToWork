using System.ComponentModel.DataAnnotations;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Codigo enviado a un correo ANTES de que exista la cuenta (registro de candidato, decision de
/// Darwin 1-Oct: la cuenta no se crea hasta verificar el correo). Una fila por correo; se borra
/// al crear la cuenta. Para cuentas ya creadas se usa SCUser.EmailVerificationCodeHash.
/// </summary>
public class SCEmailVerificationCode : BaseEntity
{
    /// <summary>Correo en minusculas y sin espacios.</summary>
    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Hash del codigo de 6 digitos (con el correo como sal), nunca el codigo en claro.</summary>
    [Required]
    [MaxLength(256)]
    public string CodeHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime LastSentAt { get; set; }

    /// <summary>Intentos fallidos con el codigo vigente.</summary>
    public int Attempts { get; set; }
}
