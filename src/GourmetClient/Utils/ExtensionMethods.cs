using HtmlAgilityPack;
using System.Collections.Generic;
using System;
using System.Net;

namespace GourmetClient.Utils;

public static class ExtensionMethods
{
    public static HtmlNode GetSingleNode(this HtmlNode node, string xpath)
    {
        return node.SelectSingleNode(xpath) ?? throw new InvalidOperationException($"No node found for XPath '{xpath}'");
    }

    public static bool ContainsNode(this HtmlNode node, string xpath)
    {
        return node.SelectSingleNode(xpath) != null;
    }

    public static IEnumerable<HtmlNode> GetNodes(this HtmlNode node, string xpath)
    {
        var nodes = node.SelectNodes(xpath);
        if (nodes != null)
        {
            return nodes;
        }

        return [];
    }

    public static string GetInnerText(this HtmlNode node)
    {
        return WebUtility.HtmlDecode(node.InnerText.Trim());
    }
}