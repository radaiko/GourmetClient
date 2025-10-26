using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GC.Common;
using GC.Models;
using HtmlAgilityPack;
using ErrorEventArgs = GC.Common.ErrorEventArgs;

namespace GC.Core.WebApis;

public static class GourmetApi {
  private const string WebUrl = "https://alaclickneu.gourmet.at/";
  private const string StartPage = "start";
  private static bool _isLoggedIn;

  
  public static void Logout() {
    Log.Info("GourmetApi.Logout called");
    foreach (Cookie cookie in BaseApi.CookieContainer.GetAllCookies())
    {
      cookie.Expired = true;
    }
    _isLoggedIn = false;
    Log.Info("GourmetApi: Logged out and cookies expired");
  }
  
  public static async Task<bool> LoginAsync() {
    Base.OnInfo?.Invoke(null, new InfoEventArgs(InfoType.GourmetApi, "Starting login"));
    if (_isLoggedIn) {
      Base.OnInfo?.Invoke(null, new InfoEventArgs(InfoType.GourmetApi, "Already logged in"));
      Log.Debug("Already logged in, skipping login");
      return true;
    }
    var username = Settings.It.GourmetUsername;
    var password = Settings.It.GourmetPassword;
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) {
      Base.OnError?.Invoke(null, new ErrorEventArgs(ErrorType.GourmetApi, null, "Gourmet username or password not set in settings."));
      return false;
    }
    try {
      // 1. Check if already logged in
      // Construct the start page URI in a robust way instead of concatenating path separators
      var startUri = new Uri(new Uri(WebUrl), StartPage + "/");
      Log.Debug($"Requesting start page to check login status: {startUri}");
      var startPageResponse = await BaseApi.Client.GetAsync(startUri);
      var startPageHtml = await startPageResponse.Content.ReadAsStringAsync();
      if (startPageHtml.Contains("navbar-link") && startPageHtml.Contains("einstellungen")) {
        _isLoggedIn = true;
        Base.OnInfo?.Invoke(null, new InfoEventArgs(InfoType.GourmetApi, "Already logged in"));
        return true;
      }
      // 2. Get login page and extract ufprt
      Log.Debug("Fetching login page to extract ufprt");
      // Reuse the previously constructed startUri for the login page request
      var loginPageResponse = await BaseApi.Client.GetAsync(startUri);
      var loginPageHtml = await loginPageResponse.Content.ReadAsStringAsync();
      var doc = new HtmlDocument();
      doc.LoadHtml(loginPageHtml);
      var ufprtNode = doc.DocumentNode.SelectSingleNode("//input[@name='ufprt']");
      var ufprt = ufprtNode.GetAttributeValue("value", "");
      // 3. Prepare form data
      var formData = new Dictionary<string, string> {
        { "Username", username },
        { "Password", password },
        { "RememberMe", "true" },
        { "ufprt", ufprt }
      };
      // 4. Post login form
      Log.Debug("Posting login form");
      var response = await BaseApi.Client.PostAsync(WebUrl, new FormUrlEncodedContent(formData));
      var responseHtml = await response.Content.ReadAsStringAsync();
      // 5. Check for login success
      if (responseHtml.Contains("navbar-link") && responseHtml.Contains("einstellungen")) {
        _isLoggedIn = true;
        Base.OnInfo?.Invoke(null, new InfoEventArgs(InfoType.GourmetApi, "Login successful"));
        return true;
      }
      Base.OnError?.Invoke(null, new ErrorEventArgs(ErrorType.GourmetApi, null, "Gourmet login failed: invalid credentials or unexpected response."));
      return false;
    } catch (Exception ex) {
      Base.OnError?.Invoke(null, new ErrorEventArgs(ErrorType.GourmetApi, ex, "Exception during Gourmet API login."));
      Log.Debug(ex.ToString());
      return false;
    }
  }

  public static async Task<List<Day>> GetOrderDaysAsync() {
    Base.OnInfo?.Invoke(null, new InfoEventArgs(InfoType.GourmetApi, "Starting menu fetch"));
    // Ensure logged in before fetching menus
    if (!_isLoggedIn) {
      Log.Debug("Not logged in, calling LoginAsync from GetOrderDaysAsync");
      await LoginAsync();
    }
    if (!_isLoggedIn) {
      throw new InvalidOperationException("Not logged in to Gourmet API.");
    }
    var i = 0;
    List<Menu> result = [];
    var seen = new HashSet<string>(); // ToMenu track unique menus by a composite key
    while (i < 4) {
      try {
        var pageUrl = WebUrl + $"menus/?page={i}";
        Log.Debug($"Fetching menus page: {pageUrl}");
        var doc = await GetPageAsync(pageUrl);
        Base.OnInfo?.Invoke(null, new InfoEventArgs(InfoType.GourmetApi, $"Processing page {i}"));
        var menus = await ExtractMenusFromPage(doc);
        Log.Debug($"Extracted {menus.Count} menus from page {i}");
        foreach (var menu in menus) {
          // Use a composite key of date, title, and type to prevent duplicates
          var key = $"{menu.Date:yyyy-MM-dd}|{menu.Title}|{menu.Type}";
          if (seen.Add(key)) {
            result.Add(menu);
          } else {
            Log.Debug($"Skipping duplicate menu: {key}");
          }
        }
        if (menus.Count == 0) {
          Log.Debug("No menus found on page, breaking pagination loop");
          break;
        }
      } catch (Exception ex) {
        Log.Error($"GourmetApi.GetOrderDaysAsync exception while processing page {i}: {ex.Message}");
        Log.Debug(ex.ToString());
        break;
      }
      i++;
    }
    Log.Info($"GourmetApi.GetOrderDaysAsync finished, total menus: {result.Count}");
    // Group menus by date into Day objects
    // Group menus into days
    var availableDays = result.ConvertAll(m => m.Date.Date).Distinct();
    return (from day in availableDays
        let menusForDay = result.Where(m => m.Date.Date == day).ToList()
        let menu1 = menusForDay.FirstOrDefault(m => m.Type == MenuType.Menu1)
                    ?? new Menu(MenuType.Menu1, "N/A", [], 0.0m, day)
        let menu2 = menusForDay.FirstOrDefault(m => m.Type == MenuType.Menu2)
                    ?? new Menu(MenuType.Menu2, "N/A", [], 0.0m, day)
        let menu3 = menusForDay.FirstOrDefault(m => m.Type == MenuType.Menu3)
                    ?? new Menu(MenuType.Menu3, "N/A", [], 0.0m, day)
        let soupAndSalad = menusForDay.FirstOrDefault(m => m.Type == MenuType.SoupAndSalad)
                           ?? new Menu(MenuType.SoupAndSalad, "N/A", [], 0.0m, day)
        select new Day(day, menu1, menu2, menu3, soupAndSalad))
      .ToList();
  }
  
  private static async Task<HtmlDocument> GetPageAsync(string url) {
    Log.Debug($"GourmetApi.GetPageAsync fetching URL: {url}");
    var response = await BaseApi.Client.GetAsync(url);
    Log.Debug($"GourmetApi.GetPageAsync response status: {response.StatusCode}");
    response.EnsureSuccessStatusCode();
    var html = await response.Content.ReadAsStringAsync();
    var doc = new HtmlDocument();
    doc.LoadHtml(html);
    return doc;
  }
  
  private static string ExtractTitle(HtmlNode node)
  {
    var titleNode = node.SelectSingleNode(".//div[@class='subtitle']");
    return titleNode.InnerText.Trim();
  }

  private static string ExtractAllergens(HtmlNode node)
  {
    var allergeneNode = node.SelectSingleNode(".//li[contains(@class, 'allergen')]");
    return allergeneNode.InnerText.Trim();
  }

  private static string ExtractPrice(HtmlNode node)
  {
    var priceNode = node.SelectSingleNode(".//div[contains(@class, 'price')]/span");
    return priceNode.InnerText.Trim();
  }

  private static DateTime? ExtractDate(HtmlNode node)
  {
      var openInfoNode = node.SelectSingleNode(".//div[contains(@class, 'open_info') and contains(@class, 'menu-article-detail')]");
      var dateAttr = openInfoNode.GetAttributeValue("data-date", "");
      if (!string.IsNullOrEmpty(dateAttr)) {
        if (DateTime.TryParseExact(dateAttr, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)) {
              return parsedDate;
        }
        if (DateTime.TryParse(dateAttr, out var fallbackDate)) {
          return fallbackDate;
        }
      }
      Base.OnError?.Invoke(null, new ErrorEventArgs(ErrorType.GourmetApi, null, $"Unable to parse date attribute '{dateAttr}'"));
      return null;
  }

  private static MenuType ExtractMenuType(HtmlNode node)
  {
    var titleDiv = node.SelectSingleNode(".//div[contains(@class, 'title')]");
    var titleText = titleDiv.InnerText.Trim();
    if (titleText.Contains("SUPPE") || titleText.Contains("SUPPE &amp; SALAT"))
      return MenuType.SoupAndSalad;
    if (titleText.Contains("MENÜ III") || titleText.Contains("MEN&#220; III"))
      return MenuType.Menu3;
    if (titleText.Contains("MENÜ II") || titleText.Contains("MEN&#220; II"))
      return MenuType.Menu2;
    return MenuType.Menu1;
  }

  private static Task<List<Menu>> ExtractMenusFromPage(HtmlDocument doc) {
    var result = new List<Menu>();
    var menuNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'meal')]");
    foreach (var (index, node) in menuNodes.Index()) {
      if (!node.InnerHtml.Contains("open_info menu-article-detail")) continue;
      var date = ExtractDate(node);
      var title = ExtractTitle(node);
      var allergene = ExtractAllergens(node);
      var price = ExtractPrice(node);
      var menuType = ExtractMenuType(node);
      if (date.HasValue) {
        result.Add(new Menu(
          menuType,
          title,
          allergene.Where(char.IsLetter).ToArray(),
          decimal.TryParse(price, NumberStyles.Currency, CultureInfo.GetCultureInfo("de-AT"), out var parsedPrice) ? parsedPrice : 0m,
          date.Value
        ));
        Base.OnInfo?.Invoke(null, new InfoEventArgs(InfoType.GourmetApi, $"Extracted menu {index}/{menuNodes.Count}: {title}"));
      } else {
        Log.Debug("ExtractMenusFromPage: skipping menu with no date");
      }
    }
    return Task.FromResult(result);
  }
}

