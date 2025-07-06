using System.Net.Http;
using System.Text.Json;

namespace GourmetClient.Network
{
    using GourmetClient.Model;
    using GourmetClient.Utils;
    using HtmlAgilityPack;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public partial class GourmetWebClient : WebClientBase
    {
        private const string WebUrl = "https://alaclickneu.gourmet.at/";

        private const string PageNameStart = "start";

        private const string PageNameMenu = "menus";

        private const string PageNameOrderedMenu = "bestellungen";

        [GeneratedRegex(@"<a href=""https://alaclickneu.gourmet.at/einstellungen/"" class=""navbar-link"">")]
        private static partial Regex LoginSuccessfulRegex();

        protected override async Task<bool> LoginImpl(string userName, string password)
        {
            var ufprtValue = await GetUfprtValueFromPage(PageNameStart, "//div[@class='login']//form");

            var parameters = new Dictionary<string, string>
            {
                {"Username", userName},
                {"Password", password},
                {"RememberMe", "false"},
                {"ufprt", ufprtValue}
            };

            using var response = await ExecuteFormPostRequest(WebUrl, parameters);
            var httpContent = await GetResponseContent(response);

            // Login is successful if link to user settings is received
            return LoginSuccessfulRegex().IsMatch(httpContent);
        }

        protected override async Task LogoutImpl()
        {
            var ufprtValue = await GetUfprtValueFromPage(PageNameStart, "//form[.//button[@id='btnHeaderLogout']]");

            var parameters = new Dictionary<string, string>
            {
                {"ufprt", ufprtValue}
            };

            using var response = await ExecutePostRequestForPage(PageNameStart, parameters);
        }

        public async Task<GourmetMenuResult> GetMenus()
        {
            // The page contains the menu elements twice (once for the desktop UI and once for the mobile UI).
            // By using a HashSet with a custom comparer, it is ensured that the same menu is only added once.
            var parsedMenus = new HashSet<GourmetMenu>();
            GourmetUserInformation? userInformation = null;

            // Set 10 pages as upper limit
            var maxPages = 10;
            var currentPage = 0;

            do
            {
                var pageParameter = new Dictionary<string, string>
                {
                    { "page", currentPage.ToString() }
                };

                using var response = await ExecuteGetRequestForPage(PageNameMenu, pageParameter);
                var httpContent = await GetResponseContent(response);

                try
                {
                    var document = new HtmlDocument();
                    document.LoadHtml(httpContent);

                    userInformation ??= ParseHtmlForUserInformation(document);

                    foreach (GourmetMenu parsedMenu in ParseGourmetMenuHtml(document))
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
                    throw new GourmetParseException("Error parsing the menu HTML", GetRequestUriString(response),
                        httpContent, exception);
                }

                currentPage++;
            } while (currentPage < maxPages);

            return new GourmetMenuResult(userInformation, parsedMenus);
        }

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

        public async Task<GourmetApiResult> AddMenuToOrderedMenu(GourmetUserInformation userInformation, GourmetMenu menu)
        {
            var parameter = new
            {
                dates = new[]
                {
                    new { date = menu.Day.ToString("MM-dd-yyyy"), menuIds = new string[] { menu.MenuId }}
                },
                eaterId = userInformation.EaterId,
                shopModelId = userInformation.ShopModelId,
                staffgroupId = userInformation.StaffGroupId
            };

            using var response = await ExecuteJsonPostRequest($"{WebUrl}umbraco/api/AlaCartApi/AddToMenuesCart", parameter);
            return await GetJsonResponseObject(response, json =>
                new GourmetApiResult(
                    json.GetProperty("success").GetBoolean(),
                    json.GetProperty("message").GetString()));
        }

        public async Task CancelOrder(GourmetOrderedMenu orderedMenu)
        {
            (HtmlDocument document, string resultUriInfo, string resultHttpContent) = await EnterOrderedMenuEditMode();

            Dictionary<string, string> cancelOrderParameters;
            try
            {
                cancelOrderParameters = GetCancelOrderParameters(document, orderedMenu.PositionId);
            }
            catch (Exception exception)
            {
                throw new GourmetParseException("Error parsing the ordered menu HTML", resultUriInfo, resultHttpContent, exception);
            }

            using var cancelOrderResponse = await ExecutePostRequestForPage(PageNameOrderedMenu, cancelOrderParameters);
        }

        public async Task ConfirmOrder()
        {
            using var orderedMenuResponse = await ExecuteGetRequestForPage(PageNameOrderedMenu);
            var orderedMenuHttpContent = await GetResponseContent(orderedMenuResponse);

            Dictionary<string, string> confirmOrderParameters;
            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(orderedMenuHttpContent);

                if (!IsOrderedMenuPageEditModeActive(document))
                {
                    // Only needs to be confirmed if edit mode is active
                    return;
                }

                confirmOrderParameters = GetToggleOrderMenuEditModeParameters(document);
            }
            catch (Exception exception)
            {
                throw new GourmetParseException("Error parsing the ordered menu HTML", GetRequestUriString(orderedMenuResponse), orderedMenuHttpContent, exception);
            }

            using var confirmResponse = await ExecutePostRequestForPage(PageNameOrderedMenu, confirmOrderParameters);
        }

