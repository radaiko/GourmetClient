using System;
using System.Net;
using System.Net.Http;

namespace GC.Core.WebApis;

public class BaseApi {
  // Shared CookieContainer and HttpClient for session persistence
  public static readonly CookieContainer CookieContainer = new();
  private static readonly HttpClientHandler InnerHandler = new() { CookieContainer = CookieContainer, AllowAutoRedirect = true };
  private static readonly ErrorHandlingHandler ErrorHandler = new() { InnerHandler = InnerHandler };
  public static HttpClient Client = new(ErrorHandler, disposeHandler: false) {
    Timeout = TimeSpan.FromSeconds(15)
  };
  
  // Public helpers for tests to override the client
  public static void SetHttpClient(HttpClient client) {
    Client = client ?? throw new ArgumentNullException(nameof(client));
  }
}