public class ErrorHandlingHandler : DelegatingHandler
{
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    try
    {
      var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

      if (!response.IsSuccessStatusCode)
      {
        var msg = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase} for {request.Method} {request.RequestUri}";
        Base.OnError?.Invoke(null, new ErrorEventArgs(ErrorType.GourmetApi, null, msg));
        // Option: throw new HttpRequestException(msg); // uncomment to force exceptions
      }

      return response;
    }
    catch (TaskCanceledException tex) when (!cancellationToken.IsCancellationRequested)
    {
      // Likely a timeout
      Base.OnError?.Invoke(null, new ErrorEventArgs(ErrorType.GourmetApi, tex, $"Request timed out for {request.RequestUri}"));
      throw;
    }
    catch (TaskCanceledException tex)
    {
      // Explicit cancellation
      Base.OnError?.Invoke(null, new ErrorEventArgs(ErrorType.GourmetApi, tex, $"Request cancelled for {request.RequestUri}"));
      throw;
    }
    catch (HttpRequestException hex)
    {
      Base.OnError?.Invoke(null, new ErrorEventArgs(ErrorType.GourmetApi, hex, $"Network error for {request.RequestUri}: {hex.Message}"));
      throw;
    }
    catch (Exception ex)
    {
      Base.OnError?.Invoke(null, new ErrorEventArgs(ErrorType.GourmetApi, ex, $"Unexpected error for {request.RequestUri}: {ex.Message}"));
      throw;
    }
  }
}