        public async Task<IReadOnlyList<BillingPosition>> GetBillingPositions(int month, int year, IProgress<int> progress)
        {
            using var billingResponse = await ExecuteGetRequestForPage(PageNameStart);
            var httpContent = await GetResponseContent(billingResponse);

            GourmetUserInformation userInformation;
            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(httpContent);

                userInformation = ParseHtmlForUserInformation(document);
            }
            catch (Exception exception)
            {
                throw new GourmetParseException("Error parsing the ordered menu HTML", GetRequestUriString(billingResponse), httpContent, exception);
            }

            var inputDate = new DateTime(year, month, 1);
            var currentDate = DateTime.Now;
            int monthsDifference = (currentDate.Year - inputDate.Year) * 12 + currentDate.Month - inputDate.Month;

            var parameters = new Dictionary<string, string>
            {
                // checkLastMonthNumber describes the target month by specifying how many months back the report should be generated
                // This value starts at zero, i.e., "0" means the current month, "1" means one month back, etc.
                {"checkLastMonthNumber", (monthsDifference).ToString()},
                {"eaterId", userInformation.EaterId},
                {"shopModelId", userInformation.ShopModelId}
            };

            try
            {
                using var apiResponse = await ExecuteJsonPostRequest($"{WebUrl}umbraco/api/AlaMyBillingApi/GetMyBillings", parameters);
                return await GetJsonResponseObject(apiResponse, ConvertToBillingPositions);
            }
            finally
            {
                progress.Report(100);
            }

