using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace GropMng.Web.Framework.TagHelpers.Admin;

/// <summary>
/// <c>grop-page-header</c> helper for standard admin page titles with optional breadcrumb/actions content.
/// </summary>
[HtmlTargetElement("grop-page-header")]
public class GropPageHeaderTagHelper : TagHelper
{
    [HtmlAttributeName("title")]
    public string Title { get; set; } = string.Empty;

    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        var childContent = await output.GetChildContentAsync();
        var wrapperClass = string.IsNullOrWhiteSpace(CssClass) ? "d-flex align-items-center justify-content-between py-3 mb-4" : $"d-flex align-items-center justify-content-between py-3 mb-4 {CssClass}";

        var html = new StringBuilder();
        html.Append("<div class='row'><div class='col-12'>");
        html.Append($"<div class='{wrapperClass}'>");
        html.Append("<div>");
        html.Append($"<h4 class='fw-bold mb-1'>{Title}</h4>");
        html.Append(childContent.GetContent());
        html.Append("</div></div></div></div>");

        output.TagName = null;
        output.Content.SetHtmlContent(html.ToString());
    }
}