using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System.Text;

namespace GropMng.Web.Framework.TagHelpers.Admin;

/// <summary>
/// <c>grop-delete-confirmation</c> helper that renders a delete button with a Bootstrap modal confirmation.
/// </summary>
[HtmlTargetElement("grop-delete-confirmation")]
public class GropDeleteConfirmationTagHelper : TagHelper
{
    private readonly IAntiforgery _antiforgery;
    private readonly IUrlHelperFactory _urlHelperFactory;

    public GropDeleteConfirmationTagHelper(IAntiforgery antiforgery, IUrlHelperFactory urlHelperFactory)
    {
        _antiforgery = antiforgery;
        _urlHelperFactory = urlHelperFactory;
    }

    [HtmlAttributeName("asp-action")]
    public string Action { get; set; } = string.Empty;

    [HtmlAttributeName("asp-controller")]
    public string? Controller { get; set; }

    [HtmlAttributeName("asp-area")]
    public string? Area { get; set; }

    [HtmlAttributeName("asp-route-id")]
    public string? RouteId { get; set; }

    [HtmlAttributeName("button-text")]
    public string ButtonText { get; set; } = "Delete";

    [HtmlAttributeName("confirm-title")]
    public string ConfirmTitle { get; set; } = "Confirm deletion";

    [HtmlAttributeName("confirm-text")]
    public string ConfirmText { get; set; } = "Are you sure you want to delete this item?";

    [HtmlAttributeName("confirm-button-text")]
    public string ConfirmButtonText { get; set; } = "Delete";

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
        var routeValues = new RouteValueDictionary();

        if (!string.IsNullOrWhiteSpace(Area))
            routeValues["area"] = Area;

        if (!string.IsNullOrWhiteSpace(RouteId))
            routeValues["id"] = RouteId;

        var postUrl = urlHelper.Action(Action, Controller, routeValues) ?? "#";
        var antiForgeryToken = _antiforgery.GetAndStoreTokens(ViewContext.HttpContext).RequestToken;
        var modalId = $"gropDeleteModal_{Guid.NewGuid():N}";

        var html = new StringBuilder();
        html.Append($"<button type='button' class='btn btn-danger' data-bs-toggle='modal' data-bs-target='#{modalId}'>");
        html.Append("<i class='bx bx-trash me-1'></i>");
        html.Append(ButtonText);
        html.Append("</button>");

        html.Append($"<div class='modal fade' id='{modalId}' tabindex='-1' aria-hidden='true'>");
        html.Append("<div class='modal-dialog modal-dialog-centered'>");
        html.Append("<div class='modal-content'>");
        html.Append("<div class='modal-header'>");
        html.Append($"<h5 class='modal-title'>{ConfirmTitle}</h5>");
        html.Append("<button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button>");
        html.Append("</div>");
        html.Append("<div class='modal-body'>");
        html.Append($"<p class='mb-0'>{ConfirmText}</p>");
        html.Append("</div>");
        html.Append("<div class='modal-footer'>");
        html.Append("<button type='button' class='btn btn-outline-secondary' data-bs-dismiss='modal'>Cancel</button>");
        html.Append($"<form method='post' action='{postUrl}'>");
        html.Append($"<input type='hidden' name='__RequestVerificationToken' value='{antiForgeryToken}' />");
        if (!string.IsNullOrWhiteSpace(RouteId))
            html.Append($"<input type='hidden' name='id' value='{RouteId}' />");
        html.Append($"<button type='submit' class='btn btn-danger'>{ConfirmButtonText}</button>");
        html.Append("</form>");
        html.Append("</div></div></div></div>");

        output.TagName = null;
        output.Content.SetHtmlContent(html.ToString());
        return Task.CompletedTask;
    }
}