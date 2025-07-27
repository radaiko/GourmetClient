using GourmetClient.Model;
using GourmetClient.Utils;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GourmetClient.Network;

public class VentopayWebClient : WebClientBase
{
    private const string WebUrl = "https://my.ventopay.com/mocca.website/";
    private const string PageNameLogin = "Login.aspx";
    private const string PageNameLogout = "Ausloggen.aspx";
    private const string PageNameTransactions = "Transaktionen.aspx";
    private const string PageNameTransactionDetails = "Rechnung.aspx";
    private const string CompanyIdTrumpf = "0da8d3ec-0178-47d5-9ccd-a996f04acb61";

    protected override async Task<bool> LoginImpl(string userName, string password)
    {
        var requestUrl = GetWebUrlForPage(PageNameLogin);
        using var loginPageResponse = await ExecuteGetRequest(requestUrl);
        var loginPageContent = await ReadResponseContent(loginPageResponse);

        Dictionary<string, string> parameters;
        try
        {
            parameters = ParseAspxParameters(loginPageContent);
        }
        catch (Exception exception) when (IsParseException(exception))
        {
            throw new GourmetParseException("Error parsing the login HTML", GetRequestUriString(loginPageResponse), loginPageContent, exception);
        }

        parameters.Add("DropDownList1", CompanyIdTrumpf);
        parameters.Add("TxtUsername", userName);
        parameters.Add("TxtPassword", password);
        parameters.Add("BtnLogin", "Login");
        parameters.Add("languageRadio", "DE");

        using var loginResponse = await ExecuteFormPostRequest(requestUrl, parameters);
        var loginContent = await ReadResponseContent(loginResponse);

        return Regex.IsMatch(loginContent, "<a\\s+href=\"Ausloggen.aspx\">");
    }

    protected override async Task LogoutImpl()
    {
        var requestUrl = GetWebUrlForPage(PageNameLogout);

        try
        {
            using var response = await ExecuteGetRequest(requestUrl);
        }
        catch (GourmetRequestException)
        {
            // Ignore this exception during logout. The session on the server side may be not cleared, but it will be
            // removed from the server eventually.
        }
    }

    public async Task<IReadOnlyList<BillingPosition>> GetBillingPositions(DateTime fromDate, DateTime toDate, IProgress<int> progress)
    {
        var parameters = new Dictionary<string, string>
        {
            { "fromDate", fromDate.ToString("dd.MM.yyyy") },
            { "untilDate", toDate.ToString("dd.MM.yyyy") }
        };

        var requestUrl = GetWebUrlForPage(PageNameTransactions);
        using var response = await ExecuteGetRequest(requestUrl, parameters);
        var htmlContent = await ReadResponseContent(response);

        try
        {
            return await GetBillingPositionsFromHtml(htmlContent, progress);
        }
        catch (Exception exception) when (IsParseException(exception))
        {
            throw new GourmetParseException("Error parsing the billing HTML", GetRequestUriString(response), htmlContent, exception);
        }
    }

    private static string GetWebUrlForPage(string pageName)
    {
        return $"{WebUrl}{pageName}";
    }

    private static Dictionary<string, string> ParseAspxParameters(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var viewStateNode = document.DocumentNode.GetSingleNode("//input[@id='__VIEWSTATE']");
        var viewStateGeneratorNode = document.DocumentNode.GetSingleNode("//input[@id='__VIEWSTATEGENERATOR']");
        var eventValidationNode = document.DocumentNode.GetSingleNode("//input[@id='__EVENTVALIDATION']");

        var parameters = new Dictionary<string, string>
        {
            { viewStateNode.GetAttributeValue("id"), viewStateNode.GetAttributeValue("value") },
            { viewStateGeneratorNode.GetAttributeValue("id"), viewStateGeneratorNode.GetAttributeValue("value") },
            { eventValidationNode.GetAttributeValue("id"), eventValidationNode.GetAttributeValue("value") }
        };

        return parameters;
    }

    private async Task<IReadOnlyList<BillingPosition>> GetBillingPositionsFromHtml(string html, IProgress<int> progress)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var billingPositions = new List<BillingPosition>();
        var contentNode = document.DocumentNode.GetSingleNode("//div[@class='content']");
        var rowNodes = contentNode.GetNodes(".//div[@class='transact']").ToArray();
        double rowCounter = 0;

        foreach (var rowNode in rowNodes)
        {
            if (!rowNode.TryGetAttributeValue("id", out string transactionId)
                || string.IsNullOrEmpty(transactionId))
            {
                continue;
            }

            var entries = await GetBillingPositionsFromTransaction(transactionId);
            billingPositions.AddRange(entries);

            rowCounter++;
            progress.Report((int)((rowCounter / rowNodes.Length) * 100));
        }

