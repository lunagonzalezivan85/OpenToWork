namespace OpenToWork.Shared.DTOs;

public class SmtpSettingsDto
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;

    /// <summary>Nunca se devuelve el valor real guardado; queda vacio al leer. Si al guardar
    /// llega vacio, se conserva la contrasena existente (no se sobreescribe).</summary>
    public string Password { get; set; } = string.Empty;

    public bool UseSsl { get; set; } = true;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Trato Directo";
    public bool Enabled { get; set; } = false;
}

public class SendTestEmailDto
{
    public string ToEmail { get; set; } = string.Empty;
}
