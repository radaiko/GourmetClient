using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GC.Common;
using GC.Models;
using HtmlAgilityPack;
using ErrorEventArgs = GC.Common.ErrorEventArgs;

namespace GC.Core.WebApis;

public static class VentoApi {
  // Made non-const and internal so tests can redirect traffic to a local WireMock server.
  private const string WebUrl = "https://my.ventopay.com/mocca.website/";
  private const string PageNameLogin = "Login.aspx";
  private const string PageNameLogout = "Ausloggen.aspx";
  private const string PageNameTransactions = "Transaktionen.aspx";
  private const string PageNameTransactionDetails = "Rechnung.aspx";
  private const string CompanyIdTrumpf = "0da8d3ec-0178-47d5-9ccd-a996f04acb61";

  private static bool _isLoggedIn;

  public static async Task<bool> LoginAsync() {
    Base.OnInfo?.Invoke(null, new InfoEventArgs(InfoType.VentoApi, "Starting login"));
    var username = Settings.It.VentoUsername;
    var password = Settings.It.VentoPassword;
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) {
      Base.OnError?.Invoke(null, new ErrorEventArgs(ErrorType.VentoApi, "Vento username or password not set in settings."));
      return false;
    }

    try {
      // Quick check: maybe session cookies already allow access
      if (await EnsureLoggedInAsync()) {
        Base.OnInfo?.Invoke(null, new InfoEventArgs(InfoType.VentoApi, "Already logged in"));
        return true;
      }

      var requestUrl = WebUrl + PageNameLogin;
      // 1) GET the login page to obtain ASP.NET form fields
      var loginPageResponse = await BaseApi.Client.GetAsync(requestUrl);
      var loginPageHtml = await loginPageResponse.Content.ReadAsStringAsync();

      Dictionary<string, string> parameters;
      try {
        parameters = ParseAspxParameters(loginPageHtml);
      }
      catch (Exception ex) {
        Base.OnError?.Invoke(null, new ErrorEventArgs(ErrorType.VentoApi, ex, "Error parsing the login HTML"));
        Logger.Debug(ex.ToString());
        return false;
      }

      // 2) add required form fields
      parameters["DropDownList1"] = CompanyIdTrumpf;
      parameters["TxtUsername"] = username;
      parameters["TxtPassword"] = password;
      parameters["BtnLogin"] = "Login";
      parameters["languageRadio"] = "DE";

      // 3) POST the form
      var content = new FormUrlEncodedContent(parameters);
      var loginResponse = await BaseApi.Client.PostAsync(requestUrl, content);
      var loginHtml = await loginResponse.Content.ReadAsStringAsync();

      // 4) check for logout link in response HTML
      if (loginHtml.Contains(PageNameLogout) || loginHtml.Contains("Ausloggen")) {
        _isLoggedIn = true;
        Base.OnInfo?.Invoke(null, new InfoEventArgs(InfoType.VentoApi, "Login successful"));
        return true;
      }

      Base.OnError?.Invoke(null, new ErrorEventArgs(ErrorType.VentoApi, loginResponse, "Vento login failed: invalid credentials or unexpected response."));
      return false;
    }
    catch (Exception ex) {
      Base.OnError?.Invoke(null, new ErrorEventArgs(ErrorType.VentoApi, ex, "Exception during Vento API login."));
      Logger.Debug(ex.ToString());
      return false;
    }
  }

  // Helper to parse ASP.NET hidden form fields like __VIEWSTATE, __VIEWSTATEGENERATOR and __EVENTVALIDATION
  private static Dictionary<string, string> ParseAspxParameters(string html) {
    var doc = new HtmlDocument();
    doc.LoadHtml(html);

    var viewStateNode = doc.DocumentNode.SelectSingleNode("//input[@id='__VIEWSTATE' or @name='__VIEWSTATE']");
    var viewStateGenNode = doc.DocumentNode.SelectSingleNode("//input[@id='__VIEWSTATEGENERATOR' or @name='__VIEWSTATEGENERATOR']");
    var eventValidationNode = doc.DocumentNode.SelectSingleNode("//input[@id='__EVENTVALIDATION' or @name='__EVENTVALIDATION']");

    if (viewStateNode == null || viewStateGenNode == null || eventValidationNode == null) {
      throw new Exception("Missing required ASP.NET form fields in login page");
    }

    string GetKey(HtmlNode node) => node.GetAttributeValue("name", node.GetAttributeValue("id", string.Empty));

    var parameters = new Dictionary<string, string> {
      { GetKey(viewStateNode), viewStateNode.GetAttributeValue("value", string.Empty) },
      { GetKey(viewStateGenNode), viewStateGenNode.GetAttributeValue("value", string.Empty) },
      { GetKey(eventValidationNode), eventValidationNode.GetAttributeValue("value", string.Empty) }
    };

    return parameters;
  }

  // Minimal login that just attempts to load the transactions page and checks for a logout link
  private static async Task<bool> EnsureLoggedInAsync() {
    if (_isLoggedIn) return true;
    try {
      var resp = await BaseApi.Client.GetAsync(WebUrl + PageNameTransactions);
      var html = await resp.Content.ReadAsStringAsync();
      if (html.Contains(PageNameLogout) || html.Contains("Ausloggen")) {
        _isLoggedIn = true;
        return true;
      }
    }
    catch { /* ignore and let caller handle */
    }
    return false;
  }

  public static async Task<InvoiceMonth> GetBillingMonthAsync(int year, int month) {
    Base.OnInfo?.Invoke(null, new InfoEventArgs(InfoType.VentoApi, "Starting menu fetch"));
    // Ensure logged in before fetching menus
    if (!_isLoggedIn) {
      Logger.Debug("Not logged in, calling LoginAsync from GetOrderDaysAsync");
      await LoginAsync();
    }
    if (!_isLoggedIn) {
      throw new InvalidOperationException("Not logged in to Vento API.");
    }
    
    // Build from/to date for the requested month
    var fromDate = new DateTime(year, month, 1);
    var toDate = fromDate.AddMonths(1).AddDays(-1);

    // Prepare request to transactions page with query parameters (if the site accepts them we try; otherwise we fetch and filter locally)
    // https://my.ventopay.com/mocca.website/Transaktionen.aspx?fromDate=01.10.2025&untilDate=31.10.2025
    var url = WebUrl + PageNameTransactions + $"?fromDate={fromDate:dd.MM.yyyy}&untilDate={toDate:dd.MM.yyyy}";

    var response = await BaseApi.Client.GetAsync(url);
    response.EnsureSuccessStatusCode();
    var html = await response.Content.ReadAsStringAsync();

    // Parse HTML and extract transaction rows. This implementation is defensive and simple.
    var doc = new HtmlDocument();
    doc.LoadHtml(html);
    
    var billingMonth = new InvoiceMonth();
    
   // Collect all <div class="transact"> nodes (including nested ones)
    var transactDivs = doc.DocumentNode.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' transact ')]");
    if (transactDivs == null) return billingMonth;
    foreach (var div in transactDivs) {
      var linkNode = div.SelectSingleNode(".//a[contains(translate(@href,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'rechnung.aspx') or contains(translate(@href,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'rechnung')]");
      if (linkNode == null) continue;
      var href = WebUtility.HtmlDecode(linkNode.GetAttributeValue("href", string.Empty)).Trim();
      if (string.IsNullOrEmpty(href)) continue;
      // build absolute url if needed
      var detailsUrl = href.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? href : WebUrl + href.TrimStart('/');
    
      try {
        var detailsResp = await BaseApi.Client.GetAsync(detailsUrl);
        detailsResp.EnsureSuccessStatusCode();
        var detailsHtml = await detailsResp.Content.ReadAsStringAsync();

        // Parse the fetched transaction page for timestamp and positions
        var transaction = ParseTransactionDetailsHtml(detailsHtml);
        if (transaction != null) {
          var tmp = billingMonth.Transactions.ToList();
          tmp.Add(transaction);
          billingMonth.Transactions = tmp.ToArray();
        }
      }
      catch {
        // ignore failures to fetch individual transaction pages
      }
    }

    return billingMonth;
  }

  // Parse a single invoice/details HTML and return a Transaction or null if none found
  internal static Transaction? ParseTransactionDetailsHtml(string detailsHtml) {
    if (string.IsNullOrWhiteSpace(detailsHtml)) return null;
    try {
      var detailsDoc = new HtmlDocument();
      detailsDoc.LoadHtml(detailsHtml);

      // Helper local function to try parse euro-formatted numbers (e.g. "1,80" or "€ 1,80")
      static bool TryParseEuro(string input, out decimal value) {
        value = 0m;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var s = WebUtility.HtmlDecode(input).Trim();
        s = s.Replace("€", string.Empty).Replace("EUR", string.Empty).Trim();
        s = Regex.Replace(s, "\\s+", "");
        if (decimal.TryParse(s, NumberStyles.Number, new CultureInfo("de-DE"), out value)) return true;
        s = s.Replace(".", string.Empty).Replace(',', '.');
        return decimal.TryParse(s, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
      }

      DateTime parsedDate = DateTime.MinValue;
      var timestampNode = detailsDoc.DocumentNode.SelectSingleNode("//span[@id='ContentPlaceHolder1_LblTimestamp']")
                          ?? detailsDoc.DocumentNode.SelectSingleNode("//*[contains(@id,'LblTimestamp')]");
      if (timestampNode != null) {
        var tsText = WebUtility.HtmlDecode(timestampNode.InnerText).Trim();
        tsText = tsText.Replace("Uhr", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("-", " ").Trim();
        if (!DateTime.TryParse(tsText, new CultureInfo("de-DE"), DateTimeStyles.AllowWhiteSpaces, out parsedDate)) {
          var m = Regex.Match(tsText, "(\\d{1,2}\\.\\s*\\w+\\s*\\d{4}).*?(\\d{1,2}:\\d{2})");
          if (m.Success) {
            var combined = (m.Groups[1].Value + " " + m.Groups[2].Value).Replace("  ", " ").Trim();
            DateTime.TryParse(combined, new CultureInfo("de-DE"), DateTimeStyles.AllowWhiteSpaces, out parsedDate);
          }
        }
      }

      var positions = new List<Position>();
      var posTable = detailsDoc.DocumentNode.SelectSingleNode("//div[contains(concat(' ', normalize-space(@class), ' '), ' section_title ') and contains(normalize-space(.), 'Positionen')]/following::table[1]");
      HtmlNodeCollection? rows;
      rows = posTable.SelectNodes(".//tbody//tr");

      string CleanName(string s) {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        s = WebUtility.HtmlDecode(s).Trim();
        s = Regex.Replace(s, @"\s+", " "); // collapse whitespace
        s = Regex.Replace(s, @"^\d+\s*[xX]\s*", ""); // remove leading quantity markers like "1 x"
        s = Regex.Replace(s, @"^[\u2022\-\•]\s*", ""); // remove leading bullets/dashes
        s = Regex.Replace(s, @"\s*[-:]\s*$", ""); // trim trailing separators
        s = Regex.Replace(s, @"\s*\(.*?\)\s*$", ""); // remove trailing parenthesized notes
        return s.Trim();
      }

      foreach (var row in rows) {
        var classAttr = row.GetAttributeValue("class", string.Empty);
        var rowText = row.InnerText ?? string.Empty;
        // skip separators and footer rows
        if (rowText.Contains("EUR", StringComparison.OrdinalIgnoreCase)) continue;
        if (classAttr.Contains("rechnungsdetail_position_line")) continue;

        var tds = row.SelectNodes(".//td");
        if (tds.Count < 5) continue;
          
        // Inline cleaner for position name

        var qtyText = tds[0].GetDirectInnerText();
        var nameText = CleanName(tds[1].GetDirectInnerText());
        var unitText = tds[2].GetDirectInnerText();
        var supportText = tds[3].GetDirectInnerText();
        var totalText = tds[4].GetDirectInnerText();

        var qty = 1;
        var mQty = Regex.Match(qtyText ?? string.Empty, "(\\d+)\\s*x", RegexOptions.IgnoreCase);
        if (mQty.Success && int.TryParse(mQty.Groups[1].Value, out var q)) qty = q;

        TryParseEuro(unitText, out var unitPrice);
        TryParseEuro(supportText, out var support);
        TryParseEuro(totalText, out var totalPrice);

        positions.Add(new Position(nameText ?? string.Empty, qty, unitPrice, support, totalPrice));
      }

      if (positions.Count == 0) return null;

      var transaction = new Transaction();
      transaction.Type = Transaction.TransactionType.CafePlusCo;
      transaction.Date = parsedDate == DateTime.MinValue ? DateTime.Now : parsedDate;
      transaction.Positions = positions.ToArray();
      return transaction;
    }
    catch (Exception ex) {
      Logger.Debug(ex.ToString());
      return null;
    }
  }

}

// Small helper wrapper to make HttpClient accept a custom handler in environments where DelegatingHandler is not used
internal static class HttpClientHandlerWrapper {
  public static HttpMessageHandler Create(HttpClientHandler baseHandler) => baseHandler;
}

// Extension helper to safely get inner text of a node without loading child text artifacts
internal static class HtmlNodeExtensions {
  public static string GetDirectInnerText(this HtmlNode node) {
    if (node == null) return string.Empty;
    return string.Concat(node.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Text).Select(n => n.InnerText)).Trim();
  }
}