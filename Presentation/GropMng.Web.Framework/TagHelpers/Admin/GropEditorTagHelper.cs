using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GropMng.Web.Framework.TagHelpers.Admin;

/// <summary>
/// <c>grop-editor</c> helper. Wraps Html.Editor and applies a Frest-compatible form-control class.
/// </summary>
[HtmlTargetElement("grop-editor", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
public class GropEditorTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";
    private const string RequiredAttributeName = "asp-required";
    private const string TemplateAttributeName = "asp-template";
    private const string HtmlAttributesAttributeName = "html-attributes";

    private readonly IHtmlHelper _htmlHelper;

    public GropEditorTagHelper(IHtmlHelper htmlHelper)
    {
        _htmlHelper = htmlHelper;
    }

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; } = default!;

    [HtmlAttributeName(RequiredAttributeName)]
    public bool IsRequired { get; set; }

    [HtmlAttributeName(TemplateAttributeName)]
    public string? Template { get; set; }

    [HtmlAttributeName(HtmlAttributesAttributeName)]
    public object? HtmlAttributes { get; set; }

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

        var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(HtmlAttributes ?? new { });
        if (!attributes.ContainsKey("class"))
            attributes["class"] = "form-control";
        else
            attributes["class"] = attributes["class"] + " form-control";

        var editor = _htmlHelper.Editor(For.Name, Template, new { htmlAttributes = attributes });

        if (IsRequired)
        {
            output.PreElement.SetHtmlContent("<div class='input-group input-group-required'>");
            output.PostElement.SetHtmlContent("<div class='input-group-btn'><span class='required'>*</span></div></div>");
        }

        output.Content.SetHtmlContent(editor);
        return Task.CompletedTask;
    }
}