            IReadOnlyList<BillingPosition> ConvertToBillingPositions(JsonElement resultJson)
            {
                var result = new List<BillingPosition>();

                int billingCount = resultJson.GetArrayLength();
                for (int billingIndex = 0; billingIndex < billingCount; billingIndex++)
                {
                    JsonElement billingElement = resultJson[billingIndex];
                    DateTime billDate = billingElement.GetProperty("BillDate").GetDateTime();
                    JsonElement billingItems = billingElement.GetProperty("BillingItemInfo");
                    int billingItemsCount = billingItems.GetArrayLength();

                    for (int itemIndex = 0; itemIndex < billingItemsCount; itemIndex++)
                    {
                        JsonElement itemElement = billingItems[itemIndex];
                        string description = itemElement.GetProperty("Description").GetString();
                        int count = itemElement.GetProperty("Count").GetInt32();
                        double totalCost = itemElement.GetProperty("Total").GetDouble();
                        double subsidy = itemElement.GetProperty("Subsidy").GetDouble();
                        double cost = totalCost - subsidy;

                        result.Add(new BillingPosition(billDate, BillingPositionType.Menu, description, count, cost));
                    }
                }

                return result;
            }
        }

        private async Task<(HtmlDocument Document, string ResultUriInfo, string ResultHttpContent)> EnterOrderedMenuEditMode()
        {
            using var orderedMenuResponse = await ExecuteGetRequestForPage(PageNameOrderedMenu);
            var orderedMenuHttpContent = await GetResponseContent(orderedMenuResponse);

            var orderedMenuDocument = new HtmlDocument();
            orderedMenuDocument.LoadHtml(orderedMenuHttpContent);

            Dictionary<string, string> enterEditModeParameters;

            try
            {
                if (IsOrderedMenuPageEditModeActive(orderedMenuDocument))
                {
                    // Already in edit mode
                    return (orderedMenuDocument, GetRequestUriString(orderedMenuResponse), orderedMenuHttpContent);
                }

                enterEditModeParameters = GetToggleOrderMenuEditModeParameters(orderedMenuDocument);
            }
            catch (Exception exception)
            {
                throw new GourmetParseException("Error parsing the ordered menu HTML", GetRequestUriString(orderedMenuResponse), orderedMenuHttpContent, exception);
            }

            using var enterEditModeResponse = await ExecutePostRequestForPage(PageNameOrderedMenu, enterEditModeParameters);
            var enterEditModeHttpContent = await GetResponseContent(enterEditModeResponse);

            var enterEditModeDocument = new HtmlDocument();
            enterEditModeDocument.LoadHtml(enterEditModeHttpContent);

            if (!IsOrderedMenuPageEditModeActive(enterEditModeDocument))
            {
                throw new GourmetRequestException("Cannot enter edit mode of ordered menus", GetRequestUriString(enterEditModeResponse));
            }

            return (enterEditModeDocument, GetRequestUriString(enterEditModeResponse), enterEditModeHttpContent);
        }

        private async Task<string> GetUfprtValueFromPage(string pageName, string formXPath)
        {
            var response = await ExecuteGetRequestForPage(pageName);
            var httpContent = await GetResponseContent(response);

            var document = new HtmlDocument();
            document.LoadHtml(httpContent);

            var formNode = document.DocumentNode.GetSingleNode(formXPath);
            return ParseUfprtValue(formNode);
        }

        private static string ParseUfprtValue(HtmlNode formNode)
        {
            var ufprtNode = formNode.GetSingleNode(".//input[@name='ufprt']");
            return ufprtNode.Attributes["value"].Value;
        }

        private Task<HttpResponseMessage> ExecuteGetRequestForPage(string pageName, IReadOnlyDictionary<string, string> urlParameters = null)
        {
            return ExecuteGetRequest($"{WebUrl}{pageName}/", urlParameters);
        }

        private Task<HttpResponseMessage> ExecutePostRequestForPage(string pageName, IReadOnlyDictionary<string, string> formParameters)
        {
            return ExecuteFormPostRequest($"{WebUrl}{pageName}/", formParameters);
        }

        private static GourmetUserInformation ParseHtmlForUserInformation(HtmlDocument document)
        {
            var loginNameNode = document.DocumentNode.GetSingleNode("//div[@class='userfield']//span[@class='loginname']");
            var shopModelNode = document.DocumentNode.GetSingleNode("//input[@id='shopModel']");
            var eaterNode = document.DocumentNode.GetSingleNode("//input[@id='eater']");
            var staffGroupNode = document.DocumentNode.GetSingleNode("//input[@id='staffGroup']");

            var nameOfUser = loginNameNode.GetInnerText();
            var shopModelId = shopModelNode.Attributes["value"].Value;
            var eaterId = eaterNode.Attributes["value"].Value;
            var staffGroupId = staffGroupNode.Attributes["value"].Value;

            return new GourmetUserInformation(nameOfUser, shopModelId, eaterId, staffGroupId);
        }

        private static IEnumerable<GourmetMenu> ParseGourmetMenuHtml(HtmlDocument document)
        {
            foreach (var menuNode in document.DocumentNode.GetNodes("//div[@class='meal']"))
            {
                var detailNode = menuNode.GetSingleNode(".//div[@class='open_info menu-article-detail']");
                var positionId = detailNode.Attributes["data-id"].Value;
                var day = ParseMenuDateString(detailNode.Attributes["data-date"].Value);
                var title = menuNode.GetSingleNode(".//div[@class='title']").ChildNodes[0].GetInnerText().Trim();
                var subTitle = menuNode.GetSingleNode(".//div[@class='subtitle']").GetInnerText();
                var allergens = ParseAllergens(menuNode.GetSingleNode(".//li[@class='allergen']").GetInnerText());
                var isAvailable = menuNode.ContainsNode(".//input[@type='checkbox' and @class='menu-clicked']");

                yield return new GourmetMenu(day, positionId, title, subTitle, allergens, isAvailable);
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

        private static bool IsOrderedMenuPageEditModeActive(HtmlDocument document)
        {
            var toggleEditModeParameterNode = document.DocumentNode.GetSingleNode("//form[@id='form_toggleEditMode']//input[@name='editMode' and @type='hidden']");

            // Edit mode is active if value is "False", because when submitting this value would disable the edit mode
            return toggleEditModeParameterNode.Attributes["value"].Value == "False";
        }

        private static Dictionary<string, string> GetToggleOrderMenuEditModeParameters(HtmlDocument document)
        {
            var formNode = document.DocumentNode.GetSingleNode("//form[@id='form_toggleEditMode']");
            var editModeNode = formNode.GetSingleNode(".//input[@name='editMode' and @type='hidden']");

            string editModeValue = editModeNode.Attributes["value"].Value;
            string ufprtValue = ParseUfprtValue(formNode);

            return new Dictionary<string, string>
            {
                {"editMode", editModeValue},
                {"ufprt", ufprtValue}
            };
        }

        private static Dictionary<string, string> GetCancelOrderParameters(HtmlDocument document, string positionId)
        {
            var eatingCycleIdNodeName = $"cp_EatingCycleId_{positionId}";
            var dateNodeName = $"cp_Date_{positionId}";

            var formNode = document.DocumentNode.GetSingleNode($"//form[@id='form_{positionId}_cp']");
            var eatingCycleIdInputNode = formNode.GetSingleNode($".//input[(@name='{eatingCycleIdNodeName}') and @type='hidden']");
            var dateInputNode = formNode.GetSingleNode($".//input[(@name='{dateNodeName}') and @type='hidden']");

            var eatingCycleIdValue = eatingCycleIdInputNode.Attributes["value"].Value;
            var dateValue = dateInputNode.Attributes["value"].Value;
            var ufprtValue = ParseUfprtValue(formNode);

            return new Dictionary<string, string>
            {
                {"cp_PositionId", positionId},
                {eatingCycleIdNodeName, eatingCycleIdValue},
                {dateNodeName, dateValue},
                {"ufprt", ufprtValue}
            };
        }
    }
}
