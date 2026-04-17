using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GropMng.Web.TagHelpers;

/// <summary>
/// Renders a hidden, dismissible Bootstrap 5 alert <c>div</c> that can be targeted
/// by generated DataTables error handlers in <c>Table.cshtml</c>.
/// </summary>
/// <remarks>
/// Usage: <c>&lt;grop-alert asp-alert-id="myTable_error" /&gt;</c>
/// </remarks>
[HtmlTargetElement("grop-alert", Attributes = "asp-alert-id")]
public class GropAlertTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the unique HTML <c>id</c> for the rendered alert element.
    /// </summary>
    [HtmlAttributeName("asp-alert-id")]
    public string AlertId { get; set; } = string.Empty;

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("id", AlertId);
        output.Attributes.SetAttribute("class", "alert alert-danger alert-dismissible d-none mt-2");
        output.Attributes.SetAttribute("role", "alert");

        output.Content.SetHtmlContent(
            "<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"alert\" aria-label=\"Close\"></button>" +
            "<span class=\"grop-alert-message\"></span>");
    }
}
