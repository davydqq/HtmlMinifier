// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using App;

Console.WriteLine("Hello, World!");

var html = File.ReadAllText("sources/auto-ria.html");

Console.WriteLine("Length before: {0}", html.Length);

var options = new MinifyHtmlOptions()
{
    RemoveScripts = true,
    RemoveNoScripts = true,
    RemoveStyles = false,
    RemoveLinks = false,
    RemoveSvg = false,
    RemoveEmptyTags = false,
    RemoveSelect = false,
    RemoveMeta = false,
    RemoveInput = false,
    RemoveLabel = false,
    RemoveCommentsRegex = false,
    RemoveWhiteSpaces = false,
    AddMarginToDiv = true
};


// var converter = new Converter();
// string markdown = converter.Convert(html);
// File.WriteAllText($"/Users/davydkonopatskyi/Desktop/HtmlMinifier/HtmlMinifier/App/sources/md-{DateTimeOffset.Now.ToString("h.mm.ss")}.md", markdown);

if (false)
{
    var outHtml = await ProcessHtml(html, options);
    Console.WriteLine("Length after: {0}", outHtml.Length);
    Console.WriteLine("Win: {0}", 100 -  Math.Round((double)outHtml.Length / html.Length * 100, 2));
// Get the base directory of the executable
    var executionPath = AppDomain.CurrentDomain.BaseDirectory;
    var projectPath = Path.GetFullPath(Path.Combine(executionPath, @"..\..\.."));
    var sourcesDirectory = Path.Combine(projectPath, "sources");
    Directory.CreateDirectory(sourcesDirectory);
    var outputPath = Path.Combine(sourcesDirectory, $"{DateTimeOffset.Now:hh.mm.ss}.html");
    File.WriteAllText(outputPath, outHtml);
   
}

if (true)
{
    // await BreakOnDivs(html, options);
    // Parse the HTML
    var context = BrowsingContext.New(Configuration.Default);
    var document = await context.OpenAsync(req => req.Content(html));
    double averageDepth = HtmlHelper.CalculateAverageDepth(document.DocumentElement);
    Console.WriteLine($"Average HTML Depth: {averageDepth}");
    var depth = (int)(Math.Floor(averageDepth) / 1.5);
    Console.WriteLine($"Depth: {depth}");
    HtmlHelper.ReplaceDeepElementsWithText(document.DocumentElement, maxDepth: depth);
    HtmlHelper.RemoveAllWithTags(document, new []{ "script", "style", "link", "noscript", "svg", "select", "meta", "input", "label"});
    HtmlHelper.RemoveEmptyTextNodes(document.DocumentElement);
    HtmlHelper.RemoveEvents(document);
    HtmlHelper.RemoveAttributes(document);
    HtmlHelper.ReplaceTagsWithText(document.DocumentElement, new[] { "a", "p", "bold", "span", "strong", "italic", "h1", "h2", "h3" });
    HtmlHelper.ReplaceNewLinesWithOneLineText(document);
    
    // var outHtml = await RemoveNotDivsAsync(document.DocumentElement.OuterHtml, options);
    var executionPath = AppDomain.CurrentDomain.BaseDirectory;
    var projectPath = Path.GetFullPath(Path.Combine(executionPath, @"..\..\.."));
    var sourcesDirectory = Path.Combine(projectPath, "output");
    Directory.CreateDirectory(sourcesDirectory);
    var outputPath = Path.Combine(sourcesDirectory, $"{DateTimeOffset.Now:hh.mm.ss}.html");

    var output = document.DocumentElement.OuterHtml;
    output = output.RemoveEmptyLines().RemoveCommentsLines();
    
    File.WriteAllText(outputPath, output);
}


async Task<string> ProcessHtml(string inputHtml, MinifyHtmlOptions options)
{
    // Create a new configuration for AngleSharp
    var config = Configuration.Default;

    // Create a new context for parsing HTML
    var context = BrowsingContext.New(config);

    // Parse the HTML string
    var document = await context.OpenAsync(req => req.Content(inputHtml));

    if (options.RemoveWhiteSpaces)
    {
        HtmlHelper.CleanWhitespace(document.DocumentElement);   
    }
    
    HtmlHelper.RemoveAttributes(document);
    HtmlHelper.RemoveEvents(document);

    var resultHtml = inputHtml;

    if (options.RemoveScripts)
    {
        resultHtml = HtmlHelper.RemoveAllWithTag(document, "script");
    }
    if (options.RemoveStyles)
    {
        resultHtml = HtmlHelper.RemoveAllWithTag(document, "style");
    }
    if (options.RemoveLinks)
    {
        resultHtml = HtmlHelper.RemoveAllWithTag(document, "link");
    }
    if (options.RemoveNoScripts)
    {
        resultHtml = HtmlHelper.RemoveAllWithTag(document, "noscript");
    }
    if (options.RemoveSvg)
    {
        resultHtml = HtmlHelper.RemoveAllWithTag(document, "svg");
    }
    if (options.RemoveSelect)
    {
        resultHtml = HtmlHelper.RemoveAllWithTag(document, "select");
    }
    if (options.RemoveMeta)
    {
        resultHtml = HtmlHelper.RemoveAllWithTag(document, "meta");
    }
    if (options.RemoveInput)
    {
        resultHtml = HtmlHelper.RemoveAllWithTag(document, "input");
    }
    if (options.RemoveLabel)
    {
        resultHtml = HtmlHelper.RemoveAllWithTag(document, "label");
    }
    if (options.AddMarginToDiv)
    {
        // Select all div elements
        var divElements = document.QuerySelectorAll("div");

        // Add margin and background style to each div
        foreach (var div in divElements)
        {
            var textOnly = string.Concat(div.ChildNodes
                .Where(node => node.NodeType == NodeType.Text)
                .Select(node => node.TextContent.Trim()));
            
            var existingStyle = div.GetAttribute("style");
            var additionalStyle = "padding: 15px; border: 3px solid #ababab !important;";
            var updatedStyle = existingStyle == null
                ? additionalStyle
                : existingStyle + " " + additionalStyle;
            div.SetAttribute("style", updatedStyle);
        }

        resultHtml = document.DocumentElement.OuterHtml;
    }
    // todo
    // split content into groups by tag and parent if not exist
    // replace classes with custom
    if (options.RemoveEmptyTags)
    {
        while (true)
        {
            var prev = resultHtml;

            // Define regex pattern for empty tags
            string emptyTagPattern = @"<(\w+)(\s*[^>]*)>\s*</\1>";
            // Remove empty tags
            resultHtml = Regex.Replace(resultHtml, emptyTagPattern, string.Empty);

            // Define regex pattern for tags with only whitespace or new lines inside
            string whitespaceInsideTagPattern = @"<(\w+)(\s*[^>]*)>\s*</\1>";
            // Remove the whitespace or new lines inside the tags
            resultHtml = Regex.Replace(resultHtml, whitespaceInsideTagPattern, @"<$1$2></$1>");

            if (prev == resultHtml)
            {
                break;
            }
        }
    }
    
    if (options.RemoveCommentsRegex)
    {
        resultHtml = resultHtml.RemoveCommentsLines().RemoveEmptyLines();
    }

    return resultHtml;
}
