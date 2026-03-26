namespace GropMng.Web.Areas.Admin.Models.Logging;

/// <summary>
/// Represents a single row in the AppLog DataTables grid.
/// </summary>
public class AppLogRowModel
{
    public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string LevelLocalized { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string TimestampLocalized { get; set; } = string.Empty;
}
