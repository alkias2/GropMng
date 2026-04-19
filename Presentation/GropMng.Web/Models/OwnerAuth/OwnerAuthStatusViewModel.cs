namespace GropMng.Web.Models.OwnerAuth;

/// <summary>
/// Represents a generic status page for owner authentication flows.
/// </summary>
public class OwnerAuthStatusViewModel
{
    public string PageTitle { get; set; } = string.Empty;

    public string Heading { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? SecondaryMessage { get; set; }

    public string? ActionText { get; set; }

    public string? ActionUrl { get; set; }
}
