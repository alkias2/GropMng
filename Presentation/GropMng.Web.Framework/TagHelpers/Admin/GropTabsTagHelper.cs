using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace GropMng.Web.Framework.TagHelpers.Admin;

/// <summary>
/// <c>grop-tabs</c> container for Bootstrap tab navigation.
/// </summary>
[HtmlTargetElement("grop-tabs")]
public class GropTabsTagHelper : TagHelper
{
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        var tabs = new List<GropTabItem>();
        context.Items[typeof(GropTabsStorageKey)] = tabs;

        await output.GetChildContentAsync();

        output.TagName = "div";
        output.Attributes.SetAttribute("class", string.IsNullOrWhiteSpace(CssClass) ? "grop-tabs" : $"grop-tabs {CssClass}");

        if (!tabs.Any())
        {
            output.Content.SetHtmlContent(string.Empty);
            return;
        }

        if (!tabs.Any(x => x.Active))
            tabs[0].Active = true;

        var tabsId = $"grop_tabs_{Guid.NewGuid():N}";
        var builder = new StringBuilder();

        builder.Append($"<ul class='nav nav-tabs' role='tablist' id='{tabsId}_nav'>");
        for (var i = 0; i < tabs.Count; i++)
        {
            var tab = tabs[i];
            var tabId = string.IsNullOrWhiteSpace(tab.Id) ? $"{tabsId}_{i}" : tab.Id;
            var activeClass = tab.Active ? "active" : string.Empty;
            var selected = tab.Active ? "true" : "false";

            builder.Append("<li class='nav-item' role='presentation'>");
            builder.Append($"<button class='nav-link {activeClass}' data-bs-toggle='tab' data-bs-target='#{tabId}' type='button' role='tab' aria-selected='{selected}'>");
            builder.Append(tab.Title);
            builder.Append("</button></li>");
        }
        builder.Append("</ul>");

        builder.Append("<div class='tab-content border border-top-0 rounded-bottom p-3'>");
        for (var i = 0; i < tabs.Count; i++)
        {
            var tab = tabs[i];
            var tabId = string.IsNullOrWhiteSpace(tab.Id) ? $"{tabsId}_{i}" : tab.Id;
            var paneClass = tab.Active ? "tab-pane fade show active" : "tab-pane fade";
            builder.Append($"<div class='{paneClass}' id='{tabId}' role='tabpanel'>");
            builder.Append(tab.Content);
            builder.Append("</div>");
        }
        builder.Append("</div>");

        output.Content.SetHtmlContent(builder.ToString());
    }
}

internal sealed class GropTabsStorageKey
{
}

internal sealed class GropTabItem
{
    public string Title { get; set; } = string.Empty;

    public string? Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool Active { get; set; }
}