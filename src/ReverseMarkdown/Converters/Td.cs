﻿using HtmlAgilityPack;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ReverseMarkdown.Converters
{
    public class Td : ConverterBase
    {
        public Td(Converter converter) : base(converter)
        {
            var elements = new [] { "td", "th" };

            foreach (var element in elements)
            {
                Converter.Register(element, this);
            }
        }

        public override string Convert(HtmlNode node)
        {
            if (Converter.Config.SlackFlavored)
            {
                throw new SlackUnsupportedTagException(node.Name);
            }
            
            var content = TreatChildren(node)
                .Chomp()
                .Replace(Environment.NewLine, "<br>");

            var colSpan = GetColSpan(node);
            return string.Concat(Enumerable.Repeat($" {content} |", colSpan));
        }

        /// <summary>
        /// Given node within td tag, checks if newline should be prepended. Will not prepend if this is the first node after any whitespace
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool FirstNodeWithinCell(HtmlNode node) {
            var parentName = node.ParentNode.Name;
            // If p is at the start of a table cell, no leading newline
            if (parentName == "td" || parentName == "th") {
                var pNodeIndex = node.ParentNode.ChildNodes.GetNodeIndex(node);
                var firstNodeIsWhitespace = node.ParentNode.FirstChild.Name == "#text" && Regex.IsMatch(node.ParentNode.FirstChild.InnerText, @"^\s*$");
                if (pNodeIndex == 0 || (firstNodeIsWhitespace && pNodeIndex == 1)) return true;
            }
            return false;
        }
        /// <summary>
        /// Given node within td tag, checks if newline should be appended. Will not append if this is the last node before any whitespace
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool LastNodeWithinCell(HtmlNode node) {
            var parentName = node.ParentNode.Name;
            if (parentName == "td" || parentName == "th") {
                var pNodeIndex = node.ParentNode.ChildNodes.GetNodeIndex(node);
                var cellNodeCount = node.ParentNode.ChildNodes.Count;
                var lastNodeIsWhitespace = node.ParentNode.LastChild.Name == "#text" && Regex.IsMatch(node.ParentNode.LastChild.InnerText, @"^\s*$");
                if (pNodeIndex == cellNodeCount - 1 || (lastNodeIsWhitespace && pNodeIndex == cellNodeCount - 2)) return true;
            }
            return false;
        }

        private int GetColSpan(HtmlNode node)
        {
            var colSpan = 1;
            
            if (Converter.Config.TableHeaderColumnSpanHandling && node.Name == "th")
            {
                colSpan = node.GetAttributeValue("colspan", 1);
            }
            return colSpan;
        }
    }
}
