namespace GropMng.Core.Domain.Localization;

/// <summary>
/// Represents a localized key-value resource for a specific language.
/// </summary>
public class LocaleStringResource : BaseEntity
{
    /// <summary>
    /// Gets or sets the language identifier.
    /// </summary>
    public int LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the normalized resource key.
    /// </summary>
    public required string ResourceName { get; set; }

    /// <summary>
    /// Gets or sets the localized value.
    /// </summary>
    public required string ResourceValue { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the update timestamp in UTC.
    /// </summary>
    public DateTime UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the owning language.
    /// </summary>
    public Language Language { get; set; } = null!;
}
