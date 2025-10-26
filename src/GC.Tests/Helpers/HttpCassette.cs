using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GC.Tests.Helpers;

/// <summary>
/// Generic HttpCassette for tests (record/replay HTTP interactions).
/// This is the consolidated implementation used by unit tests. Place cassette files under a test-recordings folder and commit them for hermetic tests.
/// </summary>
public class HttpCassette {
  private readonly JsonSerializerOptions _json = new() { WriteIndented = true };

  private HttpCassette(string cassetteDirectory, HttpMessageHandler? innerHandler = null, CookieContainer? cookieContainer = null) {
    if (string.IsNullOrWhiteSpace(cassetteDirectory)) throw new ArgumentNullException(nameof(cassetteDirectory));
    Directory.CreateDirectory(cassetteDirectory);
    var cookieContainer1 = cookieContainer;

    // Flags: environment variables override explicit args when provided
    var envRecord = Environment.GetEnvironmentVariable("RECORD");
    var forceRecord = string.Equals(envRecord, "true", StringComparison.OrdinalIgnoreCase);
    var replayOnly = string.Equals(envRecord, "false", StringComparison.OrdinalIgnoreCase);

    var inner = innerHandler ?? new HttpClientHandler { CookieContainer = cookieContainer ?? new CookieContainer(), AllowAutoRedirect = true };
    Handler = new CassetteHandler(cassetteDirectory, inner, cookieContainer1, forceRecord, replayOnly, _json);
  }

  private HttpMessageHandler Handler { get; }

  public static HttpClient CreateHttpClient([CallerFilePath] string callerFilePath = "Unknown", [CallerMemberName] string callerMemberName = "Unknown", TimeSpan? timeout = null) {
    var cassetteSubfolder = Path.Combine(Path.GetFileNameWithoutExtension(callerFilePath), callerMemberName);
    
    // find repo root by going up 3 directories
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    for (int i = 0; i < 3 && dir != null; i++) {
      dir = dir.Parent;
    }
    var baseDir = dir?.FullName ?? AppContext.BaseDirectory;
    var cassetteDir = Path.Combine(baseDir, "test-recordings", cassetteSubfolder);
    Directory.CreateDirectory(cassetteDir);
  
    var cookieContainer = new CookieContainer();
    var cassette = new HttpCassette(cassetteDir, innerHandler: new HttpClientHandler { CookieContainer = cookieContainer, AllowAutoRedirect = true }, cookieContainer: cookieContainer);
    var client = new HttpClient(cassette.Handler) { Timeout = timeout ?? TimeSpan.FromSeconds(30) };
    return client;
  }

