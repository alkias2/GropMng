using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GropMng.Web.Framework.TagHelpers.Admin;

/// <summary>
/// <c>grop-tab</c> item used inside <c>grop-tabs</c>.
/// </summary>
[HtmlTargetElement("grop-tab", ParentTag = "grop-tabs")]
public class GropTabTagHelper : TagHelper
{
    [HtmlAttributeName("title")]
    public string Title { get; set; } = string.Empty;

    [HtmlAttributeName("id")]
    public string? Id { get; set; }

    [HtmlAttributeName("active")]
    public bool Active { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (!context.Items.TryGetValue(typeof(GropTabsStorageKey), out var value) || value is not List<GropTabItem> tabs)
            throw new InvalidOperationException("grop-tab must be placed inside grop-tabs.");

        var childContent = await output.GetChildContentAsync();

        tabs.Add(new GropTabItem
        {
            Title = Title,
            Id = Id,
            Active = Active,
            Content = childContent.GetContent()
        });

        output.SuppressOutput();
    }
}