using HtmlAgilityPack;
using System.Text.RegularExpressions;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CampfireTools.WebScaping 
{
    public class Link
    {
        public string name;
        public string url;

        public Link(string name, string url)
        {
            this.name = name;
            this.url = url;
        }
    }

    public static class Utilities
    {
        public static IWebDriver StartWebDriver(string webDriverExePath) 
        {
            ChromeDriverService service = ChromeDriverService.CreateDefaultService(webDriverExePath);
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless");
            chromeOptions.AddArgument("--disable-gpu");
            chromeOptions.AddArgument("--disable-extensions");
            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("--disable-dev-shm-usage");
            chromeOptions.AddArgument("--verbose");
            chromeOptions.AddArgument("--remote-debugging-port=46391");
            service.HideCommandPromptWindow = true;
            return new ChromeDriver(service, chromeOptions);
        }

        public static bool IsNodeOfClass(HtmlNode node, List<string> classes) 
        {
            return node.GetClasses().Any(c => classes.Contains(c));
        }

        public static void DFS(HtmlNode node, List<string> classes, List<HtmlNode> foundNodes)
        {
            if (IsNodeOfClass(node, classes)) 
            {
                foundNodes.Add(node);
                return;
            }

            foreach (var childNode in node.ChildNodes) 
            {
                DFS(childNode, classes, foundNodes);
            }
        }

        public static List<Link> ExtractLinksFromHtmlCode(string htmlCode, string basePageUrl)
        {
            List<Link> uniqueLinks = new List<Link>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlCode);

            HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//a");

            if (linkNodes != null)
            {
                foreach (HtmlNode linkNode in linkNodes)
                {
                    string url = linkNode.GetAttributeValue("href", string.Empty);
                    string name = linkNode.InnerText.Trim();

                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        // Some links are relative eg. /hello/index.html. In such case add domain part
                        string absoluteUrl = new Uri(new Uri(basePageUrl), url).AbsoluteUri;
                        uniqueLinks.Add(new Link(name, absoluteUrl));
                    }
                }
            }

            return uniqueLinks;
        }

        public static List<string> ExtractWordsFromHtmlCode(string htmlCode)
        {
            List<string> words = new List<string>();
            
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlCode);
            var nodes = doc.DocumentNode.SelectNodes("//text()");
            if (nodes != null) 
            {
                nodes.ToList().ForEach(n => words.Add(n.InnerText.Trim()));
                words.RemoveAll(w => w.Length < 3);
                words.ForEach(w => w.Trim().Replace("\r\n", ""));
            }
            return words;
        }
    }
}