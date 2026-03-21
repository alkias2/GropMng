using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Web.Framework.Mvc.ModelBinding;
using System.Reflection;

namespace GropMng.Web.Framework.TagHelpers.Admin;

/// <summary>
/// <c>grop-label</c> helper. Renders a label using MVC metadata and Frest-friendly classes.
/// </summary>
[HtmlTargetElement("grop-label", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
public class GropLabelTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    private readonly IHtmlHelper _htmlHelper;
    private readonly ILocalizationService _localizationService;

    public GropLabelTagHelper(IHtmlHelper htmlHelper, ILocalizationService localizationService)
    {
        _htmlHelper = htmlHelper;
        _localizationService = localizationService;
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

        var labelText = ResolveLocalizedLabelText();
        var label = _htmlHelper.Label(For.Name, labelText, new { @class = "form-label" });
        output.Content.SetHtmlContent(label);
        return Task.CompletedTask;
    }

    private string? ResolveLocalizedLabelText()
    {
        var containerType = For.Metadata.ContainerType;
        var propertyName = For.Metadata.PropertyName;

        if (containerType is null || string.IsNullOrWhiteSpace(propertyName))
            return null;

        var propertyInfo = containerType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (propertyInfo is null)
            return null;

        var resourceAttribute = propertyInfo.GetCustomAttribute<GropResourceDisplayNameAttribute>();
        if (resourceAttribute is null || string.IsNullOrWhiteSpace(resourceAttribute.ResourceKey))
            return null;

        return _localizationService.GetResourceAsync(resourceAttribute.ResourceKey).GetAwaiter().GetResult();
    }
}
