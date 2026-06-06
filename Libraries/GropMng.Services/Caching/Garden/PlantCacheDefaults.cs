using GropMng.Core.Caching;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Cache prefixes for plant domain entities.
/// </summary>
public static class PlantCacheDefaults
{
    public static string PlantPrefix => "Grop.plant.";

    public static string PlantInstancePrefix => "Grop.plant-instance.";

    public static string PlantPhotoPrefix => "Grop.plant-photo.";

    public static string PlantNotePrefix => "Grop.plant-note.";

    public static string ContainerPrefix => "Grop.container.";

    // -- PlantInstance keys

    public static GropCacheKey PlantInstancesByOwnerKey =>
        new("Grop.plant-instance.by-owner.v1.{0}", PlantInstancePrefix);

    public static GropCacheKey PlantInstanceByIdKey =>
        new("Grop.plant-instance.by-id.v1.{0}.{1}", PlantInstancePrefix);

    // -- Container keys

    public static GropCacheKey ContainersByOwnerKey =>
        new("Grop.container.by-owner.v1.{0}", ContainerPrefix);

    public static GropCacheKey ContainerByIdKey =>
        new("Grop.container.by-id.v1.{0}.{1}", ContainerPrefix);

    // -- PlantPhoto keys

    public static GropCacheKey PlantPhotosByInstanceKey =>
        new("Grop.plant-photo.by-instance.v1.{0}.{1}", PlantPhotoPrefix);

    public static GropCacheKey PlantPhotoByIdKey =>
        new("Grop.plant-photo.by-id.v1.{0}.{1}.{2}", PlantPhotoPrefix);

    // -- PlantNote keys

    public static GropCacheKey PlantNotesByInstanceKey =>
        new("Grop.plant-note.by-instance.v1.{0}.{1}", PlantNotePrefix);
}
