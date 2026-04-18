namespace GropMng.Core.Domain.Localization;

/// <summary>
/// Represents an available application language.
/// </summary>
public class Language : BaseEntity
{
    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the culture code, such as el-GR.
    /// </summary>
    public required string LanguageCulture { get; set; }

    /// <summary>
    /// Gets or sets the SEO code, such as el or en.
    /// </summary>
    public required string UniqueSeoCode { get; set; }

    /// <summary>
    /// Gets or sets the optional flag image file name.
    /// </summary>
    public string? FlagImageFileName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether right-to-left layout is required.
    /// </summary>
    public bool Rtl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the language is published.
    /// </summary>
    public bool Published { get; set; } = true;

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the update timestamp in UTC.
    /// </summary>
    public DateTime UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the locale string resources of the language.
    /// </summary>
    public IList<LocaleStringResource> LocaleStringResources { get; set; } = new List<LocaleStringResource>();
}
