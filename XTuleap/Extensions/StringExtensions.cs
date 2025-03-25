using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace XTuleap.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        ///    Checks if the string is an HTML string.
        /// </summary>
        /// <param name="input">input string.</param>
        /// <returns>True if the string is HTML, false otherwise.</returns>
        public static bool IsHtml(this string input)
        {
            var lDoc = new HtmlDocument();
            lDoc.LoadHtml(input);
            return lDoc.DocumentNode.FirstChild != null && lDoc.DocumentNode.FirstChild.NodeType == HtmlNodeType.Element;
        }
    }
}
