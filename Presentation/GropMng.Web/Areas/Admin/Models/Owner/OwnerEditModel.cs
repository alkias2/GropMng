using System.ComponentModel.DataAnnotations;
using GropMng.Core.Domain.Garden.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Models.Owner;

/// <summary>
/// Editable owner-account model for the admin UI.
/// </summary>
public class OwnerEditModel
{
    public Guid OwnerId { get; set; }

    [Required]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string? Password { get; set; }

    public OwnerAccountStatus Status { get; set; }

    public bool IsActive { get; set; }

    public bool IsEmailConfirmed { get; set; }

    public bool IsSystemAdministrator { get; set; }

    public IList<string> SelectedRoleSystemNames { get; set; } = new List<string>();

    public IList<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
}
