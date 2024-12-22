using System.Text.RegularExpressions;
using AngleSharp.Dom;

namespace App;

public static class HtmlHelper
{
    public static void RemoveEvents(IDocument document)
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
            foreach (var attribute in attributesToRemove) element.RemoveAttribute(attribute.Name);
        }
    }

    public static string RemoveAllWithTag(IDocument document, string tag)
    {
        // Select all <script> tags
        var scriptTags = document.QuerySelectorAll(tag);

        // Remove each <script> tag from the DOM
        foreach (var scriptTag in scriptTags) scriptTag.Remove();

        // Return the modified HTML as a string
        return document.DocumentElement.OuterHtml;
    }
    
    public static void RemoveAllWithTags(IDocument document, string[] tags)
    {
        foreach (var tag in tags)
        {
            // Select all <script> tags
            var scriptTags = document.QuerySelectorAll(tag);

            // Remove each <script> tag from the DOM
            foreach (var scriptTag in scriptTags) scriptTag.Remove();
        }
    }

    public static void CleanWhitespace(INode node)
    {
        if (node is IText textNode)
            // Replace multiple spaces/newlines with a single space
            textNode.TextContent = textNode.TextContent.Trim();
        else
            // Process child nodes recursively
            foreach (var child in node.ChildNodes)
                CleanWhitespace(child);
    }

    public static void RemoveAttributes(IDocument document, List<string> attributes)
    {
        // Select all elements in the document
        var allElements = document.All;

        var attributesToBeRemoved = new HashSet<string>
        {
            "aria-label", "title", "alt"
        };
        
        attributes?.ForEach(x => attributesToBeRemoved.Add(x));

        foreach (var element in allElements)
        {
            // Remove the 'title' attribute
            foreach (var attributeToRemove in attributesToBeRemoved)
                if (element.HasAttribute(attributeToRemove))
                    element.RemoveAttribute(attributeToRemove);

            // Remove all 'data-*' attributes
            var dataAttributes = element.Attributes
                .Where(attr => attr.Name.StartsWith("data-", StringComparison.OrdinalIgnoreCase))
                .ToList(); // Create a copy to avoid modifying the collection while iterating

            foreach (var dataAttribute in dataAttributes) element.RemoveAttribute(dataAttribute.Name);
        }
    }
    
    public static double CalculateAverageDepth(INode node)
    {
        int totalDepth = 0;
        int nodeCount = 0;

        // Recursive function to traverse DOM and calculate depth
        void Traverse(INode currentNode, int currentDepth)
        {
            totalDepth += currentDepth;
            nodeCount++;

            foreach (var child in currentNode.ChildNodes)
            {
                Traverse(child, currentDepth + 1);
            }
        }

        // Start traversal from the root node
        Traverse(node, 0);

        // Calculate average depth
        return nodeCount > 0 ? (double)totalDepth / nodeCount : 0;
    }

    public static void RemoveDeepElements(INode node, int maxDepth)
    {
        // Recursive function to traverse and remove elements
        void TraverseAndRemove(INode currentNode, int currentDepth)
        {
            if (currentDepth > maxDepth && currentNode is IElement element)
            {
                // Remove the element from its parent
                element.Remove();
                return;
            }

            // Traverse child nodes (make a copy of the collection to avoid issues during removal)
            foreach (var child in currentNode.ChildNodes.ToList())
            {
                TraverseAndRemove(child, currentDepth + 1);
            }
        }

        // Start traversal from the root node
        TraverseAndRemove(node, 0);
    }
    
    public static void ReplaceDeepElementsWithText(IElement rootElement, int maxDepth)
    {
        // Recursive function to traverse and replace elements
        void TraverseAndReplace(INode currentNode, int currentDepth)
        {
            if (currentDepth > maxDepth && currentNode is IElement element)
            {
                var text = new string(element.TextContent);
                
                // Remove all child nodes of the element
                while (element.HasChildNodes)
                {
                    element.RemoveChild(element.FirstChild);
                }

                // Set the concatenated text as the content of the element
                element.TextContent = text;

                return;
            }

            // Traverse child nodes (make a copy of the collection to avoid issues during replacement)
            foreach (var child in currentNode.ChildNodes.ToList())
            {
                TraverseAndReplace(child, currentDepth + 1);
            }
        }

        // Start traversal from the root node
        TraverseAndReplace(rootElement, 0);
    }
    
    public static void RemoveEmptyTextNodes(IElement root)
    {
        if (root == null) return;

        var childNodes = root.Children.ToList(); // Create a list to avoid modifying the collection during iteration

        foreach (var child in childNodes)
        {
            // Recursively process child nodes
            RemoveEmptyTextNodes(child);

            // Check if the node's text content is empty or contains only whitespace-like characters
            if (IsTextContentEmpty(child))
            {
                child.Remove();
            }
        }
    }
    
    private static bool IsTextContentEmpty(IElement node)
    {
        // Trim and normalize the text content, then check for displayable symbols
        var normalizedText = Regex.Replace(node.TextContent ?? "", @"\s+", "");
        return string.IsNullOrEmpty(normalizedText);
    }
    
    public static void ReplaceTagsWithText(IElement root, string[] tagNames)
    {
        if (root == null || tagNames == null || tagNames.Length == 0) return;

        foreach (var tagName in tagNames)
        {
            var matchingTags = root.QuerySelectorAll(tagName).ToList(); // Get all matching tags for the current tag name

            foreach (var tag in matchingTags)
            {
                // Replace the tag with its text content
                var parent = tag.Parent;
                if (parent != null)
                {
                    var textNode = root.Owner!.CreateTextNode(tag.TextContent ?? "");
                    parent.ReplaceChild(textNode, tag);
                }
            }
        }
    }
    
    public static void ReplaceNewLinesWithOneLineText(IDocument document)
    {
        // Select all elements
        var elements = document.All;

        // Process each element's text content
        foreach (var element in elements)
        {
            foreach (var child in element.Children)
            {
                if (!child.Children.Any() && !string.IsNullOrWhiteSpace(child.TextContent))
                {
                    // Check if the text contains newlines
                    var text = child.TextContent;
                    if (text.Contains("\n"))
                    {
                        // Replace newlines with spaces and trim the text
                        var singleLineText = string.Join(" ", text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()));

                        // Update the text content
                        child.TextContent = singleLineText.Trim();
                    }
                }
            }
        }
    }
    
    public static void LimitChildrenByClass(IDocument document)
    {
        // Select all elements
        var elements = document.All;

        // Process each element to limit children with the same class attribute to 2
        foreach (var element in elements)
        {
            // Group children by class attribute
            var childrenGroupedByClass = element.Children
                .Where(child => child.HasAttribute("class"))
                .GroupBy(child => child.GetAttribute("class"));

            foreach (var group in childrenGroupedByClass)
            {
                // Skip groups with 2 or fewer children
                if (group.Count() > 2)
                {
                    // Keep only the first two children and remove the rest
                    var childrenToRemove = group.OrderBy(x => x.TextContent.Length).Skip(2).ToList();
                    foreach (var child in childrenToRemove)
                    {
                        child.Remove();
                    }
                }
            }
        }
    }
}