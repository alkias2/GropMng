using GropMng.Core.Caching;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Cache prefixes for disease and plant health entities.
/// </summary>
public static class DiseaseCacheDefaults
{
    public static string DiseasePrefix => "Grop.disease.";

    public static string PlantDiseaseRecordPrefix => "Grop.plant-disease-record.";

    public static string DiseasePhotoPrefix => "Grop.disease-photo.";

    // -- Service-level keys

    public static GropCacheKey PlantDiseaseRecordsByInstanceKey =>
        new("Grop.plant-disease-record.by-instance.v1.{0}.{1}", PlantDiseaseRecordPrefix);

    public static GropCacheKey DiseasePhotosByRecordKey =>
        new("Grop.disease-photo.by-record.v1.{0}.{1}", DiseasePhotoPrefix);
}
