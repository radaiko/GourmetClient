using System.Security.Cryptography;
using System.Text;

namespace GC.Common;

public static class Crypto {
  private const int KeySize = 256;
  private const int BlockSize = 128;
  private const int SaltSize = 16;
  private const int Iterations = 10000;

  private static readonly string Passphrase = Base.DeviceKey ?? throw new InvalidOperationException("Device key is not set.");
  
  public static string? Encrypt(string? password) {
    if (string.IsNullOrEmpty(password)) return null;
    var salt = new byte[SaltSize];
    RandomNumberGenerator.Fill(salt);
    var key = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(Passphrase), salt, Iterations, HashAlgorithmName.SHA256, KeySize / 8);
    var iv = new byte[BlockSize / 8];
    RandomNumberGenerator.Fill(iv);
    byte[] encrypted;
    using (var aes = Aes.Create()) {
      aes.Key = key;
      aes.IV = iv;
      using (var encryptor = aes.CreateEncryptor())
      using (var ms = new MemoryStream()) {
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
          sw.Write(password);
        encrypted = ms.ToArray();
      }
    }
    var combined = new byte[SaltSize + iv.Length + encrypted.Length];
    Buffer.BlockCopy(salt, 0, combined, 0, SaltSize);
    Buffer.BlockCopy(iv, 0, combined, SaltSize, iv.Length);
    Buffer.BlockCopy(encrypted, 0, combined, SaltSize + iv.Length, encrypted.Length);
    return Convert.ToBase64String(combined);
  }

  public static string? Decrypt(string? encryptedPassword) {
    if (string.IsNullOrEmpty(encryptedPassword)) return null;
    var combined = Convert.FromBase64String(encryptedPassword);
    var salt = new byte[SaltSize];
    Buffer.BlockCopy(combined, 0, salt, 0, SaltSize);
    var iv = new byte[BlockSize / 8];
    Buffer.BlockCopy(combined, SaltSize, iv, 0, iv.Length);
    var encrypted = new byte[combined.Length - SaltSize - iv.Length];
    Buffer.BlockCopy(combined, SaltSize + iv.Length, encrypted, 0, encrypted.Length);
    var key = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(Passphrase), salt, Iterations, HashAlgorithmName.SHA256, KeySize / 8);
    using Aes aes = Aes.Create();
    aes.Key = key;
    aes.IV = iv;
    using var decryptor = aes.CreateDecryptor();
    using var ms = new MemoryStream(encrypted);
    using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
    using var sr = new StreamReader(cs);
    return sr.ReadToEnd();
  }
}