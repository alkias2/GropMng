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
/// Renders a reusable confirmation modal for an existing action button, following the Nop admin pattern.
/// </summary>
/// <remarks>
/// When <see cref="Action" /> is provided, the confirmation button posts to the target MVC action.
/// Otherwise, the generated confirmation button can be handled from page-specific JavaScript.
/// </remarks>
[HtmlTargetElement("grop-action-confirmation", Attributes = "asp-button-id")]
public class GropActionConfirmationTagHelper : TagHelper
{
    #region Fields

    private readonly IAntiforgery _antiforgery;
    private readonly IUrlHelperFactory _urlHelperFactory;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="GropActionConfirmationTagHelper" /> class.
    /// </summary>
    /// <param name="antiforgery">The antiforgery service.</param>
    /// <param name="urlHelperFactory">The MVC URL helper factory.</param>
    public GropActionConfirmationTagHelper(IAntiforgery antiforgery, IUrlHelperFactory urlHelperFactory)
    {
        _antiforgery = antiforgery;
        _urlHelperFactory = urlHelperFactory;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the ID of the button that should open the confirmation modal.
    /// </summary>
    [HtmlAttributeName("asp-button-id")]
    public string ButtonId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MVC action to post to after confirmation.
    /// </summary>
    [HtmlAttributeName("asp-action")]
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets the MVC controller name.
    /// </summary>
    [HtmlAttributeName("asp-controller")]
    public string? Controller { get; set; }

    /// <summary>
    /// Gets or sets the MVC area name.
    /// </summary>
    [HtmlAttributeName("asp-area")]
    public string? Area { get; set; }

    /// <summary>
    /// Gets or sets the optional route ID sent with the confirmation request.
    /// </summary>
    [HtmlAttributeName("asp-route-id")]
    public string? RouteId { get; set; }

    /// <summary>
    /// Gets or sets the modal title text.
    /// Optional advanced override. In normal usage, the default generic text should be enough.
    /// </summary>
    [HtmlAttributeName("asp-confirm-title")]
    public string ConfirmTitle { get; set; } = "Are you sure?";

    /// <summary>
    /// Gets or sets the confirmation body text.
    /// Optional advanced override. In normal usage, the default generic text should be enough.
    /// </summary>
    [HtmlAttributeName("asp-confirm-text")]
    public string ConfirmText { get; set; } = "Are you sure you want to perform this action?";

    /// <summary>
    /// Gets or sets the confirm button label.
    /// Optional advanced override. Default follows the Nop-style generic action pattern.
    /// </summary>
    [HtmlAttributeName("asp-confirm-button-text")]
    public string ConfirmButtonText { get; set; } = "Yes";

    /// <summary>
    /// Gets or sets the cancel button label.
    /// Optional advanced override. Default follows the Nop-style generic action pattern.
    /// </summary>
    [HtmlAttributeName("asp-cancel-button-text")]
    public string CancelButtonText { get; set; } = "No, cancel";

    /// <summary>
    /// Gets or sets the current Razor view context.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    #endregion

    #region Methods

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        var sanitizedButtonId = ButtonId?.Trim();
        if (string.IsNullOrWhiteSpace(sanitizedButtonId))
        {
            output.SuppressOutput();
            return;
        }

        var modalId = $"{sanitizedButtonId}-action-confirmation";
        var submitButtonId = $"{sanitizedButtonId}-action-confirmation-submit-button";
        var html = new StringBuilder();

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
        html.Append($"<button type='button' class='btn btn-outline-secondary' data-bs-dismiss='modal'>{CancelButtonText}</button>");

        if (!string.IsNullOrWhiteSpace(Action))
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            var routeValues = new RouteValueDictionary();

            if (!string.IsNullOrWhiteSpace(Area))
            {
                routeValues["area"] = Area;
            }

            if (!string.IsNullOrWhiteSpace(RouteId))
            {
                routeValues["id"] = RouteId;
            }

            var postUrl = urlHelper.Action(Action, Controller, routeValues) ?? "#";
            var antiForgeryToken = _antiforgery.GetAndStoreTokens(ViewContext.HttpContext).RequestToken;

            html.Append($"<form method='post' action='{postUrl}' class='d-inline'>");
            html.Append($"<input type='hidden' name='__RequestVerificationToken' value='{antiForgeryToken}' />");
            if (!string.IsNullOrWhiteSpace(RouteId))
            {
                html.Append($"<input type='hidden' name='id' value='{RouteId}' />");
            }
            html.Append($"<button type='submit' id='{submitButtonId}' class='btn btn-danger'>{ConfirmButtonText}</button>");
            html.Append("</form>");
        }
        else
        {
            html.Append($"<button type='button' id='{submitButtonId}' class='btn btn-danger' data-bs-dismiss='modal'>{ConfirmButtonText}</button>");
        }

        html.Append("</div></div></div></div>");
        html.Append("<script>");
        html.Append("(function(){");
        html.Append("function bindConfirmationTrigger(){");
        html.Append($"var trigger=document.getElementById('{sanitizedButtonId}');");
        html.Append($"var modalElement=document.getElementById('{modalId}');");
        html.Append("if(!trigger||!modalElement||!window.bootstrap){return;}");
        html.Append("if(trigger.dataset.gropConfirmationBound==='true'){return;}");
        html.Append("trigger.dataset.gropConfirmationBound='true';");
        html.Append("trigger.addEventListener('click', function(e){ e.preventDefault(); window.bootstrap.Modal.getOrCreateInstance(modalElement).show(); });");
        html.Append("}");
        html.Append("if(document.readyState==='loading'){document.addEventListener('DOMContentLoaded', bindConfirmationTrigger);}else{bindConfirmationTrigger();}");
        html.Append("})();");
        html.Append("</script>");

        output.TagName = null;
        output.Content.SetHtmlContent(html.ToString());
    }

    #endregion
}
