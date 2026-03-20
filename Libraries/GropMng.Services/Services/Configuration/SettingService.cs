using System.ComponentModel;
using System.Reflection;
using GropMng.Core.Configuration;
using GropMng.Core.Domain.Configuration;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace GropMng.Services.Services.Configuration;

/// <summary>
/// Database-backed implementation of <see cref="ISettingService"/> using the Setting table.
/// </summary>
public class SettingService : ISettingService
{
    private const string SettingsCacheKey = "grop.settings.all";

    private readonly IRepository<Setting> _settingRepository;
    private readonly IMemoryCache _memoryCache;

    public SettingService(IRepository<Setting> settingRepository, IMemoryCache memoryCache)
    {
        _settingRepository = settingRepository;
        _memoryCache = memoryCache;
    }

    public async Task<TSettings> LoadAsync<TSettings>(CancellationToken cancellationToken = default)
        where TSettings : class, ISettings, new()
    {
        var settings = new TSettings();
        var allSettings = await GetAllSettingsDictionaryAsync(cancellationToken);
        var prefix = BuildSettingsPrefix(typeof(TSettings));

        foreach (var property in GetPersistableProperties(typeof(TSettings)))
        {
            var key = prefix + property.Name.ToLowerInvariant();
            if (!allSettings.TryGetValue(key, out var rawValue))
                continue;

            var converted = ConvertFromString(rawValue, property.PropertyType);
            if (converted != null)
                property.SetValue(settings, converted);
        }

        return settings;
    }

    public async Task SaveAsync<TSettings>(TSettings settings, CancellationToken cancellationToken = default)
        where TSettings : class, ISettings, new()
    {
        ArgumentNullException.ThrowIfNull(settings);

        var prefix = BuildSettingsPrefix(typeof(TSettings));
        foreach (var property in GetPersistableProperties(typeof(TSettings)))
        {
            var key = prefix + property.Name.ToLowerInvariant();
            var value = property.GetValue(settings);
            await SetByKeyInternalAsync(key, value, cancellationToken);
        }

        InvalidateCache();
    }

    public async Task<TValue> GetByKeyAsync<TValue>(string key, TValue defaultValue = default!, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeKey(key);
        var allSettings = await GetAllSettingsDictionaryAsync(cancellationToken);

        if (!allSettings.TryGetValue(normalized, out var rawValue))
            return defaultValue;

        var converted = ConvertFromString(rawValue, typeof(TValue));
        if (converted == null)
            return defaultValue;

        return (TValue)converted;
    }

    public async Task SetByKeyAsync<TValue>(string key, TValue value, CancellationToken cancellationToken = default)
    {
        await SetByKeyInternalAsync(key, value, cancellationToken);
        InvalidateCache();
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAllByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default)
    {
        var normalizedPrefix = NormalizeKey(keyPrefix);
        var allSettings = await GetAllSettingsDictionaryAsync(cancellationToken);

        return allSettings
            .Where(kvp => kvp.Key.StartsWith(normalizedPrefix, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private async Task SetByKeyInternalAsync(string key, object? value, CancellationToken cancellationToken)
    {
        var normalized = NormalizeKey(key);
        var stringValue = ConvertToInvariantString(value);

        var existing = await _settingRepository.FirstOrDefaultAsync(
            s => s.Name == normalized,
            includeDeleted: false,
            asNoTracking: false,
            cancellationToken: cancellationToken);

        if (existing == null)
        {
            await _settingRepository.CreateAsync(new Setting
            {
                Name = normalized,
                Value = stringValue
            }, saveNow: true, cancellationToken: cancellationToken);

            return;
        }

        existing.Value = stringValue;
        await _settingRepository.UpdateAsync(existing, saveNow: true, cancellationToken: cancellationToken);
    }

    private async Task<Dictionary<string, string>> GetAllSettingsDictionaryAsync(CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(SettingsCacheKey, out Dictionary<string, string>? cached) && cached != null)
            return cached;

        var all = await _settingRepository.GetAllAsync(
            queryShaper: query => query.OrderBy(s => s.Name),
            includeDeleted: false,
            cancellationToken: cancellationToken);

        var dictionary = all
            .GroupBy(s => NormalizeKey(s.Name))
            .ToDictionary(g => g.Key, g => g.Last().Value ?? string.Empty);

        _memoryCache.Set(SettingsCacheKey, dictionary, TimeSpan.FromMinutes(30));
        return dictionary;
    }

    private static IEnumerable<PropertyInfo> GetPersistableProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0);
    }

    private static string BuildSettingsPrefix(Type settingsType)
        => $"grop.{settingsType.Name.ToLowerInvariant()}.";

    private static string NormalizeKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return key.Trim().ToLowerInvariant();
    }

    private static string ConvertToInvariantString(object? value)
    {
        if (value == null)
            return string.Empty;

        var converter = TypeDescriptor.GetConverter(value.GetType());
        return converter.ConvertToInvariantString(value) ?? string.Empty;
    }

    private static object? ConvertFromString(string rawValue, Type destinationType)
    {
        if (destinationType == typeof(string))
            return rawValue;

        if (string.IsNullOrWhiteSpace(rawValue))
            return destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;

        var converter = TypeDescriptor.GetConverter(destinationType);
        return converter.ConvertFromInvariantString(rawValue);
    }

    private void InvalidateCache()
    {
        _memoryCache.Remove(SettingsCacheKey);
    }
}
