namespace GropMng.Core.Domain.Logging;

public partial class AppLog : BaseEntity
{
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? ExceptionType { get; set; }
    public int? EventId { get; set; }
    public string? RequestPath { get; set; }
    public DateTime Timestamp { get; set; }
}