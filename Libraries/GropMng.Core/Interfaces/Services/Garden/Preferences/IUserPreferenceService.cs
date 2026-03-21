using GropMng.Core.Domain.Garden.Preferences;

namespace GropMng.Core.Interfaces.Services.Garden.Preferences;

/// <summary>
/// Service interface for managing UserPreference aggregate root.
/// Handles user-scoped preference settings for units, language, and other user preferences.
/// </summary>
public interface IUserPreferenceService
{
    /// <summary>
    /// Gets user preferences for a specific user by user ID.
    /// </summary>
    /// <param name="userId">The unique user identifier (Guid)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>User preference entity for the specified user</returns>
    /// <exception cref="DomainException">Thrown when user preferences not found</exception>
    Task<UserPreference> GetUserPreferenceAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user preference by preference record ID.
    /// </summary>
    /// <param name="preferenceId">The preference record ID (int)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>User preference entity or null if not found</returns>
    Task<UserPreference?> GetPreferenceByIdAsync(int preferenceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates user preferences for a new user with default or specified values.
    /// </summary>
    /// <param name="preference">The user preference entity to create</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The created user preference with database-generated ID</returns>
    /// <exception cref="DomainException">Thrown when user already has preferences or required fields are missing</exception>
    Task<UserPreference> CreatePreferenceAsync(UserPreference preference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates user preference settings with validation.
    /// </summary>
    /// <param name="preference">The user preference entity with updated values</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The updated user preference</returns>
    /// <exception cref="DomainException">Thrown when preference not found</exception>
    Task<UserPreference> UpdatePreferenceAsync(UserPreference preference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes user preferences by preference ID.
    /// </summary>
    /// <param name="preferenceId">The preference ID (int) to delete</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <exception cref="DomainException">Thrown when preference not found</exception>
    Task DeletePreferenceAsync(int preferenceId, CancellationToken cancellationToken = default);
}

