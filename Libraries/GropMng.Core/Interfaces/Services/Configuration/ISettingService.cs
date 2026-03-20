using GropMng.Core.Configuration;

namespace GropMng.Core.Interfaces.Services.Configuration;

/// <summary>
/// Provides strongly-typed and key-based access to application settings persisted in the database.
/// </summary>
public interface ISettingService
{
    /// <summary>Loads a typed settings object by convention-based key mapping.</summary>
    Task<TSettings> LoadAsync<TSettings>(CancellationToken cancellationToken = default)
        where TSettings : class, ISettings, new();

    /// <summary>Saves all public writable properties of a typed settings object.</summary>
    Task SaveAsync<TSettings>(TSettings settings, CancellationToken cancellationToken = default)
        where TSettings : class, ISettings, new();

    /// <summary>Gets a single setting value by key.</summary>
    Task<TValue> GetByKeyAsync<TValue>(string key, TValue defaultValue = default!, CancellationToken cancellationToken = default);

    /// <summary>Sets a single setting value by key.</summary>
    Task SetByKeyAsync<TValue>(string key, TValue value, CancellationToken cancellationToken = default);

    /// <summary>Returns all settings whose key starts with the provided prefix (case-insensitive).</summary>
    Task<IReadOnlyDictionary<string, string>> GetAllByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default);
}
