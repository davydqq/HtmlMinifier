// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using App;
using Html2Markdown;

Console.WriteLine("Hello, World!");

var html = File.ReadAllText("sources/auto-ria.html");

Console.WriteLine("Length before: {0}", html.Length);

var options = new MinifyHtmlOptions()
{
    RemoveScripts = true,
    RemoveNoScripts = true,
    RemoveStyles = true,
    RemoveLinks = true,
    RemoveSvg = true,
    RemoveEmptyTags = true,
    RemoveSelect = true,
    RemoveMeta = true,
    RemoveInput = true,
    RemoveLabel = true
};


var converter = new Converter();
string markdown = converter.Convert(html);
File.WriteAllText($"/Users/davydkonopatskyi/Desktop/HtmlMinifier/HtmlMinifier/App/sources/md-{DateTimeOffset.Now.ToString("h:mm:ss")}.md", markdown);

var outHtml = await ProcessHtml(html, options);

Console.WriteLine("Length after: {0}", outHtml.Length);
Console.WriteLine("Win: {0}", 100 -  Math.Round((double)outHtml.Length / html.Length * 100, 2));

File.WriteAllText($"/Users/davydkonopatskyi/Desktop/HtmlMinifier/HtmlMinifier/App/sources/{DateTimeOffset.Now.ToString("h:mm:ss")}.html", outHtml);

async Task<string> ProcessHtml(string inputHtml, MinifyHtmlOptions options)
{
    // Create a new configuration for AngleSharp
    var config = Configuration.Default;

    // Create a new context for parsing HTML
    var context = BrowsingContext.New(config);

    // Parse the HTML string
    var document = await context.OpenAsync(req => req.Content(inputHtml));

    CleanWhitespace(document.DocumentElement);
    RemoveAttributes(document);
    RemoveEvents(document);

    var resultHtml = inputHtml;

    if (options.RemoveScripts)
    {
        resultHtml = RemoveAllWithTag(document, "script");
    }
    if (options.RemoveStyles)
    {
        resultHtml = RemoveAllWithTag(document, "style");
    }
    if (options.RemoveLinks)
    {
        resultHtml = RemoveAllWithTag(document, "link");
    }
    if (options.RemoveNoScripts)
    {
        resultHtml = RemoveAllWithTag(document, "noscript");
    }
    if (options.RemoveSvg)
    {
        resultHtml = RemoveAllWithTag(document, "svg");
    }
    if (options.RemoveSelect)
    {
        resultHtml = RemoveAllWithTag(document, "select");
    }
    if (options.RemoveMeta)
    {
        resultHtml = RemoveAllWithTag(document, "meta");
    }
    if (options.RemoveInput)
    {
        resultHtml = RemoveAllWithTag(document, "input");
    }
    if (options.RemoveLabel)
    {
        resultHtml = RemoveAllWithTag(document, "label");
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

    string commentPattern = @"^\s*<!--.*?-->\s*$";

    resultHtml = string.Join("\n", resultHtml
        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
        .Where(line => !string.IsNullOrWhiteSpace(line) && !Regex.IsMatch(line, commentPattern)));

    return resultHtml;
}

void RemoveEvents(IDocument document)
{
    var elements = document.All;

    // Remove specific attributes
    foreach (var element in elements)
    {
        // Check if the element has any attributes like `onClick`
        var attributesToRemove = element.Attributes
            .Where(attr => attr.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Remove each attribute
        foreach (var attribute in attributesToRemove)
        {
            element.RemoveAttribute(attribute.Name);
        }
    }
}

string RemoveAllWithTag(IDocument document, string tag)
{
    // Select all <script> tags
    var scriptTags = document.QuerySelectorAll(tag);

    // Remove each <script> tag from the DOM
    foreach (var scriptTag in scriptTags)
    {
        scriptTag.Remove();
    }

    // Return the modified HTML as a string
    return document.DocumentElement.OuterHtml;
}

void CleanWhitespace(INode node)
{
    if (node is IText textNode)
    {
        // Replace multiple spaces/newlines with a single space
        textNode.TextContent = textNode.TextContent.Trim();
    }
    else
    {
        // Process child nodes recursively
        foreach (var child in node.ChildNodes)
        {
            CleanWhitespace(child);
        }
    }
}

void RemoveAttributes(IDocument document)
{
    // Select all elements in the document
    var allElements = document.All;

    var attributesToBeRemoved = new HashSet<string>()
    {
        "aria-label", "title", "alt"
    };

    foreach (var element in allElements)
    {
        // Remove the 'title' attribute
        foreach (var attributeToRemove in attributesToBeRemoved)
        {
            if (element.HasAttribute(attributeToRemove))
            {
                element.RemoveAttribute(attributeToRemove);
            }
        }

        // Remove all 'data-*' attributes
        var dataAttributes = element.Attributes
            .Where(attr => attr.Name.StartsWith("data-", StringComparison.OrdinalIgnoreCase))
            .ToList(); // Create a copy to avoid modifying the collection while iterating

        foreach (var dataAttribute in dataAttributes)
        {
            element.RemoveAttribute(dataAttribute.Name);
        }
    }
}