  private class CassetteHandler(string dir, HttpMessageHandler inner, CookieContainer? cookieContainer, bool forceRecord, bool replayOnly, JsonSerializerOptions json)
    : DelegatingHandler(inner) {
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
      var key = ComputeKey(request);
      var file = Path.Combine(dir, key + ".blob");

      if (File.Exists(file) && !forceRecord) {
        // Read raw bytes so we can support optionally-encrypted cassette files.
        var fileBytes = await File.ReadAllBytesAsync(file, cancellationToken).ConfigureAwait(false);
        byte[] plainBytes;
        try {
          plainBytes = TryDecryptIfNeeded(fileBytes);
        }
        catch (Exception ex) {
          throw new InvalidOperationException($"Failed to decrypt cassette file '{file}': {ex.Message}", ex);
        }

        var readAllTextAsync = Encoding.UTF8.GetString(plainBytes);
        var record = JsonSerializer.Deserialize<ResponseRecord>(readAllTextAsync)!;
        var response = new HttpResponseMessage((HttpStatusCode)record.StatusCode) {
          ReasonPhrase = record.ReasonPhrase,
          RequestMessage = request,
          Content = new ByteArrayContent(Convert.FromBase64String(record.ContentBase64 ?? string.Empty))
        };

        if (record.Headers != null) {
          foreach (var kv in record.Headers.Where(kv => !response.Headers.TryAddWithoutValidation(kv.Key, kv.Value))) {
            response.Content.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
          }
        }

        if (cookieContainer != null && record.Headers != null) {
          if (record.Headers.TryGetValue("Set-Cookie", out var setCookies) || record.Headers.TryGetValue("set-cookie", out setCookies)) {
            try {
              var baseUri = new Uri(request.RequestUri?.GetLeftPart(UriPartial.Authority) ?? "http://localhost");
              foreach (var sc in setCookies) {
                cookieContainer.SetCookies(baseUri, sc);
              }
            }
            catch {
              // ignore cookie parse issues
            }
          }
        }

        return response;
      }

      if (replayOnly && !File.Exists(file)) {
        throw new InvalidOperationException($"Replay-only mode is enabled but no recording exists for request: {request.RequestUri}");
      }

      // Acquire the real response and ensure it (and its Content) are disposed even if later processing throws.
      using var realResponse = await base.SendAsync(CloneRequest(request), cancellationToken).ConfigureAwait(false);
      var bytes = await realResponse.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

      var recordToSave = new ResponseRecord {
        StatusCode = (int)realResponse.StatusCode,
        ReasonPhrase = realResponse.ReasonPhrase,
        Headers = realResponse.Headers.Concat(realResponse.Content.Headers)
                  .ToDictionary(k => k.Key, v => v.Value.ToArray()),
        ContentBase64 = bytes.Length == 0 ? string.Empty : Convert.ToBase64String(bytes)
      };

      var serialized = JsonSerializer.Serialize(recordToSave, json);
      // Ensure target directory exists (guard if key accidentally contains subfolders)
      try {
        var targetDir = Path.GetDirectoryName(file) ?? dir;
        Directory.CreateDirectory(targetDir);
      }
      catch {
        // if creating directory fails, we'll let the File write call surface the error
      }

      // Write either plaintext JSON or encrypted bytes depending on environment.
      var plainOut = Encoding.UTF8.GetBytes(serialized);
      var outBytes = EncryptIfNeeded(plainOut);
      // Write to a temp file in the same directory then atomically replace the target file.
      var writeDir = Path.GetDirectoryName(file) ?? dir;
      var tempPath = Path.Combine(writeDir, Path.GetRandomFileName());
      try {
        await File.WriteAllBytesAsync(tempPath, outBytes, cancellationToken).ConfigureAwait(false);
        // If the destination already exists, use File.Replace to perform an atomic swap where supported.
        if (File.Exists(file)) {
          File.Replace(tempPath, file, null);
        }
        else {
          // Move into place. This will atomically move on the same filesystem.
          File.Move(tempPath, file);
        }
      }
      catch {
        try {
          if (File.Exists(tempPath)) File.Delete(tempPath);
        }
        catch (Exception ex) {
          // Log the cleanup failure to aid debugging — do not throw from cleanup.
          System.Diagnostics.Debug.WriteLine($"HttpCassette: failed to delete temp file '{tempPath}': {ex.Message}");
        }
        throw;
      }

      var replayResponse = new HttpResponseMessage(realResponse.StatusCode) {
        ReasonPhrase = realResponse.ReasonPhrase,
        RequestMessage = request,
        Content = new ByteArrayContent(bytes)
      };

      foreach (var kv in recordToSave.Headers.Where(kv => !replayResponse.Headers.TryAddWithoutValidation(kv.Key, kv.Value))) {
        replayResponse.Content.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
      }

      return replayResponse;
    }

    private static string ComputeKey(HttpRequestMessage request) {
      var uri = request.RequestUri?.AbsoluteUri ?? string.Empty;
      var method = request.Method.Method;
      string bodyHash = string.Empty;
      if (request.Content != null) {
        var bytes = request.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        // Prefer the static API over creating an instance
        var hash = SHA256.HashData(bytes);
        // Use the dedicated Convert API that produces lowercase hex, then truncate.
        // Use AsSpan and ToString to get the first 16 chars; avoids Substring call chain and is explicit.
        bodyHash = "-" + Convert.ToHexStringLower(hash).AsSpan(0, 16).ToString();
      }

      var raw = method + "_" + uri + bodyHash;
      foreach (var c in Path.GetInvalidFileNameChars()) raw = raw.Replace(c, '_');
      if (raw.Length > 200) raw = raw.Substring(0, 200);
      // Use URL-safe Base64 (replace '+' and '/' and remove padding '=') to avoid creating nested paths on Unix
      var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
      var safe = base64.Replace("+", "-").Replace("/", "_").Replace("=", "");
      // Additionally replace any directory separator characters just in case
      safe = safe.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');
      // As a final guard, remove any leftover '/' or '\\' characters
      safe = safe.Replace('/', '_').Replace('\\', '_');
      if (string.IsNullOrEmpty(safe)) safe = "request";
      // Truncate to a reasonable filename length
      if (safe.Length > 255) safe = safe.Substring(0, 255);
      return safe;
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage req) {
      var clone = new HttpRequestMessage(req.Method, req.RequestUri) {
        Version = req.Version
      };
      // Preserve VersionPolicy so the cloned request retains original HTTP version negotiation behavior.
      // req is a required argument; assign directly to preserve semantics on newer runtimes.
      clone.VersionPolicy = req.VersionPolicy;
      if (req.Content != null) {
        var bytes = req.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        clone.Content = new ByteArrayContent(bytes);
        foreach (var h in req.Content.Headers) clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
      }
      foreach (var h in req.Headers) clone.Headers.TryAddWithoutValidation(h.Key, h.Value);
      return clone;
    }

