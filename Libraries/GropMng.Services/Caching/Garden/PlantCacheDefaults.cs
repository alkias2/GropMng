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
}