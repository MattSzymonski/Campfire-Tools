using System;
using System.Threading;
using OpenQA.Selenium;

namespace CampfireTools.WebScaping 
{
    public enum CrawlingBehaviour
    {
        None,
        Scroll,
        PageSwitch
    }

    // Crawls page with items listing either by scrolling (eg. facebook) or switch to the next page (eg. allegro)
    public class PageCrawler
    {
        public string Url { get; }
        public CrawlingBehaviour CrawlingBehaviour { get; }
        public int FirstPageValue { get; private set; }
        public string NextPageButtonLink { get; }
        public int NextPageIncrement { get; }
        public int NextPageMaxValue { get; }
        public int CurrentCrawlValue { get; private set; }
        public string CurrentCrawlUrl { get; private set; }

        bool FirstPage { get; set; }

        public PageCrawler(string url, CrawlingBehaviour crawlingBehaviour, int firstPageValue = 1, string nextPageButtonLink = "", int nextPageIncrement = 1, int nextPageMaxValue = 3)
        {
            Url = url;
            CrawlingBehaviour = crawlingBehaviour; 
            FirstPageValue = firstPageValue;
            NextPageButtonLink = nextPageButtonLink; // Link to the next page in formattable form where parameter is page number (eg. 1, 2, 3) of items number (eg. 10, 20, ..)
            NextPageIncrement = nextPageIncrement; // By how many increment (for pages use 1, for items use eg. 10
            NextPageMaxValue = nextPageMaxValue; // When to end switching (for pages it is the number of last page, for items is the number of last item)
            FirstPage = true; // For PageSwitch when crawling forward for the first time do not increment current page
            CurrentCrawlValue = 0;
            CurrentCrawlUrl = url;
        }

        public string GetHtmlCode(IWebDriver webDriver)
        {
            return webDriver.PageSource;
        }

        public bool CrawlForward(IWebDriver webDriver)
        {
            if (CrawlingBehaviour == CrawlingBehaviour.Scroll)
            {
                webDriver.Navigate().GoToUrl(CurrentCrawlUrl);
                for (int i = 0; i < 5; i++)
                {
                    ((IJavaScriptExecutor)webDriver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                    Thread.Sleep(2000); // Wait for the dynamically loaded content to load
                }
                Thread.Sleep(5000);
                return false;
            }
            else if (CrawlingBehaviour == CrawlingBehaviour.PageSwitch)
            {
                if (CurrentCrawlValue >= NextPageMaxValue)
                {
                    return false;
                }

                if (FirstPage) 
                {
                    FirstPage = false;
                }
                else
                {
                    CurrentCrawlValue += NextPageIncrement;
                }

                CurrentCrawlUrl = string.Format(NextPageButtonLink, CurrentCrawlValue);
                webDriver.Navigate().GoToUrl(CurrentCrawlUrl);

                return true;
            }
            else if (CrawlingBehaviour == CrawlingBehaviour.None)
            {
                webDriver.Navigate().GoToUrl(CurrentCrawlUrl);
                return false;
            }

            throw new Exception("Crawling behaviour not supported");
        }
    }
}