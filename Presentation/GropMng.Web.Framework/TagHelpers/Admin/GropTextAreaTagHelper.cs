using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GropMng.Web.Framework.TagHelpers.Admin;

/// <summary>
/// <c>grop-textarea</c> helper with consistent Frest classes.
/// </summary>
[HtmlTargetElement("grop-textarea", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
public class GropTextAreaTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";
    private const string RowsAttributeName = "asp-rows";
    private const string RequiredAttributeName = "asp-required";

    private readonly IHtmlHelper _htmlHelper;

    public GropTextAreaTagHelper(IHtmlHelper htmlHelper)
    {
        _htmlHelper = htmlHelper;
    }

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; } = default!;

    [HtmlAttributeName(RowsAttributeName)]
    public int Rows { get; set; } = 4;

    [HtmlAttributeName(RequiredAttributeName)]
    public bool IsRequired { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        output.SuppressOutput();

        if (_htmlHelper is IViewContextAware viewContextAware)
            viewContextAware.Contextualize(ViewContext);

        var textArea = _htmlHelper.TextArea(For.Name, null, Rows, 0, new { @class = "form-control" });

        if (IsRequired)
        {
            output.PreElement.SetHtmlContent("<div class='input-group input-group-required'>");
            output.PostElement.SetHtmlContent("<div class='input-group-btn'><span class='required'>*</span></div></div>");
        }

        output.Content.SetHtmlContent(textArea);
        return Task.CompletedTask;
    }
}
