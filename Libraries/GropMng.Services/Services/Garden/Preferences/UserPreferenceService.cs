using GropMng.Core;
using GropMng.Core.Domain.Garden;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Preferences;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Preferences;

namespace GropMng.Services.Services.Garden.Preferences;

/// <summary>
/// Service implementation for managing UserPreference aggregate root.
/// Handles user-scoped preference settings for units, language, and other user preferences.
/// </summary>
public class UserPreferenceService : IUserPreferenceService
{
    #region Fields

    private readonly IRepository<UserPreference> _preferenceRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the UserPreferenceService class.
    /// </summary>
    public UserPreferenceService(IRepository<UserPreference> preferenceRepository)
    {
        _preferenceRepository = preferenceRepository ?? throw new ArgumentNullException(nameof(preferenceRepository));
    }

    #endregion

    #region Public Methods - CRUD Operations

    /// <summary>
    /// Gets user preferences for a specific user.
    /// </summary>
    public async Task<UserPreference> GetUserPreferenceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User ID is required and cannot be empty.");

        var preferences = await _preferenceRepository.FindAsync(
            p => p.OwnerId == userId,
            false,
            true,
            cancellationToken);

        if (preferences.Count == 0)
            throw new DomainException($"User preferences not found for user '{userId}'.");

        return preferences[0];
    }

    /// <summary>
    /// Gets user preference by preference ID.
    /// </summary>
    public async Task<UserPreference?> GetPreferenceByIdAsync(int preferenceId, CancellationToken cancellationToken = default)
    {
        if (preferenceId <= 0)
            throw new DomainException("Preference ID must be greater than zero.");

        return await _preferenceRepository.GetByIdAsync(preferenceId, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates user preferences for a new user with default values.
    /// </summary>
    public async Task<UserPreference> CreatePreferenceAsync(UserPreference preference, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preference);

        if (preference.OwnerId == Guid.Empty)
            throw new DomainException("User ID is required and cannot be empty.");

        await EnsureUserPreferenceIsUniqueAsync(preference.OwnerId, cancellationToken);

        StampForCreate(preference);

        await _preferenceRepository.CreateAsync(preference, true, cancellationToken);

        return preference;
    }

    /// <summary>
    /// Updates user preference settings with validation.
    /// </summary>
    public async Task<UserPreference> UpdatePreferenceAsync(UserPreference preference, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preference);

        if (preference.Id <= 0)
            throw new DomainException("Preference ID is required for update.");

        if (preference.OwnerId == Guid.Empty)
            throw new DomainException("User ID is required and cannot be empty.");

        var existingPreference = await EnsurePreferenceExistsAsync(preference.Id, cancellationToken);

        existingPreference.LengthUnit = preference.LengthUnit;
        existingPreference.VolumeUnit = preference.VolumeUnit;
        existingPreference.TemperatureUnit = preference.TemperatureUnit;
        existingPreference.DefaultLanguage = preference.DefaultLanguage;

        StampForUpdate(existingPreference);

        await _preferenceRepository.UpdateAsync(existingPreference, true, cancellationToken);

        return existingPreference;
    }

    /// <summary>
    /// Deletes user preferences by preference ID.
    /// </summary>
    public async Task DeletePreferenceAsync(int preferenceId, CancellationToken cancellationToken = default)
    {
        if (preferenceId <= 0)
            throw new DomainException("Preference ID must be greater than zero.");

        var preference = await EnsurePreferenceExistsAsync(preferenceId, cancellationToken);

        await _preferenceRepository.DeleteAsync(preference, true, true, cancellationToken);
    }

    #endregion

    #region Private Methods - Validation & Domain Guards

    /// <summary>
    /// Validates that a preference record exists, throwing an exception if not found.
    /// </summary>
    private async Task<UserPreference> EnsurePreferenceExistsAsync(int preferenceId, CancellationToken cancellationToken = default)
    {
        var preference = await _preferenceRepository.GetByIdAsync(preferenceId, cancellationToken: cancellationToken);

        if (preference == null)
            throw new DomainException($"User preference with ID '{preferenceId}' not found.");

        return preference;
    }

    /// <summary>
    /// Validates that a user has only one preference record.
    /// </summary>
    private async Task EnsureUserPreferenceIsUniqueAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User ID cannot be empty for uniqueness validation.");

        var existingPreferences = await _preferenceRepository.FindAsync(
            p => p.OwnerId == userId,
            false,
            true,
            cancellationToken);

        if (existingPreferences.Count > 0)
            throw new DomainException($"User preferences already exist for user '{userId}'.");
    }

    /// <summary>
    /// Stamps an entity for creation with audit fields.
    /// </summary>
    private void StampForCreate(BaseEntity entity)
    {
        if (entity is AuditableEntity auditableEntity)
        {
            auditableEntity.CreatedAtUtc = DateTime.UtcNow;
            auditableEntity.UpdatedAtUtc = DateTime.UtcNow;
            auditableEntity.IsDeleted = false;
        }
    }

    /// <summary>
    /// Stamps an entity for update with audit fields.
    /// </summary>
    private void StampForUpdate(BaseEntity entity)
    {
        if (entity is AuditableEntity auditableEntity)
        {
            auditableEntity.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    #endregion
}



