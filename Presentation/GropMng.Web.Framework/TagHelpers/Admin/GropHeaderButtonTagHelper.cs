using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Framework.TagHelpers.Admin;

/// <summary>
/// Represents a child action element inside <c>grop-admin-page-header</c>.
/// This tag helper is collected by the parent and is not rendered directly.
/// </summary>
[HtmlTargetElement("grop-header-button", ParentTag = "grop-admin-page-header")]
public class GropHeaderButtonTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the optional HTML element id.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the button/anchor label text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional icon CSS classes (for example, <c>bx bx-save</c>).
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the CSS classes applied to the rendered action element.
    /// </summary>
    public string CssClass { get; set; } = "btn btn-primary";

    /// <summary>
    /// Gets or sets the optional href. When set, the action renders as an anchor.
    /// </summary>
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets the button type when rendering as <c>&lt;button&gt;</c>.
    /// </summary>
    public string Type { get; set; } = "button";

    /// <summary>
    /// Gets or sets the target form id when rendering as <c>&lt;button&gt;</c>.
    /// </summary>
    public string? Form { get; set; }

    /// <summary>
    /// Gets or sets custom data attributes using the <c>button-data-*</c> prefix.
    /// Example: <c>button-data-confirm="true"</c>.
    /// </summary>
    [HtmlAttributeName(DictionaryAttributePrefix = "button-data-")]
    public IDictionary<string, string> ButtonData { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Suppresses direct output and registers this action in the shared parent context.
    /// </summary>
    /// <param name="context">The tag helper execution context.</param>
    /// <param name="output">The tag helper output.</param>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.SuppressOutput();

        if (!context.Items.TryGetValue(typeof(GropHeaderButtonTagHelper), out var bucket) || bucket is not List<GropHeaderButtonTagHelper> buttons)
        {
            buttons = new List<GropHeaderButtonTagHelper>();
            context.Items[typeof(GropHeaderButtonTagHelper)] = buttons;
        }

        buttons.Add(this);
    }
}


