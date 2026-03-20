using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GropMng.Web.Framework.TagHelpers.Admin;

/// <summary>
/// <c>grop-label</c> helper. Renders a label using MVC metadata and Frest-friendly classes.
/// </summary>
[HtmlTargetElement("grop-label", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
public class GropLabelTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    private readonly IHtmlHelper _htmlHelper;

    public GropLabelTagHelper(IHtmlHelper htmlHelper)
    {
        _htmlHelper = htmlHelper;
    }

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; } = default!;

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

        var label = _htmlHelper.Label(For.Name, null, new { @class = "form-label" });
        output.Content.SetHtmlContent(label);
        return Task.CompletedTask;
    }
}