    // Encryption format when RECORD_ENCRYPTION_KEY is set:
    // [MAGIC 4 bytes 'ENC1'][nonce 12 bytes][tag 16 bytes][ciphertext...]
    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("ENC1");
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private static byte[]? GetEncryptionKeyFromEnv() {
      var env = Environment.GetEnvironmentVariable("RECORD_ENCRYPTION_KEY");
      if (string.IsNullOrEmpty(env)) return null;
      // Derive a 32-byte key from the provided string using SHA256 (simple, deterministic).
      // Prefer the static API over creating an SHA256 instance
      var key = SHA256.HashData(Encoding.UTF8.GetBytes(env));
      return key;
    }

    private static byte[] EncryptIfNeeded(byte[] plain) {
      var key = GetEncryptionKeyFromEnv();
      if (key == null) return plain; // no encryption requested

      var nonce = RandomNumberGenerator.GetBytes(NonceSize);
      var ciphertext = new byte[plain.Length];
      var tag = new byte[TagSize];
      // Use the AesGcm constructor that specifies tag size to satisfy newer APIs.
      using (var aes = new AesGcm(key, tagSizeInBytes: TagSize)) {
        aes.Encrypt(nonce, plain, ciphertext, tag);
      }

      var outBytes = new byte[Magic.Length + NonceSize + TagSize + ciphertext.Length];
      Buffer.BlockCopy(Magic, 0, outBytes, 0, Magic.Length);
      Buffer.BlockCopy(nonce, 0, outBytes, Magic.Length, NonceSize);
      Buffer.BlockCopy(tag, 0, outBytes, Magic.Length + NonceSize, TagSize);
      Buffer.BlockCopy(ciphertext, 0, outBytes, Magic.Length + NonceSize + TagSize, ciphertext.Length);
      return outBytes;
    }

    private static byte[] TryDecryptIfNeeded(byte[] fileBytes) {
      if (fileBytes.Length >= Magic.Length) {
        bool hasMagic = true;
        for (int i = 0; i < Magic.Length; i++) {
          if (fileBytes[i] != Magic[i]) { hasMagic = false; break; }
        }
        if (!hasMagic) {
          // Not encrypted, assume UTF8 JSON text
          return fileBytes;
        }
      } else {
        return fileBytes; // too small to be encrypted
      }

      var key = GetEncryptionKeyFromEnv();
      if (key == null) throw new InvalidOperationException("Cassette file is encrypted but RECORD_ENCRYPTION_KEY is not set.");

      var offset = Magic.Length;
      if (fileBytes.Length < offset + NonceSize + TagSize) throw new InvalidOperationException("Encrypted cassette file is malformed.");

      var nonce = new byte[NonceSize];
      Buffer.BlockCopy(fileBytes, offset, nonce, 0, NonceSize);
      offset += NonceSize;
      var tag = new byte[TagSize];
      Buffer.BlockCopy(fileBytes, offset, tag, 0, TagSize);
      offset += TagSize;
      var ciphertextLen = fileBytes.Length - offset;
      var ciphertext = new byte[ciphertextLen];
      Buffer.BlockCopy(fileBytes, offset, ciphertext, 0, ciphertextLen);

      var plain = new byte[ciphertextLen];
      try {
        using var aes = new AesGcm(key, tagSizeInBytes: TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plain);
      }
      catch (Exception ex) {
        throw new CryptographicException("Failed to decrypt cassette payload.", ex);
      }

      return plain;
    }

    private class ResponseRecord {
      public int StatusCode { get; init; }
      public string? ReasonPhrase { get; init; }
      public Dictionary<string, string[]>? Headers { get; init; }
      public string? ContentBase64 { get; init; }
    }
  }
}
