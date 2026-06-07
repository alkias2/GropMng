namespace GropMng.Web.Areas.Admin.Models.Owner;

/// <summary>
/// Represents a single owner row in the admin grid.
/// </summary>
public class OwnerRowModel
{
    public string OwnerId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string RolesSummary { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public bool IsEmailConfirmed { get; set; }
}
