using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using CampfireTools.WebScaping;

namespace CampfireTools.WebScapingTests
{
    [TestFixture]
    public class WebScapingTests
    {
        // --- SubstringFilter ---

        [Test]
        public void FilteringRequiredSubstrings()
        {
            // Arrange
            List<string> expected = new List<string>() { "eeAlphax", "eeBetax" };

            List<string> data = new List<string>() { "eeAlphax", "eeBetax", "eeGamma" };
            List<string> requiredSubstrings = new List<string>() { "ax", "ee" };
            SubstringFilter substringFilter = new SubstringFilter(requiredSubstrings: requiredSubstrings);

            // Act
            List<string> actual = substringFilter.Filter(data);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FilteringForbiddenSubstrings()
        {
            // Arrange
            List<string> expected = new List<string>() { "eeGamma" };

            List<string> data = new List<string>() { "eeAlphax", "eeBetax", "eeGamma" };
            List<string> forbiddenSubstrings = new List<string>() { "ax" };
            SubstringFilter substringFilter = new SubstringFilter(forbiddenSubstrings: forbiddenSubstrings);

            // Act
            List<string> actual = substringFilter.Filter(data);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FilteringRequiredAndForbiddenSubstrings()
        {
            // Arrange
            List<string> expected = new List<string>() { "aabbee", "aabbeett" };

            List<string> data = new List<string>() { "aaeexx", "aabbee", "test", "aabbeett", "aabbeexxtt", "tttxx", "tt" };
            List<string> requiredSubstrings = new List<string>() { "aa", "ee" };
            List<string> forbiddenSubstrings = new List<string>() { "xx" };
            SubstringFilter substringFilter = new SubstringFilter(requiredSubstrings: requiredSubstrings, forbiddenSubstrings: forbiddenSubstrings);

            // Act
            List<string> actual = substringFilter.Filter(data);

            // Assert
            Assert.AreEqual(expected, actual);
        }
   

        // --- HtmlCodeFilter ---

        string htmlCode = "";

        [SetUp]
        public void Setup()
        {
            htmlCode = 
            """
            <!DOCTYPE html>
            <html>
                <head><title>Example HTML Page</title></head>
                <body>
                    <header><h1>Welcome to My Website</h1></header>
                    <div class="Alpha">
                        <div>
                            <p>Hello here</p>
                            <p>Hello Alpha</p>
                        </div>
                        <div class="Beta">
                            <p>Some data</p>
                            <div class="Theta">
                                <p>Test test</p>
                            </div>
                        </div>
                    </div>
                </body>
            </html>
            """;
        }

        [Test]
        public void FilteringAllowedClasses()
        {
            // Arrange
            string expected = 
            """
            <div class="Alpha">
                <div>
                    <p>Hello here</p>
                    <p>Hello Alpha</p>
                </div>
                <div class="Beta">
                    <p>Some data</p>
                    <div class="Theta">
                        <p>Test test</p>
                    </div>
                </div>
            </div>
            """;
            expected = Regex.Replace(expected, @"[\s\r\n]", string.Empty);
            
            List<string> allowedClasses = new List<string>() { "Alpha" };
            HtmlCodeFilter htmlCodeFilter = new HtmlCodeFilter(allowedClasses: allowedClasses);

            // Act
            string actual = htmlCodeFilter.Filter(htmlCode);
            actual = Regex.Replace(actual, @"[\s\r\n]", string.Empty);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FilteringAllowedAndForbiddenClasses()
        {
            // Arrange
            string expected = 
            """
            <div class="Beta">
                <p>Some data</p>
            </div>
            """;
            expected = Regex.Replace(expected, @"[\s\r\n]", string.Empty);
            
            List<string> allowedClasses = new List<string>() { "Beta" };
            List<string> forbiddenClasses = new List<string>() { "Theta" };
            HtmlCodeFilter htmlCodeFilter = new HtmlCodeFilter(allowedClasses: allowedClasses, forbiddenClasses: forbiddenClasses);

            // Act
            string actual = htmlCodeFilter.Filter(htmlCode);
            actual = Regex.Replace(actual, @"[\s\r\n]", string.Empty);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FilteringAllowedAndForbiddenClasses2()
        {
            // Arrange
            string expected = 
            """
            <div class="Alpha">
                <div>
                    <p>Hello here</p>
                    <p>Hello Alpha</p>
                </div>
            </div>
            """;
            expected = Regex.Replace(expected, @"[\s\r\n]", string.Empty);
            
            List<string> allowedClasses = new List<string>() { "Alpha" };
            List<string> forbiddenClasses = new List<string>() { "Beta" };
            HtmlCodeFilter htmlCodeFilter = new HtmlCodeFilter(allowedClasses: allowedClasses, forbiddenClasses: forbiddenClasses);

            // Act
            string actual = htmlCodeFilter.Filter(htmlCode);
            actual = Regex.Replace(actual, @"[\s\r\n]", string.Empty);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FilteringForbiddenClasses()
        {
            // Arrange
            string expected = 
            """
            <!DOCTYPE html>
            <html>
                <head><title>Example HTML Page</title></head>
                <body>
                    <header><h1>Welcome to My Website</h1></header>
                    <div class="Alpha">
                        <div>
                            <p>Hello here</p>
                            <p>Hello Alpha</p>
                        </div>
                    </div>
                </body>
            </html>
            """;

            expected = Regex.Replace(expected, @"[\s\r\n]", string.Empty);
            
            List<string> forbiddenClasses = new List<string>() { "Beta" };
            HtmlCodeFilter htmlCodeFilter = new HtmlCodeFilter(forbiddenClasses: forbiddenClasses);

            // Act
            string actual = htmlCodeFilter.Filter(htmlCode);
            actual = Regex.Replace(actual, @"[\s\r\n]", string.Empty);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        // --- Utilities ---

        [Test]
        public void ExtractLinksFromHtml()
        {
            // Arrange
            string htmlInput = 
            """
            <!DOCTYPE html>
            <html>
                <head><title>Example HTML Page</title></head>
                <body>
                    <header><h1>Welcome to My Website</h1></header>
                    <div class="Alpha">
                        <a href="https://example.com">Link Example</a>
                        <div>
                            <p>Hello here</p>
                            <a href="/tests/xxx/index.html">Link tests</a>
                            <p>Hello Alpha</p>
                        </div>
                        <a href="https://stackoverflow-says-hello.com"/>
                    </div>
                </body>
            </html>
            """;

            List<Link> expected = new List<Link>() 
            { 
                new Link("Link Example", "https://example.com/"), 
                new Link("Link tests", "https://mywebsite.com/tests/xxx/index.html"),
                new Link("Stackoverflow Says Hello", "https://stackoverflow-says-hello.com/")
            };
    
            List<Link> actual = Utilities.ExtractLinksFromHtmlCode(htmlInput, "https://mywebsite.com");
           
            // Assert
            CollectionAssert.AreEqual(expected.Select(i => i.name), actual.Select(i => i.name));
            CollectionAssert.AreEqual(expected.Select(i => i.url), actual.Select(i => i.url));
        }

         [Test]
        public void ExtractWordsFromHtml()
        {
            // Arrange
            string htmlInput = 
            """
            <!DOCTYPE html>
            <html>
                <head><title>Example HTML Page</title></head>
                <body>
                    <header><h1>Welcome to My Website</h1></header>
                    <div class="Alpha">
                        <a href="https://example.com">Link Example</a>
                        <div>
                            <p>Hello here</p>
                            <a href="/tests/xxx/index.html">Link tests</a>
                            <p>Hello Alpha</p>
                        </div>
                        <a href="https://stackoverflow-says-hello.com"/>
                    </div>
                </body>
            </html>
            """;

            List<string> expected = new List<string>() 
            { 
                "Example HTML Page", "Welcome to My Website", "Link Example", "Hello here", "Link tests", "Hello Alpha"
            };
    
            List<string> actual = Utilities.ExtractWordsFromHtmlCode(htmlInput);
           
            // Assert
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}