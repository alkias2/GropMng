using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace GropMng.Web.Framework.TagHelpers.Admin;

/// <summary>
/// <c>grop-card</c> helper that renders a Frest-compatible card with optional header and collapsible body.
/// </summary>
[HtmlTargetElement("grop-card")]
public class GropCardTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets card title.
    /// </summary>
    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for outer card element.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <summary>
    /// Gets or sets whether the card body can be collapsed.
    /// </summary>
    [HtmlAttributeName("collapsible")]
    public bool Collapsible { get; set; }

    /// <summary>
    /// Gets or sets whether card is initially collapsed when collapsible is enabled.
    /// </summary>
    [HtmlAttributeName("collapsed")]
    public bool Collapsed { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        var childContent = await output.GetChildContentAsync();
        var cardClass = string.IsNullOrWhiteSpace(CssClass) ? "card" : $"card {CssClass}";

        output.TagName = "div";
        output.Attributes.SetAttribute("class", cardClass);

        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(Title) || Collapsible)
        {
            builder.Append("<div class='card-header d-flex align-items-center'>");

            if (!string.IsNullOrWhiteSpace(Title))
                builder.Append($"<h5 class='card-title mb-0 me-auto'>{Title}</h5>");
            else
                builder.Append("<div class='me-auto'></div>");

            if (Collapsible)
            {
                var collapseId = $"grop_card_{Guid.NewGuid():N}";
                var expanded = !Collapsed;
                builder.Append($"<button class='btn btn-sm btn-outline-secondary' type='button' data-bs-toggle='collapse' data-bs-target='#{collapseId}' aria-expanded='{expanded.ToString().ToLowerInvariant()}'>");
                builder.Append("<i class='bx bx-chevron-down'></i>");
                builder.Append("</button>");
                builder.Append("</div>");
                builder.Append($"<div id='{collapseId}' class='collapse {(expanded ? "show" : string.Empty)}'>");
                builder.Append("<div class='card-body'>");
                builder.Append(childContent.GetContent());
                builder.Append("</div></div>");
            }
            else
            {
                builder.Append("</div>");
                builder.Append("<div class='card-body'>");
                builder.Append(childContent.GetContent());
                builder.Append("</div>");
            }
        }
        else
        {
            builder.Append("<div class='card-body'>");
            builder.Append(childContent.GetContent());
            builder.Append("</div>");
        }

        output.Content.SetHtmlContent(builder.ToString());
    }
}