using HtmlAgilityPack;
using AngleSharp.Html.Parser;
using AngleSharp.Html;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CampfireTools.WebScaping 
{
    public class HtmlCodeFilter
    {
        public List<string>? AllowedClasses { get; }
        public List<string>? ForbiddenClasses { get; }

        private HtmlParser parser = new HtmlParser();
        private PrettyMarkupFormatter formatter = new PrettyMarkupFormatter() { Indentation = "    " };
        private StringWriter writer = new StringWriter();

        public HtmlCodeFilter(List<string>? allowedClasses = null, List<string>? forbiddenClasses = null)
        {
            AllowedClasses = allowedClasses;
            ForbiddenClasses = forbiddenClasses;
        }

        // Filter html code in a way that only sections of required classes are returned and sections of forbidden classes are removed
        public string Filter(string htmlCode)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlCode);
            HtmlNode rootNode = doc.DocumentNode; 

            // Remove all nodes of not allowed class except those which are descendants of nodes of allowed class
            if (AllowedClasses?.Count > 0) 
            {
                List<HtmlNode> filteredNodes = new List<HtmlNode>();    
                Utilities.DFS(rootNode, AllowedClasses, filteredNodes);
                HtmlNode rootNodeFiltered = HtmlNode.CreateNode("<div></div>");
                filteredNodes.ForEach(node => rootNodeFiltered.AppendChild(node));
                rootNode = rootNodeFiltered;
            }

            // Remove all nodes of not allowed class
            if (ForbiddenClasses?.Count > 0)
            {
                foreach (var node in rootNode.OwnerDocument.DocumentNode.Descendants().Reverse())
                {
                    if (Utilities.IsNodeOfClass(node, ForbiddenClasses)) 
                    {
                        node.Remove();
                    }
                }
            }

            return rootNode.InnerHtml; 
        }
    }
}

