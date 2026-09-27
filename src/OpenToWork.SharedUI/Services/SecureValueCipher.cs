using System.Security.Cryptography;
using System.Text;

namespace OpenToWork.SharedUI.Services;

/// <summary>Cifrado AES-256-GCM para valores sensibles que viajan a localStorage (tokens).
/// A diferencia del AesEncryptionService anterior (CBC con IV estatico derivado de la clave),
/// usa nonce aleatorio por mensaje + tag de autenticacion. Formato: "enc:" + base64(nonce|tag|cipher).
/// Valores sin prefijo se devuelven tal cual (ventana de migracion de sesiones existentes).</summary>
public class SecureValueCipher
{
    private const string EncPrefix = "enc:";
    private const int NonceSize = 12;
    private const int TagSize = 16;

    // null = sin cifrado (ver PassThrough)
    private readonly byte[]? _key;

    /// <param name="key">Clave maestra (Security:LocalStorageKey). Si es null/vacia se genera una
    /// efimera por arranque: las sesiones guardadas no sobreviven un reinicio pero nunca hay
    /// clave hardcodeada en el repo.</param>
    public SecureValueCipher(string? key)
    {
        _key = !string.IsNullOrWhiteSpace(key)
            ? SHA256.HashData(Encoding.UTF8.GetBytes(key))
            : RandomNumberGenerator.GetBytes(32);
    }

    private SecureValueCipher() { }

    /// <summary>Sin cifrado, para clientes WASM: AesGcm no existe en el navegador
    /// (PlatformNotSupportedException) y la clave tendria que viajar al propio navegador,
    /// asi que no protegeria nada. Los valores "enc:" de sesiones Blazor Server anteriores
    /// no se pueden descifrar y se devuelven vacios (el usuario vuelve a iniciar sesion).</summary>
    public static SecureValueCipher PassThrough() => new();

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText) || _key is null) return plainText;

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var result = new byte[NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, result, NonceSize + TagSize, cipherBytes.Length);
        return EncPrefix + Convert.ToBase64String(result);
    }

    public string Decrypt(string? stored)
    {
        if (string.IsNullOrEmpty(stored) || !stored.StartsWith(EncPrefix)) return stored ?? string.Empty;
        if (_key is null) return string.Empty;

        try
        {
            var bytes = Convert.FromBase64String(stored[EncPrefix.Length..]);
            if (bytes.Length <= NonceSize + TagSize) return string.Empty;

            var nonce = bytes[..NonceSize];
            var tag = bytes[NonceSize..(NonceSize + TagSize)];
            var cipherBytes = bytes[(NonceSize + TagSize)..];
            var plainBytes = new byte[cipherBytes.Length];

            using var aes = new AesGcm(_key, TagSize);
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (CryptographicException)
        {
            return string.Empty;
        }
        catch (FormatException)
        {
            return string.Empty;
        }
    }
}
