namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Cache prefixes for disease and plant health entities.
/// </summary>
public static class DiseaseCacheDefaults
{
    public static string DiseasePrefix => "Grop.disease.";

    public static string PlantDiseaseRecordPrefix => "Grop.plant-disease-record.";
}