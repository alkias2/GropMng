using Microsoft.AspNetCore.Routing;

namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Represents the AJAX data URL configuration for a DataTables grid operation.
/// Mirrors the NopCommerce <c>DataUrl</c> class.
/// Supports both MVC action/controller routing and explicit URL strings.
/// </summary>
public class GropDataUrl
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="GropDataUrl"/> class
    /// using MVC action and controller routing.
    /// </summary>
    /// <param name="actionName">The action method name in the controller.</param>
    /// <param name="controllerName">The controller name.</param>
    /// <param name="routeValues">Optional additional route values.</param>
    public GropDataUrl(string actionName, string controllerName,
        RouteValueDictionary routeValues = null)
    {
        ActionName = actionName;
        ControllerName = controllerName;
        RouteValues = routeValues;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GropDataUrl"/> class
    /// using an explicit URL string.
    /// </summary>
    /// <param name="url">The explicit URL (supports app-relative paths starting with "~/").</param>
    public GropDataUrl(string url)
    {
        Url = url;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GropDataUrl"/> class
    /// using an explicit URL string with a data identifier field.
    /// </summary>
    /// <param name="url">The explicit URL.</param>
    /// <param name="dataId">The row data field name appended to the URL.</param>
    public GropDataUrl(string url, string dataId)
    {
        Url = url;
        DataId = dataId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GropDataUrl"/> class
    /// using an explicit URL string with trailing-slash control.
    /// </summary>
    /// <param name="url">The explicit URL.</param>
    /// <param name="trimEnd">When <c>true</c>, the trailing slash is removed from the resolved URL.</param>
    public GropDataUrl(string url, bool trimEnd)
    {
        Url = url;
        TrimEnd = trimEnd;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the action method name in the controller.
    /// Used when routing via MVC conventions.
    /// </summary>
    public string ActionName { get; set; }

    /// <summary>
    /// Gets or sets the controller name.
    /// Used when routing via MVC conventions.
    /// </summary>
    public string ControllerName { get; set; }

    /// <summary>
    /// Gets or sets an explicit URL string (app-relative or absolute).
    /// Used as an alternative to action/controller routing.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets additional MVC route values appended to the generated URL.
    /// </summary>
    public RouteValueDictionary RouteValues { get; set; }

    /// <summary>
    /// Gets or sets the row data field name appended to the resolved URL.
    /// Used for single-item detail/delete URLs that include the entity ID.
    /// </summary>
    public string DataId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the trailing slash should be removed
    /// from the resolved URL.
    /// </summary>
    public bool TrimEnd { get; set; }

    #endregion
}
