using AngleSharp;
using AngleSharp.Dom;

namespace App;

public static class HtmlProcessor
{
    public static async Task CutHtml(string html)
    {
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
        HtmlHelper.RemoveAttributes(document, new List<string>() { "style" });
        HtmlHelper.LimitChildrenByClass(document);
        HtmlHelper.ReplaceTagsWithText(document.DocumentElement, new[] { "a", "p", "bold", "span", "strong", "italic", "h1", "h2", "h3" });
        HtmlHelper.ReplaceNewLinesWithOneLineText(document);
    
        var output = document.DocumentElement.OuterHtml.RemoveEmptyLines().RemoveCommentsLines();

        // todo one line html for gpt;
        
        SaveFile(output);
    }

    // additionalStyle = padding: 15px; border: 3px solid #ababab !important;
    public static void SetStylesForElement(IElement element, string additionalStyle)
    {
        var existingStyle = element.GetAttribute("style");
        
        var updatedStyle = existingStyle == null
            ? additionalStyle
            : existingStyle + " " + additionalStyle;
        
        element.SetAttribute("style", updatedStyle);
    }
    
    private static void SaveFile(string content)
    {
        var executionPath = AppDomain.CurrentDomain.BaseDirectory;
        var projectPath = Path.GetFullPath(Path.Combine(executionPath, @"..\..\.."));
        var sourcesDirectory = Path.Combine(projectPath, "output");
        Directory.CreateDirectory(sourcesDirectory);
        var outputPath = Path.Combine(sourcesDirectory, $"{DateTimeOffset.Now:hh.mm.ss}.html");
        File.WriteAllText(outputPath, content);
    }
}