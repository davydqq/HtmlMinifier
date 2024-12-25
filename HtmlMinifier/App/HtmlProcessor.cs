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
        var depth = (int)(Math.Floor(averageDepth) / 1.2);
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
        
        SaveFile(output, "");
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
    
    public static async Task SelectHtml(string htmlContent, string selector, ElementAction action)
    {
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(htmlContent));

        // Find the element by ID
        var targetElement = document.QuerySelector(selector);
        if (targetElement == null)
        {
            Console.WriteLine($"Element with ID '{selector}' not found.");
            return;
        }

        // Get all parent elements and the target element's descendants
        var parentsAndDescendants = GetAllParentsAndDescendants(targetElement);

        // Get all <style> elements and their parents
        var styleElementsAndParents = new HashSet<IElement>();
        var styleElements = document.QuerySelectorAll("style");
        foreach (var styleElement in styleElements)
        {
            styleElementsAndParents.Add(styleElement);
            var parent = styleElement.ParentElement;
            while (parent != null)
            {
                styleElementsAndParents.Add(parent);
                parent = parent.ParentElement;
            }
        }
        
        parentsAndDescendants.UnionWith(styleElementsAndParents);
        
        // Get all elements
        var allElements = document.All.ToList();

        foreach (var element in allElements)
        {
            // Skip the target element, its parents, and its descendants
            if (parentsAndDescendants.Contains(element))
                continue;

            if (element.TagName.ToLower() == "style")
            {
                continue;
            }
            
            switch (action)
            {
                case ElementAction.SetOpacity:
                    SetStylesForElement(element, "opacity: 0;");
                    break;
                case ElementAction.SetDisplayNone:
                    SetStylesForElement(element, "display: none;");
                    break;
                case ElementAction.RemoveOthers:
                    element.Remove();
                    break;
            }
        }
        
        SaveFile(document.DocumentElement.OuterHtml, $"{selector}.{action.ToString()}");
    }
    
    private static HashSet<IElement> GetAllParentsAndDescendants(IElement element)
    {
        var result = new HashSet<IElement>();

        // Add the element itself
        result.Add(element);

        // Add all parents
        var current = element.ParentElement;
        while (current != null)
        {
            result.Add(current);
            current = current.ParentElement;
        }

        // Add all descendants
        AddDescendants(element, result);

        return result;
    }
    
    private static void AddDescendants(IElement element, HashSet<IElement> result)
    {
        foreach (var child in element.Children)
        {
            result.Add(child);
            AddDescendants(child, result);
        }
    }
    
    private static void SaveFile(string content, string fileNamePrefix)
    {
        var executionPath = AppDomain.CurrentDomain.BaseDirectory;
        var projectPath = Path.GetFullPath(Path.Combine(executionPath, @"..\..\.."));
        var sourcesDirectory = Path.Combine(projectPath, "output");
        Directory.CreateDirectory(sourcesDirectory);
        var outputPath = Path.Combine(sourcesDirectory, $"{fileNamePrefix}.{DateTimeOffset.Now:hh.mm.ss}.html");
        File.WriteAllText(outputPath, content);
    }
}