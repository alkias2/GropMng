using GropMng.Core;
using GropMng.Core.Domain.Garden.Enums;
using AppLogLevel = GropMng.Core.Domain.Logging.LogLevel;

namespace GropMng.Web.Initialization.Seeders;

/// <summary>
/// Seeds enum localization resources required by the current startup flow.
/// </summary>
internal sealed class EnumLocalizationSeeder
{
    private readonly LocaleResourceSeeder _localeResourceSeeder;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumLocalizationSeeder"/> class.
    /// </summary>
    /// <param name="localeResourceSeeder">The locale resource seeder.</param>
    public EnumLocalizationSeeder(LocaleResourceSeeder localeResourceSeeder)
    {
        _localeResourceSeeder = localeResourceSeeder;
    }

    /// <summary>
    /// Seeds enum localization resources for the specified language.
    /// </summary>
    /// <param name="languageSeoCode">The language SEO code.</param>
    /// <param name="languageId">The language identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous seed operation.</returns>
    public Task SeedAsync(string languageSeoCode, int languageId, CancellationToken cancellationToken = default)
        => _localeResourceSeeder.SeedResourcesAsync(languageId, BuildEnumLocalizationResources(languageSeoCode), cancellationToken);

    private static IReadOnlyDictionary<string, string> BuildEnumLocalizationResources(string languageSeoCode)
    {
        var resources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var greekTranslations = string.Equals(languageSeoCode, "el", StringComparison.OrdinalIgnoreCase)
            ? GetGreekEnumTranslations()
            : null;

        var gardenEnumTypes = typeof(GardenSeason).Assembly
            .GetTypes()
            .Where(type => type.IsEnum && string.Equals(type.Namespace, "GropMng.Core.Domain.Garden.Enums", StringComparison.Ordinal));

        foreach (var enumType in gardenEnumTypes)
        {
            foreach (var enumMemberName in Enum.GetNames(enumType))
            {
                var key = $"Enums.{enumType}.{enumMemberName}";
                resources[key] = ResolveEnumTranslation(languageSeoCode, key, enumMemberName, greekTranslations);
            }
        }

        foreach (var enumMemberName in Enum.GetNames(typeof(AppLogLevel)))
        {
            var key = $"Enums.{typeof(AppLogLevel)}.{enumMemberName}";
            resources[key] = ResolveEnumTranslation(languageSeoCode, key, enumMemberName, greekTranslations);
        }

        return resources;
    }

    private static string ResolveEnumTranslation(
        string languageSeoCode,
        string enumResourceKey,
        string enumMemberName,
        IReadOnlyDictionary<string, string>? greekTranslations)
    {
        var defaultValue = CommonHelper.ConvertEnum(enumMemberName);

        if (!string.Equals(languageSeoCode, "el", StringComparison.OrdinalIgnoreCase))
            return defaultValue;

        if (greekTranslations is not null && greekTranslations.TryGetValue(enumResourceKey, out var translatedValue))
            return translatedValue;

        return defaultValue;
    }

    private static IReadOnlyDictionary<string, string> GetGreekEnumTranslations()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantCategory.Shrub"] = "Θάμνος",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantCategory.Tree"] = "Δέντρο",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantCategory.Climber"] = "Αναρριχώμενο",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantCategory.Ornamental"] = "Καλλωπιστικό",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantCategory.Edible"] = "Βρώσιμο",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantCategory.Aromatic"] = "Αρωματικό",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantCategory.Succulent"] = "Παχύφυτο",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantCategory.Grass"] = "Χόρτο",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantCategory.Fern"] = "Φτέρη",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantCategory.Other"] = "Άλλο",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantGrowthType.Annual"] = "Ετήσιο",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantGrowthType.Biennial"] = "Διετές",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantGrowthType.Perennial"] = "Πολυετές",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantGrowthType.Bulb"] = "Βολβός",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantSunRequirement.FullSun"] = "Πλήρης Ήλιος",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantSunRequirement.PartialShade"] = "Ημισκιά",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantSunRequirement.FullShade"] = "Πλήρης Σκιά",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantWaterRequirement.Low"] = "Χαμηλή",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantWaterRequirement.Moderate"] = "Μέτρια",
            ["Enums.GropMng.Core.Domain.Garden.Enums.PlantWaterRequirement.High"] = "Υψηλή",
            ["Enums.GropMng.Core.Domain.Logging.LogLevel.Trace"] = "Ανίχνευση",
            ["Enums.GropMng.Core.Domain.Logging.LogLevel.Debug"] = "Αποσφαλμάτωση",
            ["Enums.GropMng.Core.Domain.Logging.LogLevel.Information"] = "Πληροφορία",
            ["Enums.GropMng.Core.Domain.Logging.LogLevel.Warning"] = "Προειδοποίηση",
            ["Enums.GropMng.Core.Domain.Logging.LogLevel.Error"] = "Σφάλμα",
            ["Enums.GropMng.Core.Domain.Logging.LogLevel.Critical"] = "Κρίσιμο",

            // New Disease Management enums (from PlantProblem feature)
            ["Enums.GropMng.Core.Domain.Garden.Enums.Severity.Low"] = "Χαμηλή",
            ["Enums.GropMng.Core.Domain.Garden.Enums.Severity.Medium"] = "Μέτρια",
            ["Enums.GropMng.Core.Domain.Garden.Enums.Severity.High"] = "Υψηλή",
            ["Enums.GropMng.Core.Domain.Garden.Enums.ProblemStatus.Active"] = "Ενεργό",
            ["Enums.GropMng.Core.Domain.Garden.Enums.ProblemStatus.Monitoring"] = "Παρακολούθηση",
            ["Enums.GropMng.Core.Domain.Garden.Enums.ProblemStatus.Resolved"] = "Επιλύθηκε",
            ["Enums.GropMng.Core.Domain.Garden.Enums.InfoSource.OwnKnowledge"] = "Δική μου γνώση",
            ["Enums.GropMng.Core.Domain.Garden.Enums.InfoSource.Agronomist"] = "Γεωπόνος",
            ["Enums.GropMng.Core.Domain.Garden.Enums.InfoSource.AITool"] = "Εργαλείο ΑΙ",
            ["Enums.GropMng.Core.Domain.Garden.Enums.InfoSource.Internet"] = "Διαδίκτυο",
            ["Enums.GropMng.Core.Domain.Garden.Enums.InfoSource.Other"] = "Άλλο",
            ["Enums.GropMng.Core.Domain.Garden.Enums.ScheduleFrequencyUnit.Days"] = "Ημέρες",
            ["Enums.GropMng.Core.Domain.Garden.Enums.ScheduleFrequencyUnit.Weeks"] = "Εβδομάδες",
            ["Enums.GropMng.Core.Domain.Garden.Enums.ScheduleFrequencyUnit.Months"] = "Μήνες",
            ["Enums.GropMng.Core.Domain.Garden.Enums.ScheduleStatus.Active"] = "Ενεργό",
            ["Enums.GropMng.Core.Domain.Garden.Enums.ScheduleStatus.Completed"] = "Ολοκληρώθηκε",
            ["Enums.GropMng.Core.Domain.Garden.Enums.ScheduleStatus.Cancelled"] = "Ακυρώθηκε"
        };
    }
}