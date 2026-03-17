namespace GropMng.Web.ViewModels;

/// <summary>
/// Represents a view model used by UI rendering and request/response composition.
/// Carries presentation-focused data without embedding business logic.
/// </summary>
public class AppLogListItemViewModel
{
    public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
