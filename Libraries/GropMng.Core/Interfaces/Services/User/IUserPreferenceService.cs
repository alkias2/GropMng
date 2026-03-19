using GropMng.Core.Domain.Garden.Preferences;

namespace GropMng.Core.Interfaces.Services.User;

/// <summary>
/// Service interface for managing user preferences.
/// Handles per-user preference storage, retrieval, and deletion in a key-value pattern.
/// </summary>
public interface IUserPreferenceService
{
    #region User Preference Operations

    /// <summary>
    /// Gets all preferences for a specific user.
    /// </summary>
    /// <param name="userId">The user UUID</param>
    /// <returns>Dictionary of preference key-value pairs</returns>
    Task<IDictionary<string, string>> GetAllPreferencesAsync(Guid userId);

    /// <summary>
    /// Gets a specific preference value for a user.
    /// </summary>
    /// <param name="userId">The user UUID</param>
    /// <param name="key">The preference key</param>
    /// <returns>Preference value or null if not found</returns>
    Task<string> GetPreferenceAsync(Guid userId, string key);

    /// <summary>
    /// Sets (creates or updates) a user preference.
    /// </summary>
    /// <param name="userId">The user UUID</param>
    /// <param name="key">The preference key</param>
    /// <param name="value">The preference value</param>
    /// <returns>The created or updated UserPreference entity</returns>
    /// <exception cref="DomainException">Thrown when key or value is empty</exception>
    Task<UserPreference> SetPreferenceAsync(Guid userId, string key, string value);

    /// <summary>
    /// Deletes a specific user preference.
    /// </summary>
    /// <param name="userId">The user UUID</param>
    /// <param name="key">The preference key to delete</param>
    /// <exception cref="DomainException">Thrown when preference not found</exception>
    Task DeletePreferenceAsync(Guid userId, string key);

    /// <summary>
    /// Deletes all preferences for a user.
    /// </summary>
    /// <param name="userId">The user UUID</param>
    Task DeleteAllPreferencesAsync(Guid userId);

    #endregion
}
