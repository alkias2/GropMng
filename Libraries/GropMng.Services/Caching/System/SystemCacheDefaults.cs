namespace GropMng.Services.Caching.System;

/// <summary>
/// Cache prefixes for system-level entities.
/// </summary>
public static class SystemCacheDefaults
{
    public static string LanguagePrefix => "Grop.language.";

    public static string LocalePrefix => "Grop.locale.";

    public static string PermissionPrefix => "Grop.permission.";

    public static string UserPreferencePrefix => "Grop.user-preference.";

    public static string PicturePrefix => "Grop.picture.";
}