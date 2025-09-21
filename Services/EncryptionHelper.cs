using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class EncryptionHelper
{
    // AES-256 requires a 32-byte (256-bit) key.
    // Use a strong, secret key in production; this is just an example.
    private static readonly string EncryptionKey = "12345678901234567890123456789012";

    /// <summary>
    /// Encrypts plain text using AES-256 with a random IV prepended to the ciphertext.
    /// Returns Base64-encoded result.
    /// </summary>
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
        aes.GenerateIV(); // Unique IV per encryption

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();

        // Write IV at the beginning of the output stream (needed for decryption)
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs, Encoding.UTF8))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Decrypts Base64-encoded ciphertext that includes the prepended IV.
    /// </summary>
    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        byte[] fullCipher;
        try
        {
            fullCipher = Convert.FromBase64String(cipherText);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Input is not a valid Base64 string.");
        }

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);

        var ivLength = aes.BlockSize / 8; // usually 16 bytes for AES

        if (fullCipher.Length < ivLength)
            throw new ArgumentException("Invalid cipher text.");

        var iv = new byte[ivLength];
        var cipher = new byte[fullCipher.Length - ivLength];

        Array.Copy(fullCipher, 0, iv, 0, ivLength);
        Array.Copy(fullCipher, ivLength, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipher);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs, Encoding.UTF8);

        return sr.ReadToEnd();
    }

    /// <summary>
    /// Attempts to safely decrypt a string. 
    /// If the string is not Base64 or decryption fails, returns original string.
    /// </summary>
    public static string SafeDecrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        if (!IsBase64String(cipherText))
            return cipherText;

        try
        {
            return Decrypt(cipherText);
        }
        catch (Exception ex)
        {
            // Optional: log exception somewhere here
            Console.WriteLine($"[Decryption Failed] Input: {cipherText}, Error: {ex.Message}");
            return cipherText;
        }
    }

    /// <summary>
    /// Checks if a string is valid Base64.
    /// </summary>
    private static bool IsBase64String(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        // Base64 length must be multiple of 4
        if (input.Length % 4 != 0)
            return false;

        Span<byte> buffer = new byte[input.Length];
        return Convert.TryFromBase64String(input, buffer, out _);
    }
}
