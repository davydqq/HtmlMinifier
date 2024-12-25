// See https://aka.ms/new-console-template for more information

using App;

var html = File.ReadAllText("sources/coin-market-cap.html");
// await HtmlProcessor.CutHtml(html);

await HtmlProcessor.SelectHtml(html, "div.cmc-body-wrapper", ElementAction.SetDisplayNone);