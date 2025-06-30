using System.Globalization;
using System.Net.Http;

namespace GourmetClient.Network
{
    using GourmetClient.Model;
    using GourmetClient.Utils;
    using HtmlAgilityPack;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public partial class GourmetWebClient : WebClientBase
    {
        private const string WebUrl = "https://alaclickneu.gourmet.at/";

        private const string UfprtNodeName = "ufprt";

        private const string PageNameLogin = "start";

        private const string PageNameLogout = "start";

        private const string PageNameUserSettings = "einstellungen";

        private const string PageNameMenu = "menus";

        private const string PageNameOrderedMenu = "ShoppingCart";

        private const string PageNameAddMealToOrderedMenu = "AddItem";

        private const string PageNameBilling = "Billing";

        private const string ActionNameAddMealToOrderedMenu = "AddItemToShoppingCart";

        private const string ActionNameCancelMealOrder = "CancelItem";

        [GeneratedRegex(@"<a href=""https://alaclickneu.gourmet.at/einstellungen/"" class=""navbar-link"">")]
        private static partial Regex LoginSuccessfulRegex();

        [GeneratedRegex(@"<div\s+class=""settings gform"">")]
        private static partial Regex IsUserSettingsPageRegex();

        [GeneratedRegex(@"(MENÜ\s+[I]{1,3})")]
        private static partial Regex MenuTitleRegex();

        protected override async Task<bool> LoginImpl(string userName, SecureString password)
        {
            var ufprtValue = await GetUfprtValueFromPage(PageNameLogin);

            var parameters = new Dictionary<string, string>
            {
                {"Username", userName},
                {"Password", password.ToPlainPassword()},
                {"RememberMe", "false"},
                {"ufprt", ufprtValue}
            };

            using var response = await ExecutePostRequest(WebUrl, parameters);
            var httpContent = await GetResponseContent(response);

            // Login is successful if link to user settings is received
            return LoginSuccessfulRegex().IsMatch(httpContent);
        }

        protected override async Task LogoutImpl()
        {
            var ufprtValue = await GetUfprtValueFromPage(PageNameLogout);

            var parameters = new Dictionary<string, string>
            {
                {"ufprt", ufprtValue}
            };

            using var response = await ExecutePostRequestForPage(PageNameLogout, parameters);
        }

        public async Task<GourmetUserData> GetUserData()
        {
            using var response = await ExecuteGetRequestForPage(PageNameUserSettings);
            var httpContent = await GetResponseContent(response);

            if (!IsUserSettingsPageRegex().IsMatch(httpContent))
            {
                return null;
            }

            var nameOfUser = ParseHtmlForNameOfUser(httpContent);

            if (nameOfUser == null)
            {
                return null;
            }

            return new GourmetUserData(nameOfUser);
        }

        public async Task<GourmetMenu> GetMenu()
        {
            var currentPage = 0;
            var parsedDays = new List<GourmetMenuDay>();

            // Set 10 pages as limit (equals 10 weeks)
            while (currentPage < 10)
            {
                var pageParameter = new Dictionary<string, string>
                {
                    {"page", currentPage.ToString()}
                };

                using var response = await ExecuteGetRequestForPage(PageNameMenu, pageParameter);
                var httpContent = await GetResponseContent(response);

                IReadOnlyCollection<GourmetMenuDay> foundDays;

                try
                {
                    foundDays = ParseGourmetMenuHtml(httpContent);
                }
                catch (Exception exception)
                {
                    throw new GourmetParseException("Error parsing the menu HTML", GetRequestUriString(response), httpContent, exception);
                }

                if (foundDays.Count == 0)
                {
                    break;
                }

                parsedDays.AddRange(foundDays);
                currentPage++;
            }

            return new GourmetMenu(parsedDays);
        }

        public async Task<OrderedGourmetMenu> GetOrderedMenu()
        {
            using var response = await ExecuteGetRequestForPage__Old(PageNameOrderedMenu);
            var httpContent = await GetResponseContent(response);

            try
            {
                return ParseOrderedGourmetMenuHtml(httpContent);
            }
            catch (Exception exception)
            {
                throw new GourmetParseException("Error parsing the ordered menu HTML", GetRequestUriString(response), httpContent, exception);
            }
        }

        public async Task AddMealToOrderedMenu(GourmetMenuMeal meal)
        {
            meal = meal ?? throw new ArgumentNullException(nameof(meal));

            if (string.IsNullOrEmpty(meal.ProductId))
            {
                throw new InvalidOperationException($"Meal {meal.Name} (Description: '{meal.Description}') cannot be ordered");
            }

            var parameters = new Dictionary<string, string>
            {
                { "ProductID", meal.ProductId },
                { "IsMenu", "1" }
            };

            using var response = await ExecuteGetRequest(WebUrl, GetUrlParametersForPage__Old(PageNameAddMealToOrderedMenu, ActionNameAddMealToOrderedMenu, parameters));
        }

        public async Task CancelOrder(OrderedGourmetMenuMeal orderedMeal)
        {
            orderedMeal = orderedMeal ?? throw new ArgumentNullException(nameof(orderedMeal));

            if (!orderedMeal.IsOrderCancelable)
            {
                throw new InvalidOperationException($"Order {orderedMeal.Name} (OrderId: '{orderedMeal.OrderId}') cannot be canceled");
            }

            var parameters = new Dictionary<string, string>
            {
                { "id", orderedMeal.OrderId },
                { "ismenu", "1" }
            };

            using var response = await ExecuteGetRequest(WebUrl, GetUrlParametersForPage__Old(PageNameOrderedMenu, ActionNameCancelMealOrder, parameters));
        }

        public async Task ConfirmOrder()
        {
            using var responseOrderedMenu = await ExecuteGetRequestForPage__Old(PageNameOrderedMenu);
            var httpContentOrderedMenu = await GetResponseContent(responseOrderedMenu);
            var confirmParameters = ParseConfirmParametersFromOrderedGourmetMenuHtml(httpContentOrderedMenu);

            using var response = await ExecutePostRequest(WebUrl, confirmParameters);
        }

        public async Task<IReadOnlyList<BillingPosition>> GetBillingPositions(int month, int year, IProgress<int> progress)
        {
            var parameters = new Dictionary<string, string>
            {
                {"PostBackSelectMonth", "1"},
                {"inputAbrechnung", $"{month}-{year}"}
            };

            //using var response = await ExecutePostRequest(WebUrl, GetUrlParametersForPage__Old(PageNameBilling), parameters);
            using var response = await ExecutePostRequest(WebUrl, parameters);
            var httpContent = await GetResponseContent(response);

            try
            {
                return ParseBillingPositionsFromBillingHtml(httpContent);
            }
            catch (Exception exception)
            {
                throw new GourmetParseException("Error parsing the billing HTML", GetRequestUriString(response), httpContent, exception);
            }
            finally
            {
                progress.Report(100);
            }
        }

        private async Task<string> GetUfprtValueFromPage(string pageName)
        {
            var response = await ExecuteGetRequestForPage(pageName);
            var httpContent = await GetResponseContent(response);
            var document = new HtmlDocument();

            document.LoadHtml(httpContent);
            var ufprtNode = document.DocumentNode.GetSingleNode($"//input[@name='{UfprtNodeName}']");

            if (ufprtNode == null)
            {
                throw new GourmetParseException($"Node with name '{UfprtNodeName}' not found", GetRequestUriString(response), httpContent);
            }

            return ufprtNode.Attributes["value"].Value;
        }

        private Task<HttpResponseMessage> ExecuteGetRequestForPage(string pageName, IReadOnlyDictionary<string, string> urlParameters = null)
        {
            return ExecuteGetRequest($"{WebUrl}{pageName}/", urlParameters);
        }

        private Task<HttpResponseMessage> ExecutePostRequestForPage(string pageName, IReadOnlyDictionary<string, string> formParameters)
        {
            return ExecutePostRequest($"{WebUrl}{pageName}/", formParameters);
        }

        private Task<HttpResponseMessage> ExecuteGetRequestForPage__Old(string pageName)
        {
            return ExecuteGetRequest(WebUrl, GetUrlParametersForPage__Old(pageName));
        }

        private static string ParseHtmlForNameOfUser(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var loginNameNode = document.DocumentNode.SelectSingleNode("//div[@class='userfield']//span[@class='loginname']");
            if (loginNameNode == null)
            {
                return null;
            }

            return loginNameNode.GetInnerText();
        }

        private static IReadOnlyCollection<GourmetMenuDay> ParseGourmetMenuHtml(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var parsedMeals = new Dictionary<DateTime, List<GourmetMenuMeal>>();

            foreach (var mealNode in document.DocumentNode.GetNodes("//div[@class='meal']"))
            {
                var detailNode = mealNode.GetSingleNode(".//div[@class='open_info menu-article-detail']");
                var positionId = detailNode.Attributes["data-id"].Value;
                var dateString = detailNode.Attributes["data-date"].Value;
                var day = ParseMealDateString(dateString);

                if (!parsedMeals.ContainsKey(day))
                {
                    parsedMeals.Add(day, new List<GourmetMenuMeal>());
                }

                var mealsForDay = parsedMeals[day];

                if (mealsForDay.Any(meal => meal.ProductId == positionId))
                {
                    // Meal has already been parsed
                    // The page contains the meal elements twice (desktop UI and mobile UI)
                    continue;
                }

                var title = mealNode.GetSingleNode(".//div[@class='title']").ChildNodes[0].GetInnerText().Trim();
                var subTitle = mealNode.GetSingleNode(".//div[@class='subtitle']").GetInnerText();

                var titleMatch = MenuTitleRegex().Match(title);
                if (titleMatch.Success)
                {
                    title = titleMatch.Groups[1].Value;
                }

                mealsForDay.Add(new GourmetMenuMeal(positionId, title, subTitle));
            }

            return parsedMeals.Select(entry => new GourmetMenuDay(entry.Key, entry.Value)).ToList();
        }

        private static DateTime ParseMealDateString(string dateString)
        {
            var splitValue = dateString.Split('-');
            if (splitValue.Length != 3)
            {
                throw new InvalidOperationException($"Expected three values after splitting the date string '{dateString}' but there are {splitValue.Length} value(s)");
            }

            var monthString = splitValue[0];
            var dayString = splitValue[1];
            var yearString = splitValue[2];

            if (!int.TryParse(dayString, out var day))
            {
                throw new InvalidOperationException($"Could not parse value '{dayString}' for day as integer");
            }

            if (!int.TryParse(monthString, out var month))
            {
                throw new InvalidOperationException($"Could not parse value '{monthString}' for month as integer");
            }

            if (!int.TryParse(yearString, out var year))
            {
                throw new InvalidOperationException($"Could not parse value '{yearString}' for year as integer");
            }

            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        private static OrderedGourmetMenu ParseOrderedGourmetMenuHtml(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var days = new List<OrderedGourmetMenuDay>();

            foreach (var orderItemNode in document.DocumentNode.GetNodes("//div[contains(@class, 'order-item')]"))
            {
                var dateInputNode = orderItemNode.GetSingleNode(".//input[contains(@name, 'order_') and @type='hidden']");
                var titleNode = orderItemNode.GetSingleNode(".//div[@class='title']");

                var dateInputNameValue = dateInputNode.Attributes["name"].Value;

                var orderIdMatch = Regex.Match(dateInputNameValue, "^order_([0-9]+)$");
                if (!orderIdMatch.Success)
                {
                    throw new InvalidOperationException($"Order id '{dateInputNameValue}' has an invalid format");
                }

                var orderId = orderIdMatch.Groups[1].Value;
                var date = GetDateFromOrderedMenuDayAttribute(dateInputNode.Attributes["value"].Value);
                var mealName = titleNode.GetInnerText();
                var isOrderCancelable = orderItemNode.GetNodes(".//div[@class='cancel']").Any();
                bool isOrderApproved;

                var orderApprovedInputNode = orderItemNode.SelectSingleNode($".//input[@name='ITM_SelectedRotationId_{orderId}' and @type='radio']");
                if (orderApprovedInputNode != null)
                {
                    isOrderApproved = orderApprovedInputNode.Attributes["class"].Value == "greentext";
                }
                else
                {
                    var checkMarkIconNode = orderItemNode.SelectSingleNode(".//span[@class='checkmark']//i[@class='fa fa-check']");
                    isOrderApproved = checkMarkIconNode != null;
                }

                var orderedMeal = new OrderedGourmetMenuMeal(orderId, mealName, isOrderApproved, isOrderCancelable);

                days.Add(new OrderedGourmetMenuDay(date, orderedMeal));
            }

            return new OrderedGourmetMenu(days);
        }

        private static IReadOnlyDictionary<string, string> ParseConfirmParametersFromOrderedGourmetMenuHtml(string html)
        {
            var parameters = new Dictionary<string, string>();

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var formNode = document.DocumentNode.GetSingleNode("//form[@action='default.aspx' and @method='post' and @name='genericform']");

            foreach (var inputNode in formNode.GetNodes(".//*[self::input or self::select]"))
            {
                var name = inputNode.Attributes["name"]?.Value;

                if (!string.IsNullOrEmpty(name))
                {
                    parameters.Add(name, inputNode.Attributes["value"]?.Value ?? string.Empty);
                }
            }

            var confirmButtonNode = formNode.GetSingleNode(".//button[@name='btn_order_confirm']");
            parameters.Add(confirmButtonNode.Attributes["name"].Value, confirmButtonNode.Attributes["value"].Value);

            return parameters;
        }

        private static IReadOnlyList<BillingPosition> ParseBillingPositionsFromBillingHtml(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var billingPositions = new List<BillingPosition>();
            var tableBodyNode = document.DocumentNode.GetSingleNode("//div[@class='abrechnung']//table[contains(@class, 'table-bordered')]//tbody");
            var lastParsedDate = DateTime.MinValue;

            foreach (var rowNode in tableBodyNode.GetNodes(".//tr"))
            {
                var dateNode = rowNode.SelectSingleNode(".//td[@data-title='Datum']");
                if (dateNode == null)
                {
                    continue;
                }

                var countNode = rowNode.GetSingleNode(".//td[@data-title='Stk.']");
                var mealNameNode = rowNode.GetSingleNode(".//td[@data-title='Speise']");
                var totalCostNode = rowNode.GetSingleNode(".//td[@data-title='Gesamt']");
                var subsidyNode = rowNode.GetSingleNode(".//td[@data-title='Stützung']");

                // If a bill contains multiple items, then only the first item row contains the date
                // For the other items, the column is empty
                var date = lastParsedDate;
                var dateNodeText = dateNode.GetInnerText();
                if (!string.IsNullOrWhiteSpace(dateNodeText))
                {
                    date = GetDateFromBillingEntryDateString(dateNodeText);
                    lastParsedDate = date;
                }

                var mealName = mealNameNode.GetInnerText();
                var countString = countNode.GetInnerText();
                var totalCostString = totalCostNode.GetInnerText().Replace("€", string.Empty).Trim();
                var subsidyString = subsidyNode.GetInnerText().Replace("€", string.Empty).Trim();

                if (!int.TryParse(countString, out var count))
                {
                    throw new InvalidOperationException($"Count '{countString}' has an invalid format");
                }

                if (!double.TryParse(totalCostString, new CultureInfo("de-DE"), out var totalCost))
                {
                    throw new InvalidOperationException($"Cost '{totalCostString}' has an invalid format");
                }

                // If a bill contains multiple items, then only the first item row contains a subsidy value
                // For the other items, the column is empty
                var subsidy = 0.0;
                if (!string.IsNullOrWhiteSpace(subsidyString) && !double.TryParse(subsidyString, new CultureInfo("de-DE"), out subsidy))
                {
                    throw new InvalidOperationException($"Subsidy '{subsidyString}' has an invalid format");
                }

                var cost = totalCost - subsidy;
                billingPositions.Add(new BillingPosition(date, false, BillingPositionType.Meal, mealName, count, cost));
            }

            return billingPositions;
        }

        private static DateTime GetDateFromMenuDayNodeValue(string nodeValue)
        {
            var splitValue = nodeValue.Split('.');
            if (splitValue.Length != 2)
            {
                throw new InvalidOperationException($"Expected two values after splitting the date node value '{nodeValue}' but there are {splitValue.Length} value(s)");
            }

            var dayString = splitValue[0];
            var monthString = splitValue[1];

            if (!int.TryParse(dayString, out var day))
            {
                throw new InvalidOperationException($"Could not parse value '{dayString}' for day as integer");
            }

            if (!int.TryParse(monthString, out var month))
            {
                throw new InvalidOperationException($"Could not parse value '{monthString}' for month as integer");
            }

            return new DateTime(DateTime.Now.Year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        private static DateTime GetDateFromOrderedMenuDayAttribute(string attributeValue)
        {
            var spaceSplitValue = attributeValue.Split(' ');
            if (spaceSplitValue.Length != 2)
            {
                throw new InvalidOperationException($"Expected two values after splitting the date node value '{attributeValue}' but there are {spaceSplitValue.Length} value(s)");
            }

            var dateSplitValue = spaceSplitValue[0].Split('.');
            if (dateSplitValue.Length != 3)
            {
                throw new InvalidOperationException($"Expected three values after splitting the date node value '{attributeValue}' but there are {spaceSplitValue.Length} value(s)");
            }

            var dayString = dateSplitValue[0];
            var monthString = dateSplitValue[1];
            var yearString = dateSplitValue[2];

            if (!int.TryParse(dayString, out var day))
            {
                throw new InvalidOperationException($"Could not parse value '{dayString}' for day as integer");
            }

            if (!int.TryParse(monthString, out var month))
            {
                throw new InvalidOperationException($"Could not parse value '{monthString}' for month as integer");
            }

            if (!int.TryParse(yearString, out var year))
            {
                throw new InvalidOperationException($"Could not parse value '{year}' for year as integer");
            }

            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        private static DateTime GetDateFromBillingEntryDateString(string dateString)
        {
            var dateSplitValue = dateString.Split('.');
            if (dateSplitValue.Length != 3)
            {
                throw new InvalidOperationException($"Expected three values after splitting the date node value '{dateString}' but there are {dateSplitValue.Length} value(s)");
            }

            var dayString = dateSplitValue[0];
            var monthString = dateSplitValue[1];
            var yearString = dateSplitValue[2];

            if (!int.TryParse(dayString, out var day))
            {
                throw new InvalidOperationException($"Could not parse value '{dayString}' for day as integer");
            }

            if (!int.TryParse(monthString, out var month))
            {
                throw new InvalidOperationException($"Could not parse value '{monthString}' for month as integer");
            }

            if (!int.TryParse(yearString, out var year))
            {
                throw new InvalidOperationException($"Could not parse value '{yearString}' for year as integer");
            }

            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        private static IReadOnlyDictionary<string, string> GetUrlParametersForPage__Old(string pageName, string actionName = null, IReadOnlyDictionary<string, string> parameters = null)
        {
            var pageParameters = new Dictionary<string, string>
            {
                { "c", pageName }
            };

            if (!string.IsNullOrEmpty(actionName))
            {
                pageParameters.Add("a", actionName);
            }

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    pageParameters.Add(parameter.Key, parameter.Value);
                }
            }

            return pageParameters;
        }
    }
}
