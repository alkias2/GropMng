namespace GropMng.Web.Models
{
    /// <summary>
    /// Represents a view model used by UI rendering and request/response composition.
    /// Carries presentation-focused data without embedding business logic.
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
