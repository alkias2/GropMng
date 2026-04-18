using GropMng.Web.Initialization.Seeders;

namespace GropMng.Web.Initialization;

/// <summary>
/// Orchestrates startup seed execution in the required deterministic order.
/// </summary>
internal sealed class StartupSeeder : IStartupSeeder
{
    private readonly OwnerSeeder _ownerSeeder;
    private readonly LanguageSeeder _languageSeeder;
    private readonly LocaleResourceSeeder _localeResourceSeeder;
    private readonly EnumLocalizationSeeder _enumLocalizationSeeder;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupSeeder"/> class.
    /// </summary>
    /// <param name="ownerSeeder">The owner seeder.</param>
    /// <param name="languageSeeder">The language seeder.</param>
    /// <param name="localeResourceSeeder">The locale resource seeder.</param>
    /// <param name="enumLocalizationSeeder">The enum localization seeder.</param>
    public StartupSeeder(
        OwnerSeeder ownerSeeder,
        LanguageSeeder languageSeeder,
        LocaleResourceSeeder localeResourceSeeder,
        EnumLocalizationSeeder enumLocalizationSeeder)
    {
        _ownerSeeder = ownerSeeder;
        _languageSeeder = languageSeeder;
        _localeResourceSeeder = localeResourceSeeder;
        _enumLocalizationSeeder = enumLocalizationSeeder;
    }

    /// <inheritdoc />
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _ownerSeeder.SeedAsync(cancellationToken);

        var seededLanguages = await _languageSeeder.SeedAsync(cancellationToken);

        await _localeResourceSeeder.RemoveDeprecatedAppLogLevelResourcesAsync(seededLanguages.GreekLanguage.Id, cancellationToken);
        await _localeResourceSeeder.RemoveDeprecatedAppLogLevelResourcesAsync(seededLanguages.EnglishLanguage.Id, cancellationToken);

        await _localeResourceSeeder.SeedDefaultResourcesAsync("el", seededLanguages.GreekLanguage.Id, cancellationToken);
        await _localeResourceSeeder.SeedDefaultResourcesAsync("en", seededLanguages.EnglishLanguage.Id, cancellationToken);

        await _enumLocalizationSeeder.SeedAsync("el", seededLanguages.GreekLanguage.Id, cancellationToken);
        await _enumLocalizationSeeder.SeedAsync("en", seededLanguages.EnglishLanguage.Id, cancellationToken);
    }
}