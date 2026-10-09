using System.Net;

namespace OpenToWork.Core.Services;

/// <summary>
/// HTML de los correos transaccionales. Hecho con tablas y estilos en linea (lo unico que respetan
/// Gmail, Outlook y Apple Mail) y sin imagenes: el logo es texto sobre fondo de color, asi se ve
/// aunque el cliente bloquee las imagenes. Colores de la marca (tema navy del portal).
/// </summary>
public static class EmailTemplates
{
    /// <summary>Correo con el codigo de 6 digitos para verificar el correo (registro o cuenta ya creada).</summary>
    public static string VerificationCode(string? firstName, string code, int validityMinutes)
    {
        var greeting = string.IsNullOrWhiteSpace(firstName) ? "Hola" : $"Hola, {WebUtility.HtmlEncode(firstName.Trim())}";

        return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<meta name=""color-scheme"" content=""light"">
<title>Tu código de verificación - Trato Directo</title>
</head>
<body style=""margin:0;padding:0;background-color:#F0F2F5;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;"">
    Tu código es {code}. Vence en {validityMinutes} minutos.
  </div>
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color:#F0F2F5;"">
    <tr>
      <td align=""center"" style=""padding:32px 16px;"">
        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""max-width:560px;"">
          <tr>
            <td align=""center"" style=""padding-bottom:24px;"">
              <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                <tr>
                  <td style=""background-color:#0B4F8A;border-radius:10px;width:40px;height:40px;text-align:center;vertical-align:middle;font-family:Arial,Helvetica,sans-serif;font-size:16px;font-weight:bold;color:#FFFFFF;"">TD</td>
                  <td style=""padding-left:10px;font-family:Arial,Helvetica,sans-serif;font-size:20px;font-weight:bold;color:#101828;letter-spacing:-0.3px;"">Trato Directo</td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#FFFFFF;border-radius:16px;border:1px solid #E4E7EC;"">
              <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                <tr>
                  <td style=""height:6px;background-color:#0B4F8A;border-radius:16px 16px 0 0;font-size:0;line-height:0;"">&nbsp;</td>
                </tr>
                <tr>
                  <td style=""padding:36px 40px 8px 40px;font-family:Arial,Helvetica,sans-serif;"">
                    <h1 style=""margin:0 0 12px 0;font-size:24px;line-height:30px;color:#101828;font-weight:bold;"">Verifica tu correo</h1>
                    <p style=""margin:0;font-size:15px;line-height:24px;color:#475467;"">
                      {greeting}, estás a un paso de completar tu cuenta en <strong style=""color:#101828;"">Trato Directo</strong>.
                      Escribe este código en la pantalla para confirmar que este correo es tuyo:
                    </p>
                  </td>
                </tr>
                <tr>
                  <td align=""center"" style=""padding:28px 40px;"">
                    <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color:#E8F0F7;border:1px dashed #0B4F8A;border-radius:12px;"">
                      <tr>
                        <td style=""padding:18px 32px;font-family:'Courier New',Courier,monospace;font-size:36px;line-height:40px;font-weight:bold;letter-spacing:10px;color:#0B4F8A;text-align:center;"">{code}</td>
                      </tr>
                    </table>
                    <p style=""margin:14px 0 0 0;font-family:Arial,Helvetica,sans-serif;font-size:13px;line-height:20px;color:#667085;"">
                      &#9201; Vence en <strong style=""color:#101828;"">{validityMinutes} minutos</strong>
                    </p>
                  </td>
                </tr>
                <tr>
                  <td style=""padding:0 40px 36px 40px;"">
                    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color:#F9FAFB;border-radius:10px;"">
                      <tr>
                        <td style=""padding:14px 16px;font-family:Arial,Helvetica,sans-serif;font-size:13px;line-height:20px;color:#475467;"">
                          <strong style=""color:#101828;"">¿No has sido tú?</strong> Ignora este correo: sin el código no se crea ninguna cuenta.
                          Nunca te pediremos este código por teléfono ni por WhatsApp.
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td align=""center"" style=""padding:24px 16px 0 16px;font-family:Arial,Helvetica,sans-serif;font-size:12px;line-height:18px;color:#98A2B3;"">
              Trato Directo<br>
              <a href=""https://tratodirecto.es"" style=""color:#0B4F8A;text-decoration:none;"">tratodirecto.es</a>
              &nbsp;&middot;&nbsp;
              <a href=""https://tratodirecto.es/privacy"" style=""color:#0B4F8A;text-decoration:none;"">Política de privacidad</a>
              <br><br>
              Recibes este correo porque se usó esta dirección para crear o verificar una cuenta.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    /// <summary>Aviso al titular cuando alguien pide un codigo de registro para un correo que ya
    /// tiene cuenta (anti-enumeracion, auditoria 08-Oct H-41): el endpoint responde igual que si
    /// hubiera enviado el codigo, y el titular es el unico que se entera.</summary>
    public static string RegistrationAttemptNotice()
    {
        return @"<!DOCTYPE html>
<html lang=""es"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<meta name=""color-scheme"" content=""light"">
<title>Intento de registro - Trato Directo</title>
</head>
<body style=""margin:0;padding:0;background-color:#F0F2F5;"">
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color:#F0F2F5;"">
    <tr>
      <td align=""center"" style=""padding:32px 16px;"">
        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""max-width:560px;"">
          <tr>
            <td align=""center"" style=""padding-bottom:24px;"">
              <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                <tr>
                  <td style=""background-color:#0B4F8A;border-radius:10px;width:40px;height:40px;text-align:center;vertical-align:middle;font-family:Arial,Helvetica,sans-serif;font-size:16px;font-weight:bold;color:#FFFFFF;"">TD</td>
                  <td style=""padding-left:10px;font-family:Arial,Helvetica,sans-serif;font-size:20px;font-weight:bold;color:#101828;letter-spacing:-0.3px;"">Trato Directo</td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#FFFFFF;border-radius:16px;border:1px solid #E4E7EC;padding:36px 40px;font-family:Arial,Helvetica,sans-serif;"">
              <h1 style=""margin:0 0 12px 0;font-size:22px;line-height:28px;color:#101828;font-weight:bold;"">Ya tienes una cuenta</h1>
              <p style=""margin:0 0 14px 0;font-size:15px;line-height:24px;color:#475467;"">
                Alguien pidió crear una cuenta en <strong style=""color:#101828;"">Trato Directo</strong> con este correo,
                pero ya existe una cuenta asociada. No se ha creado ni cambiado nada.
              </p>
              <p style=""margin:0 0 14px 0;font-size:15px;line-height:24px;color:#475467;"">
                Si fuiste tú, <a href=""https://tratodirecto.es/login"" style=""color:#0B4F8A;"">inicia sesión</a>
                o usa <a href=""https://tratodirecto.es/forgot-password"" style=""color:#0B4F8A;"">recuperar contraseña</a>.
              </p>
              <p style=""margin:0;font-size:13px;line-height:20px;color:#667085;"">
                <strong style=""color:#101828;"">¿No has sido tú?</strong> Ignora este correo: tu cuenta sigue segura.
              </p>
            </td>
          </tr>
          <tr>
            <td align=""center"" style=""padding:24px 16px 0 16px;font-family:Arial,Helvetica,sans-serif;font-size:12px;line-height:18px;color:#98A2B3;"">
              Trato Directo · <a href=""https://tratodirecto.es"" style=""color:#0B4F8A;text-decoration:none;"">tratodirecto.es</a>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    /// <summary>Aviso a la empresa cuando el equipo la marca como verificada en el admin
    /// (ciclo de confianza H-40: la verificacion es el gate de funciones sensibles).</summary>
    public static string CompanyVerified(string? companyName)
    {
        var name = WebUtility.HtmlEncode(companyName?.Trim() ?? "");

        return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<meta name=""color-scheme"" content=""light"">
<title>Empresa verificada - Trato Directo</title>
</head>
<body style=""margin:0;padding:0;background-color:#F0F2F5;"">
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color:#F0F2F5;"">
    <tr>
      <td align=""center"" style=""padding:32px 16px;"">
        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""max-width:560px;"">
          <tr>
            <td align=""center"" style=""padding-bottom:24px;"">
              <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                <tr>
                  <td style=""background-color:#0B4F8A;border-radius:10px;width:40px;height:40px;text-align:center;vertical-align:middle;font-family:Arial,Helvetica,sans-serif;font-size:16px;font-weight:bold;color:#FFFFFF;"">TD</td>
                  <td style=""padding-left:10px;font-family:Arial,Helvetica,sans-serif;font-size:20px;font-weight:bold;color:#101828;letter-spacing:-0.3px;"">Trato Directo</td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#FFFFFF;border-radius:16px;border:1px solid #E4E7EC;padding:36px 40px;font-family:Arial,Helvetica,sans-serif;"">
              <h1 style=""margin:0 0 12px 0;font-size:22px;line-height:28px;color:#101828;font-weight:bold;"">Tu empresa está verificada</h1>
              <p style=""margin:0 0 14px 0;font-size:15px;line-height:24px;color:#475467;"">
                El equipo de <strong style=""color:#101828;"">Trato Directo</strong> ha revisado
                <strong style=""color:#101828;"">{name}</strong> y ya aparece como empresa verificada.
                Con ello se desbloquean las funciones sensibles del portal cuando se publiquen.
              </p>
              <p style=""margin:0;font-size:13px;line-height:20px;color:#667085;"">
                Si no esperabas este correo o crees que es un error, escríbenos respondiendo a este mensaje.
              </p>
            </td>
          </tr>
          <tr>
            <td align=""center"" style=""padding:24px 16px 0 16px;font-family:Arial,Helvetica,sans-serif;font-size:12px;line-height:18px;color:#98A2B3;"">
              Trato Directo · <a href=""https://tratodirecto.es"" style=""color:#0B4F8A;text-decoration:none;"">tratodirecto.es</a>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }
}
