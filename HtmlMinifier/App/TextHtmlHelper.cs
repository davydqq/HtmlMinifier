using System.Text.RegularExpressions;

namespace App;

public static class TextHtmlHelper
{
    public static string RemoveEmptyLines(this string inputHtml)
    {
        return string.Join("\n", inputHtml
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line)));
    }
    
    public static List<string> SplitLines(this string text)
    {
        return text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }
    
    public static string GetConcatText(this string rawText)
    {
        // Concatenate all text within the element
        var lines = rawText.SplitLines()
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        return lines.Any() ? lines.Aggregate((pv, cr) => pv + ";" + cr) : "";
    }
    
    public static string RemoveCommentsLines(this string inputHtml)
    {
        var commentPattern = @"^\s*<!--.*?-->\s*$";
        
        return string.Join("\n", inputHtml
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !Regex.IsMatch(line, commentPattern)));
    }
}