        return billingPositions;
    }

    private async Task<IReadOnlyList<BillingPosition>> GetBillingPositionsFromTransaction(string transactionId)
    {
        var parameters = new Dictionary<string, string>
        {
            { "id", transactionId }
        };

        var requestUrl = GetWebUrlForPage(PageNameTransactionDetails);
        using var response = await ExecuteGetRequest(requestUrl, parameters);
        var htmlContent = await ReadResponseContent(response);

        var document = new HtmlDocument();
        document.LoadHtml(htmlContent);

        var contentNode = document.DocumentNode.GetSingleNode("//div[@id='rechnung']");
        var restaurantInfoNode = contentNode.GetSingleNode(".//span[@id='ContentPlaceHolder1_LblRestaurantInfo']");
        var restaurantInfo = GetRestaurantInfo(restaurantInfoNode.InnerHtml);

        if (restaurantInfo.Name.Contains("Gourmet") && !restaurantInfo.Location.Contains("Kaffeeautomat"))
        {
            // Gourmet transactions are received from the Gourmet website (except for the coffee machine)
            return [];
        }

        var dateNode = contentNode.GetSingleNode(".//span[@id='ContentPlaceHolder1_LblTimestamp']");
        var tableBodyNode = contentNode.GetSingleNode(".//div[@class='rechnungpart']//table//tbody");

        var dateTime = GetDateTimeFromTransactionDateString(dateNode.GetInnerText());
        var billingPositions = new List<BillingPosition>();

        foreach (var rowNode in tableBodyNode.GetNodes(".//tr[not(contains(@class, 'rechnungsdetail'))]"))
        {
            var columnNodes = rowNode.GetNodes(".//td").ToList();

            var countNode = columnNodes[0];
            var positionNameNode = columnNodes[1];
            var costNode = columnNodes[4];

            var positionName = positionNameNode.GetInnerText();
            var countString = countNode.GetInnerText().Replace("x", string.Empty).Trim();
            var costString = costNode.GetInnerText();

            if (!int.TryParse(countString, out var count))
            {
                throw new FormatException($"Count '{countString}' has an invalid format");
            }

            if (!double.TryParse(costString, new CultureInfo("de-DE"), out var cost))
            {
                throw new FormatException($"Cost '{costString}' has an invalid format");
            }

            var positionType = positionName switch
            {
                "Snackware" => BillingPositionType.Menu,
                _ => BillingPositionType.Drink
            };

            billingPositions.Add(new BillingPosition(dateTime, positionType, positionName, count, cost));
        }

        return billingPositions;
    }

    private static DateTime GetDateTimeFromTransactionDateString(string dateString)
    {
        var match = Regex.Match(dateString, "(\\d+)\\.\\s+([a-zA-z]+)\\s+(\\d+)\\s+-\\s+(\\d+):(\\d+)");

        if (!match.Success)
        {
            throw new FormatException($"Date string '{dateString}' has an invalid format");
        }

        var dayString = match.Groups[1].Value;
        var monthString = match.Groups[2].Value;
        var yearString = match.Groups[3].Value;
        var hourString = match.Groups[4].Value;
        var minuteString = match.Groups[5].Value;

        if (!int.TryParse(dayString, out var day))
        {
            throw new FormatException($"Could not parse value '{dayString}' for day as integer");
        }

        var month = monthString switch
        {
            "Jan" => 1,
            "Feb" => 2,
            "Mrz" => 3,
            "Apr" => 4,
            "Mai" => 5,
            "Jun" => 6,
            "Jul" => 7,
            "Aug" => 8,
            "Sep" => 9,
            "Okt" => 10,
            "Nov" => 11,
            "Dez" => 12,
            _ => throw new FormatException($"Invalid month value: '{monthString}'")
        };

        if (!int.TryParse(yearString, out var year))
        {
            throw new FormatException($"Could not parse value '{yearString}' for year as integer");
        }

        if (!int.TryParse(hourString, out var hour))
        {
            throw new FormatException($"Could not parse value '{hourString}' for hour as integer");
        }

        if (!int.TryParse(minuteString, out var minute))
        {
            throw new FormatException($"Could not parse value '{minuteString}' for minute as integer");
        }

        return new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Local).ToUniversalTime();
    }

    private static RestaurantInfo GetRestaurantInfo(string infoString)
    {
        var parts = infoString.Split("<br>");
        return new RestaurantInfo(parts[0], parts[3]);
    }

    private static bool IsParseException(Exception exception)
    {
        return exception is GourmetHtmlNodeException || exception is FormatException;
    }

    private record RestaurantInfo(string Name, string Location);
}