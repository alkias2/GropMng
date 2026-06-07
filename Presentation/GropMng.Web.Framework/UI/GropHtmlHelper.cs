namespace GropMng.Web.Framework.UI;

/// <summary>
/// Default request-scoped implementation of <see cref="IGropHtmlHelper"/>.
/// </summary>
public class GropHtmlHelper : IGropHtmlHelper
{
    private string _activeAdminMenuSystemName = string.Empty;

    /// <inheritdoc />
    public void SetActiveMenuItemSystemName(string systemName)
    {
        _activeAdminMenuSystemName = systemName?.Trim() ?? string.Empty;
    }

    /// <inheritdoc />
    public string GetActiveMenuItemSystemName()
    {
        return _activeAdminMenuSystemName;
    }
}