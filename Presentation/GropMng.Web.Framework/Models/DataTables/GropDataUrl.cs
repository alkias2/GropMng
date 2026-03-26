namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Represents the AJAX data URL configuration for a DataTables grid.
/// </summary>
public class GropDataUrl
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="GropDataUrl"/> class.
    /// </summary>
    /// <param name="actionName">The action method name in the controller.</param>
    /// <param name="controllerName">The controller name.</param>
    public GropDataUrl(string actionName, string controllerName)
    {
        ActionName = actionName;
        ControllerName = controllerName;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the action method name in the controller.
    /// </summary>
    public string ActionName { get; set; }

    /// <summary>
    /// Gets or sets the controller name.
    /// </summary>
    public string ControllerName { get; set; }

    #endregion
}
