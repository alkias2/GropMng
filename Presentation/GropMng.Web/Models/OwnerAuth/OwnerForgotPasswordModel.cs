using System.ComponentModel.DataAnnotations;

namespace GropMng.Web.Models.OwnerAuth;

/// <summary>
/// Represents the public forgot-password request form.
/// </summary>
public class OwnerForgotPasswordModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
