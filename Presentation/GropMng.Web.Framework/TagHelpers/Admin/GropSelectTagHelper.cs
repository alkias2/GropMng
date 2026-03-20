using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GropMng.Web.Framework.TagHelpers.Admin;

/// <summary>
/// <c>grop-select</c> helper for single-select dropdown fields.
/// </summary>
[HtmlTargetElement("grop-select", Attributes = ForAttributeName + "," + ItemsAttributeName, TagStructure = TagStructure.WithoutEndTag)]
public class GropSelectTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";
    private const string ItemsAttributeName = "asp-items";
    private const string RequiredAttributeName = "asp-required";

    private readonly IHtmlHelper _htmlHelper;

    public GropSelectTagHelper(IHtmlHelper htmlHelper)
    {
        _htmlHelper = htmlHelper;
    }

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; } = default!;

    [HtmlAttributeName(ItemsAttributeName)]
    public IEnumerable<SelectListItem> Items { get; set; } = Enumerable.Empty<SelectListItem>();

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

        var attributes = new Dictionary<string, object> { ["class"] = "form-select" };
        var select = _htmlHelper.DropDownList(For.Name, Items, attributes);

        if (IsRequired)
        {
            output.PreElement.SetHtmlContent("<div class='input-group input-group-required'>");
            output.PostElement.SetHtmlContent("<div class='input-group-btn'><span class='required'>*</span></div></div>");
        }

        output.Content.SetHtmlContent(select);
        return Task.CompletedTask;
    }
}
