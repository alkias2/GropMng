namespace GropMng.Web.Framework.Localization;

/// <summary>
/// Represents a resolved localized string value.
/// </summary>
public sealed class LocalizedString
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizedString"/> class.
    /// </summary>
    /// <param name="value">The localized value.</param>
    public LocalizedString(string value)
    {
        Value = value ?? string.Empty;
    }

    /// <summary>
    /// Gets the localized value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Returns the localized value.
    /// </summary>
    /// <returns>The localized value.</returns>
    public override string ToString() => Value;
}
