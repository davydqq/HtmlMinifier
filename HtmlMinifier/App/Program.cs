// See https://aka.ms/new-console-template for more information

using App;

var html = File.ReadAllText("sources/auto-ria.html");
// await HtmlProcessor.CutHtml(html);

await HtmlProcessor.ModifyElementsById(html, "catalogSearchAT", ElementAction.RemoveOthers);