/// <summary>
/// Renders the admin page header including title, breadcrumbs, and action buttons.
/// </summary>
[HtmlTargetElement("grop-admin-page-header")]
[RestrictChildren("grop-header-button")]
public class GropAdminPageHeaderTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the header title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the breadcrumb items rendered above the title row.
    /// </summary>
    public List<AdminBreadcrumbItemModel> Breadcrumbs { get; set; } = new();

    /// <summary>
    /// Builds and renders the complete admin header markup.
    /// </summary>
    /// <param name="context">The tag helper execution context.</param>
    /// <param name="output">The tag helper output.</param>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Initialize shared bucket for child action tag helpers.
        context.Items[typeof(GropHeaderButtonTagHelper)] = new List<GropHeaderButtonTagHelper>();

        // Execute children so they can register themselves in the shared bucket.
        await output.GetChildContentAsync();

        var buttons = (List<GropHeaderButtonTagHelper>)context.Items[typeof(GropHeaderButtonTagHelper)];

        // Render final container markup.
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "row");

        var htmlContent = BuildContent(buttons);
        output.Content.SetHtmlContent(htmlContent);
    }

    /// <summary>
    /// Creates the composed header HTML content using breadcrumbs and collected actions.
    /// </summary>
    /// <param name="buttons">Collected child action definitions.</param>
    /// <returns>The rendered header content.</returns>
    private IHtmlContent BuildContent(List<GropHeaderButtonTagHelper> buttons)
    {
        var content = new HtmlContentBuilder();
        var col = new TagBuilder("div");
        col.AddCssClass("col-12");

        // Breadcrumb section.
        if (Breadcrumbs.Any())
        {
            var nav = new TagBuilder("nav");
            nav.AddCssClass("mb-3");
            nav.Attributes["aria-label"] = "breadcrumb";

            var ol = new TagBuilder("ol");
            ol.AddCssClass("breadcrumb breadcrumb-style1 mb-0");

            for (var i = 0; i < Breadcrumbs.Count; i++)
            {
                var item = Breadcrumbs[i];
                var isLast = i == Breadcrumbs.Count - 1;

                if (!string.IsNullOrWhiteSpace(item.Url) && !isLast)
                {
                    var li = new TagBuilder("li");
                    li.AddCssClass("breadcrumb-item");

                    var a = new TagBuilder("a");
                    a.Attributes["href"] = item.Url!;
                    a.InnerHtml.Append(item.Text);

                    li.InnerHtml.AppendHtml(a);
                    ol.InnerHtml.AppendHtml(li);
                }
                else
                {
                    var li = new TagBuilder("li");
                    li.AddCssClass("breadcrumb-item active");
                    li.InnerHtml.Append(item.Text);
                    ol.InnerHtml.AppendHtml(li);
                }
            }

            nav.InnerHtml.AppendHtml(ol);
            col.InnerHtml.AppendHtml(nav);
        }

        // Title and actions row.
        var headerRow = new TagBuilder("div");
        headerRow.AddCssClass("d-flex flex-column flex-md-row justify-content-between align-items-start align-items-md-center py-3 mb-4");

        var titleWrap = new TagBuilder("div");
        var title = new TagBuilder("h4");
        title.AddCssClass("fw-bold mb-0");
        title.InnerHtml.Append(Title);
        titleWrap.InnerHtml.AppendHtml(title);
        headerRow.InnerHtml.AppendHtml(titleWrap);

        if (buttons.Any())
        {
            var buttonsWrap = new TagBuilder("div");
            buttonsWrap.AddCssClass("d-flex align-content-center flex-wrap gap-2 mt-3 mt-md-0");

            foreach (var btn in buttons)
            {
                buttonsWrap.InnerHtml.AppendHtml(BuildButton(btn));
            }

            headerRow.InnerHtml.AppendHtml(buttonsWrap);
        }

        col.InnerHtml.AppendHtml(headerRow);
        content.AppendHtml(col);
        return content;
    }

    /// <summary>
    /// Builds a single action element as either <c>&lt;a&gt;</c> or <c>&lt;button&gt;</c>.
    /// </summary>
    /// <param name="btn">The action metadata captured from the child tag helper.</param>
    /// <returns>A configured tag builder instance for the action element.</returns>
    private static TagBuilder BuildButton(GropHeaderButtonTagHelper btn)
    {
        var isAnchor = !string.IsNullOrEmpty(btn.Href);
        var element = new TagBuilder(isAnchor ? "a" : "button");

        if (!string.IsNullOrWhiteSpace(btn.Id))
            element.Attributes["id"] = btn.Id;

        if (!string.IsNullOrWhiteSpace(btn.CssClass))
            element.AddCssClass(btn.CssClass);

        if (!string.IsNullOrEmpty(btn.Href))
        {
            element.Attributes["href"] = btn.Href;
        }
        else
        {
            element.Attributes["type"] = string.IsNullOrWhiteSpace(btn.Type) ? "button" : btn.Type;
            if (!string.IsNullOrWhiteSpace(btn.Form))
                element.Attributes["form"] = btn.Form;
        }

        if (btn.ButtonData.Count > 0)
        {
            foreach (var attribute in btn.ButtonData)
            {
                if (string.IsNullOrWhiteSpace(attribute.Key))
                    continue;

                element.Attributes[$"data-{attribute.Key}"] = attribute.Value ?? string.Empty;
            }
        }

        if (!string.IsNullOrWhiteSpace(btn.Icon))
        {
            var icon = new TagBuilder("i");
            icon.AddCssClass($"{btn.Icon} me-1");
            element.InnerHtml.AppendHtml(icon);
            element.InnerHtml.Append(" ");
        }

        element.InnerHtml.Append(btn.Text);
        return element;
    }
}