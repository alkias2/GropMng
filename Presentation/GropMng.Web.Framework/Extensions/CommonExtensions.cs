namespace GropMng.Web.Framework.Extensions;

/// <summary>
/// Extensions
/// </summary>
public static class CommonExtensions
{
    /// <summary>
    /// Κόβει το string στο επιθυμητό μήκος και προσθέτει "..." αν χρειάζεται.
    /// </summary>
    /// <param name="value">Το string προς επεξεργασία</param>
    /// <param name="maxLength">Μέγιστο μήκος συμπεριλαμβανομένων των "..."</param>
    /// <param name="ellipsis">Το suffix (default: "...")</param>
    public static string Truncate(this string? value, int maxLength, string ellipsis = "...")
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (maxLength <= 0) return string.Empty;
        if (value.Length <= maxLength) return value;

        int truncateAt = maxLength - ellipsis.Length;

        if (truncateAt <= 0) return ellipsis[..maxLength];

        return value[..truncateAt] + ellipsis;
    }

    /// <summary>
    /// Truncate που κόβει σε ολόκληρη λέξη (δεν κόβει στη μέση λέξης).
    /// </summary>
    public static string TruncateAtWord(this string? value, int maxLength, string ellipsis = "...")
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Length <= maxLength) return value;

        int truncateAt = maxLength - ellipsis.Length;
        if (truncateAt <= 0) return ellipsis[..maxLength];

        // Βρες το τελευταίο κενό πριν το όριο
        int lastSpace = value.LastIndexOf(' ', truncateAt);
        int cutPoint = lastSpace > 0 ? lastSpace : truncateAt;

        return value[..cutPoint].TrimEnd() + ellipsis;
    }
}