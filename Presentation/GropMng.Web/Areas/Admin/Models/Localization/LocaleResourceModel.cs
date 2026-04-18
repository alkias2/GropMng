using System.ComponentModel.DataAnnotations;
using GropMng.Web.Framework.Mvc.ModelBinding;

namespace GropMng.Web.Areas.Admin.Models.Localization;

/// <summary>
/// Represents a locale resource create/edit model.
/// </summary>
public class LocaleResourceModel
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the language identifier.
    /// </summary>
    public int LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the resource key.
    /// </summary>
    [Required]
    [MaxLength(400)]
    [GropResourceDisplayName("admin.localization.resource.fields.name")]
    public string ResourceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resource value.
    /// </summary>
    [Required]
    [GropResourceDisplayName("admin.localization.resource.fields.value")]
    public string ResourceValue { get; set; } = string.Empty;
}
