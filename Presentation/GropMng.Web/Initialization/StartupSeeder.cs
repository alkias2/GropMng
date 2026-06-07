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
    private readonly SoilIngredientSeeder _soilIngredientSeeder;
    private readonly PlantCatalogSeeder _plantCatalogSeeder;
    private readonly FertilizerCatalogSeeder _fertilizerCatalogSeeder;
    private readonly DiseaseCatalogSeeder _diseaseCatalogSeeder;
    private readonly LocationAndGardenSpotSeeder _locationAndGardenSpotSeeder;
    private readonly SoilMixSeeder _soilMixSeeder;
    private readonly ContainerSeeder _containerSeeder;
    private readonly PlantInstanceSeeder _plantInstanceSeeder;
    private readonly PlantPhotoSeeder _plantPhotoSeeder;
    private readonly WateringScheduleSeeder _wateringScheduleSeeder;
    private readonly FertilizingScheduleSeeder _fertilizingScheduleSeeder;
    private readonly PlantDiseaseRecordSeeder _plantDiseaseRecordSeeder;
    private readonly RepottingLogSeeder _repottingLogSeeder;

    public StartupSeeder(
        OwnerSeeder ownerSeeder,
        LanguageSeeder languageSeeder,
        LocaleResourceSeeder localeResourceSeeder,
        EnumLocalizationSeeder enumLocalizationSeeder,
        SoilIngredientSeeder soilIngredientSeeder,
        PlantCatalogSeeder plantCatalogSeeder,
        FertilizerCatalogSeeder fertilizerCatalogSeeder,
        DiseaseCatalogSeeder diseaseCatalogSeeder,
        LocationAndGardenSpotSeeder locationAndGardenSpotSeeder,
        SoilMixSeeder soilMixSeeder,
        ContainerSeeder containerSeeder,
        PlantInstanceSeeder plantInstanceSeeder,
        PlantPhotoSeeder plantPhotoSeeder,
        WateringScheduleSeeder wateringScheduleSeeder,
        FertilizingScheduleSeeder fertilizingScheduleSeeder,
        PlantDiseaseRecordSeeder plantDiseaseRecordSeeder,
        RepottingLogSeeder repottingLogSeeder)
    {
        _ownerSeeder = ownerSeeder;
        _languageSeeder = languageSeeder;
        _localeResourceSeeder = localeResourceSeeder;
        _enumLocalizationSeeder = enumLocalizationSeeder;
        _soilIngredientSeeder = soilIngredientSeeder;
        _plantCatalogSeeder = plantCatalogSeeder;
        _fertilizerCatalogSeeder = fertilizerCatalogSeeder;
        _diseaseCatalogSeeder = diseaseCatalogSeeder;
        _locationAndGardenSpotSeeder = locationAndGardenSpotSeeder;
        _soilMixSeeder = soilMixSeeder;
        _containerSeeder = containerSeeder;
        _plantInstanceSeeder = plantInstanceSeeder;
        _plantPhotoSeeder = plantPhotoSeeder;
        _wateringScheduleSeeder = wateringScheduleSeeder;
        _fertilizingScheduleSeeder = fertilizingScheduleSeeder;
        _plantDiseaseRecordSeeder = plantDiseaseRecordSeeder;
        _repottingLogSeeder = repottingLogSeeder;
    }

    /// <inheritdoc />
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // 1. Owner + language baseline
        await _ownerSeeder.SeedAsync(cancellationToken);

        var seededLanguages = await _languageSeeder.SeedAsync(cancellationToken);

        await _localeResourceSeeder.RemoveDeprecatedAppLogLevelResourcesAsync(seededLanguages.GreekLanguage.Id, cancellationToken);
        await _localeResourceSeeder.RemoveDeprecatedAppLogLevelResourcesAsync(seededLanguages.EnglishLanguage.Id, cancellationToken);

        await _localeResourceSeeder.SeedDefaultResourcesAsync("el", seededLanguages.GreekLanguage.Id, cancellationToken);
        await _localeResourceSeeder.SeedDefaultResourcesAsync("en", seededLanguages.EnglishLanguage.Id, cancellationToken);

        await _enumLocalizationSeeder.SeedAsync("el", seededLanguages.GreekLanguage.Id, cancellationToken);
        await _enumLocalizationSeeder.SeedAsync("en", seededLanguages.EnglishLanguage.Id, cancellationToken);

        // 2. Reference catalogs (no OwnerId dependency)
        var ingredientIds = await _soilIngredientSeeder.SeedAsync(cancellationToken);
        var plantIds = await _plantCatalogSeeder.SeedAsync(cancellationToken);
        var fertilizerIds = await _fertilizerCatalogSeeder.SeedAsync(cancellationToken);
        var diseaseIds = await _diseaseCatalogSeeder.SeedAsync(cancellationToken);

        // 3. Owner spatial structure
        var locationResult = await _locationAndGardenSpotSeeder.SeedAsync(cancellationToken);

        // 4. Owner soil mixes (depend on ingredients)
        var soilMixIds = await _soilMixSeeder.SeedAsync(ingredientIds, cancellationToken);

        // 5. Plant instances (depend on all catalog lookups + spatial + soil mixes)
        // Containers are seeded AFTER instances because Container.PlantInstanceId is the FK.
        var instanceIds = await _plantInstanceSeeder.SeedAsync(
            plantIds,
            locationResult.SpotsByName,
            soilMixIds,
            cancellationToken);

        // 6. Owner containers (depend on plant instance IDs — each container links to one instance)
        await _containerSeeder.SeedAsync(instanceIds, cancellationToken);

        // 7. Plant instance photos (depend on plant instance IDs)
        await _plantPhotoSeeder.SeedAsync(instanceIds, cancellationToken);

        // 8. Care schedules + history (depend on plant instance IDs)
        await _wateringScheduleSeeder.SeedAsync(instanceIds, cancellationToken);
        await _fertilizingScheduleSeeder.SeedAsync(instanceIds, fertilizerIds, cancellationToken);
        await _plantDiseaseRecordSeeder.SeedAsync(instanceIds, diseaseIds, cancellationToken);
        await _repottingLogSeeder.SeedAsync(instanceIds, cancellationToken);
    }
}