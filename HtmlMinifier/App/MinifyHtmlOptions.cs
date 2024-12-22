namespace App;

public class MinifyHtmlOptions
{
    public bool RemoveScripts { set; get; }

    public bool RemoveNoScripts { set; get; }

    public bool RemoveStyles { set; get; }

    public bool RemoveLinks { set; get; }

    public bool RemoveSvg { set; get; }

    public bool RemoveEmptyTags { set; get; }

    public bool RemoveSelect { set; get; }

    public bool RemoveInput { set; get; }

    public bool RemoveLabel { set; get; }

    public bool RemoveMeta { set; get; }
    
    public bool RemoveCommentsRegex { set; get; }
    
    public bool RemoveWhiteSpaces { set; get; }
    
    public bool AddMarginToDiv { set; get; }
}