namespace GropMng.Web.Framework.UI;

/// <summary>
/// Provides per-request Razor UI state helpers similar to the Nop admin helper seam.
/// </summary>
public interface IGropHtmlHelper
{
    /// <summary>
    /// Sets the system name of the admin menu item that should be rendered active and expanded.
    /// </summary>
    /// <param name="systemName">The stable menu system name.</param>
    void SetActiveMenuItemSystemName(string systemName);

    /// <summary>
    /// Gets the system name of the admin menu item that should be rendered active and expanded.
    /// </summary>
    /// <returns>The configured menu system name, or an empty string when none was set.</returns>
    string GetActiveMenuItemSystemName();
}