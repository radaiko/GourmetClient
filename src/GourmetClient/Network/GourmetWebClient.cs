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

        private const string PageNameMenu = "menus";

        private const string PageNameOrderedMenu = "bestellungen";

        private const string PageNameAddMealToOrderedMenu = "AddItem";

        private const string PageNameBilling = "Billing";

        private const string ActionNameAddMealToOrderedMenu = "AddItemToShoppingCart";

        private const string ActionNameCancelMealOrder = "CancelItem";

        [GeneratedRegex(@"<a href=""https://alaclickneu.gourmet.at/einstellungen/"" class=""navbar-link"">")]
        private static partial Regex LoginSuccessfulRegex();

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

        public async Task<GourmetMenuResult> GetMenus()
        {
            // The page contains the menu elements twice (once for the desktop UI and once for the mobile UI).
            // By using a HashSet with a custom comparer, it is ensured that the same menu is only added once.
            var parsedMenus = new HashSet<GourmetMeal>(new GourmetMenuComparer());

            GourmetUserInformation userInformation = null;
            var currentPage = 0;

            // Set 10 pages as upper limit
            while (currentPage < 10)
            {
                var pageParameter = new Dictionary<string, string>
                {
                    {"page", currentPage.ToString()}
                };

                using var response = await ExecuteGetRequestForPage(PageNameMenu, pageParameter);
                var httpContent = await GetResponseContent(response);

                try
                {
                    var document = new HtmlDocument();
                    document.LoadHtml(httpContent);

                    userInformation ??= ParseHtmlForUserInformation(document);

                    foreach (GourmetMeal parsedMenu in ParseGourmetMenuHtml(document))
                    {
                        parsedMenus.Add(parsedMenu);
                    }

                    if (!HasNextPageButton(document))
                    {
                        break;
                    }
                }
                catch (Exception exception)
                {
                    throw new GourmetParseException("Error parsing the menu HTML", GetRequestUriString(response), httpContent, exception);
                }

                currentPage++;
            }

            return new GourmetMenuResult(userInformation, parsedMenus);
        }

        //public async Task<GourmetMeal> GetFullMenu(GourmetUserInformation userInformation, ParsedGourmetMenu parsedMenu)
        //{
        //    var parameter = new
        //    {
        //        shopModelId = userInformation.ShopModelId,
        //        eaterId = userInformation.EaterId,
        //        staffgroupId = userInformation.StaffGroupId,
        //        date = parsedMenu.DateString,
        //        menuId = parsedMenu.PositionId
        //    };

        //    char[] ParseAllergens(string value)
        //    {
        //        if (string.IsNullOrWhiteSpace(value))
        //        {
        //            return [];
        //        }

        //        return value.Split('|', StringSplitOptions.RemoveEmptyEntries).Select(part => part[0]).ToArray();
        //    }

        //    using var response = await ExecuteJsonPostRequest($"{WebUrl}umbraco/api/AlaArticleApi/GetMenuInfo", parameter);
        //    return await GetJsonResponseObject(
        //        response,
        //        json => new GourmetMeal(
        //            ParseMealDateString(json.GetProperty("dateString").GetString()),
        //            json.GetProperty("id").GetString(),
        //            json.GetProperty("menuName").GetString(),
        //            json.GetProperty("description").GetString(),
        //            json.GetProperty("menuNumber").GetString(),
        //            ParseAllergens(json.GetProperty("allergens").GetString()),
        //            json.GetProperty("grossPrice").GetDouble(),
        //            json.GetProperty("amount").GetInt32(),
        //            json.GetProperty("ordered").GetInt32())
        //        );

        //}

        public async Task<IReadOnlyCollection<GourmetOrderedMenu>> GetOrderedMenus()
        {
            using var response = await ExecuteGetRequestForPage(PageNameOrderedMenu);
            var httpContent = await GetResponseContent(response);

            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(httpContent);

                return ParseOrderedGourmetMenuHtml(document).ToArray();
            }
            catch (Exception exception)
            {
                throw new GourmetParseException("Error parsing the ordered menu HTML", GetRequestUriString(response), httpContent, exception);
            }
        }

      /*  public async Task AddMealToOrderedMenu(GourmetMenuMeal meal)
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
      */
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

        private static GourmetUserInformation ParseHtmlForUserInformation(HtmlDocument document)
        {
            var loginNameNode = document.DocumentNode.SelectSingleNode("//div[@class='userfield']//span[@class='loginname']");
            var shopModelNode = document.DocumentNode.SelectSingleNode("//input[@id='shopModel']");
            var eaterNode = document.DocumentNode.SelectSingleNode("//input[@id='eater']");
            var staffGroupNode = document.DocumentNode.SelectSingleNode("//input[@id='staffGroup']");

            var nameOfUser = loginNameNode.GetInnerText();
            var shopModelId = shopModelNode.Attributes["value"].Value;
            var eaterId = eaterNode.Attributes["value"].Value;
            var staffGroupId = staffGroupNode.Attributes["value"].Value;

            return new GourmetUserInformation(nameOfUser, shopModelId, eaterId, staffGroupId);
        }

        private static IEnumerable<GourmetMeal> ParseGourmetMenuHtml(HtmlDocument document)
        {
            foreach (var mealNode in document.DocumentNode.GetNodes("//div[@class='meal']"))
            {
                var detailNode = mealNode.GetSingleNode(".//div[@class='open_info menu-article-detail']");
                var positionId = detailNode.Attributes["data-id"].Value;
                var day = ParseMenuDateString(detailNode.Attributes["data-date"].Value);
                var title = mealNode.GetSingleNode(".//div[@class='title']").ChildNodes[0].GetInnerText().Trim();
                var subTitle = mealNode.GetSingleNode(".//div[@class='subtitle']").GetInnerText();
                var allergens = ParseAllergens(mealNode.GetSingleNode(".//li[@class='allergen']").GetInnerText());
                var isAvailable = mealNode.ContainsNode(".//input[@type='checkbox' and @class='menu-clicked']");

                yield return new GourmetMeal(day, positionId, title, subTitle, allergens, isAvailable);
            }
        }

        private static DateTime ParseMenuDateString(string dateString)
        {
            // Sample value: "06-30-2025"
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

        private static char[] ParseAllergens(string allergensString)
        {
            // Sample value: "A, C, G"
            return allergensString
                .Split(',')
                .Select(part => part.Trim())
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => part[0])
                .ToArray();
        }

        private static bool HasNextPageButton(HtmlDocument document)
        {
            return document.DocumentNode.ContainsNode("//a[contains(@class, 'menues-next')]");
        }

        private static IEnumerable<GourmetOrderedMenu> ParseOrderedGourmetMenuHtml(HtmlDocument document)
        {
            foreach (var orderItemNode in document.DocumentNode.GetNodes("//div[contains(@class, 'order-item')]"))
            {
                var formNode = orderItemNode.GetSingleNode(".//form[contains(@class, 'form-info-orders')]");
                var positionId = formNode.GetSingleNode(".//input[@name='cp_PositionId']").Attributes["value"].Value;

                var eatingCycleIdInputNode = formNode.GetSingleNode($".//input[(@name='cp_EatingCycleId_{positionId}') and @type='hidden']");
                var dateInputNode = formNode.GetSingleNode($".//input[(@name='cp_Date_{positionId}') and @type='hidden']");

                var eatingCycleId = eatingCycleIdInputNode.Attributes["value"].Value;
                var day = ParseOrderedMenuDateString(dateInputNode.Attributes["value"].Value);
                var title = formNode.GetSingleNode(".//div[@class='title']").GetInnerText();
                bool isOrderApproved;

                // This <input> node is only available if the web page is currently in edit mode.
                var orderApprovedInputNode = orderItemNode.SelectSingleNode($".//input[@name='cec_NewEatingCycleId_{positionId}' and @type='radio']");
                if (orderApprovedInputNode != null)
                {
                    isOrderApproved = orderApprovedInputNode.Attributes["class"].Value.Contains("confirmed");
                }
                else
                {
                    // If the web page is not in edit mode, then this <i> node indicates whether the order is approved.
                    isOrderApproved = orderItemNode.ContainsNode(".//span[@class='checkmark']//i[@class='fa fa-check']");
                }

                yield return new GourmetOrderedMenu(day, positionId, eatingCycleId, title, isOrderApproved);
            }
        }

        private static DateTime ParseOrderedMenuDateString(string dateString)
        {
            // Sample value: "30.06.2025 00:00:00"
            var spaceSplitValue = dateString.Split(' ');
            if (spaceSplitValue.Length != 2)
            {
                throw new InvalidOperationException($"Expected two values after splitting the date node value '{dateString}' but there are {spaceSplitValue.Length} value(s)");
            }

            var dateSplitValue = spaceSplitValue[0].Split('.');
            if (dateSplitValue.Length != 3)
            {
                throw new InvalidOperationException($"Expected three values after splitting the date node value '{dateString}' but there are {spaceSplitValue.Length} value(s)");
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

        private class GourmetMenuComparer : IEqualityComparer<GourmetMeal>
        {
            /// <summary>
            /// Compares whether two <see cref="GourmetMeal"/> instances are equal.
            /// Two meals are considered equal if their <see cref="GourmetMeal.MenuId"/> and <see cref="GourmetMeal.Day"/>
            /// properties are equal. This is because the menu id is only unique within one day, but menus on different
            /// days can have the same menu id.
            /// </summary>
            /// <param name="x">The first instance.</param>
            /// <param name="y">The second instance.</param>
            /// <returns>A value indicating whether the two instances are equal.</returns>
            public bool Equals(GourmetMeal x, GourmetMeal y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.Day == y.Day && x.MenuId == y.MenuId;
            }

            public int GetHashCode(GourmetMeal obj)
            {
                return HashCode.Combine(obj.Day, obj.MenuId);
            }
        }
    }
}
