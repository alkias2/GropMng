using System.ComponentModel.DataAnnotations;
using GropMng.Web.Framework.Mvc.ModelBinding;

namespace GropMng.Web.Areas.Admin.Models.Localization;

/// <summary>
/// Represents a language create/edit model.
/// </summary>
public class LanguageModel
{
    /// <summary>
    /// Gets or sets the language identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    [GropResourceDisplayName("admin.localization.language.fields.name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the culture value.
    /// </summary>
    [Required]
    [MaxLength(20)]
    [GropResourceDisplayName("admin.localization.language.fields.culture")]
    public string LanguageCulture { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SEO code.
    /// </summary>
    [Required]
    [MaxLength(2)]
    [GropResourceDisplayName("admin.localization.language.fields.seocode")]
    public string UniqueSeoCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional flag image file name.
    /// </summary>
    [MaxLength(100)]
    [GropResourceDisplayName("admin.localization.language.fields.flag")]
    public string? FlagImageFileName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether RTL layout is used.
    /// </summary>
    [GropResourceDisplayName("admin.localization.language.fields.rtl")]
    public bool Rtl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the language is published.
    /// </summary>
    [GropResourceDisplayName("admin.localization.language.fields.published")]
    public bool Published { get; set; } = true;

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    [GropResourceDisplayName("admin.localization.language.fields.displayorder")]
    public int DisplayOrder { get; set; }
}
