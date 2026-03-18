using System.Reflection;
using System.Runtime.Serialization;

namespace GropMng.Core.Domain.Garden.Enums;

public enum GardenSeason
{
    Spring,
    Summer,
    Autumn,
    Winter,
    AllYear
}

public enum PlantHealthStatus
{
    Excellent,
    Good,
    Fair,
    Poor,
    Critical
}

public enum GardenContainerType
{
    Pot,
    Bed,
    HangingBasket,
    WindowBox,
    RaisedBed,
    Other
}

public enum PlantDiseaseType
{
    Fungal,
    Bacterial,
    Viral,
    Pest,
    Deficiency,
    Physiological,
    Other
}

public enum RemedyTreatmentType
{
    Preventive,
    Curative
}

public enum LengthUnitType
{
    [EnumMember(Value = "cm")]
    Centimetre,

    [EnumMember(Value = "in")]
    Inch
}

public enum VolumeUnitType
{
    [EnumMember(Value = "l")]
    Litre,

    [EnumMember(Value = "gal")]
    Gallon
}

public enum TemperatureUnitType
{
    [EnumMember(Value = "C")]
    Celsius,

    [EnumMember(Value = "F")]
    Fahrenheit
}

public enum FertilizerQuantityUnit
{
    [EnumMember(Value = "g")]
    Gram,

    [EnumMember(Value = "kg")]
    Kilogram,

    [EnumMember(Value = "ml")]
    Millilitre,

    [EnumMember(Value = "l")]
    Litre,

    [EnumMember(Value = "tbsp")]
    Tablespoon,

    [EnumMember(Value = "tsp")]
    Teaspoon
}

public enum GardenTimeOfDay
{
    Morning,
    Midday,
    Evening,
    Any
}

public enum PlantSizeCategory
{
    Seedling,
    Small,
    Medium,
    Large,
    Mature
}

public enum PlantDiseaseSeverity
{
    Mild,
    Moderate,
    Severe,
    Critical
}

public enum PlantDiseaseOutcome
{
    Resolved,
    Ongoing,
    Lost,
    Unknown
}

public enum AiQueryScenario
{
    Watering,
    Fertilizing,
    Repotting,
    Planting,
    Disease,
    Pruning,
    Pest,
    General
}

public enum GardenOrientation
{
    [EnumMember(Value = "N")]
    North,

    [EnumMember(Value = "NE")]
    NorthEast,

    [EnumMember(Value = "E")]
    East,

    [EnumMember(Value = "SE")]
    SouthEast,

    [EnumMember(Value = "S")]
    South,

    [EnumMember(Value = "SW")]
    SouthWest,

    [EnumMember(Value = "W")]
    West,

    [EnumMember(Value = "NW")]
    NorthWest
}

public enum GardenCoverType
{
    Open,
    Covered,

    [EnumMember(Value = "Semi-covered")]
    SemiCovered
}

public enum PlantCategory
{
    Shrub,
    Tree,
    Climber,
    Ornamental,
    Edible,
    Aromatic,
    Succulent,
    Grass,
    Fern,
    Other
}

public enum PlantGrowthType
{
    Annual,
    Biennial,
    Perennial,
    Bulb
}

public enum PlantSunRequirement
{
    FullSun,
    PartialShade,
    FullShade
}

public enum PlantWaterRequirement
{
    Low,
    Moderate,
    High
}

public enum SoilTextureType
{
    Sandy,
    Loamy,
    Clay,
    Silty,
    Peaty,
    Chalky
}

public enum SoilDrainageType
{
    Poor,
    Moderate,
    Good,
    Excellent
}

public enum FertilizerKind
{
    Organic,
    Chemical,
    Mineral,
    Liquid,
    Granular,
    SlowRelease
}

public enum FertilizerApplicationMethod
{
    Soil,
    Foliar,
    Drip,
    Diluted
}

public enum PesticideKind
{
    Fungicide,
    Insecticide,
    Herbicide,
    Acaricide,
    Bactericide,
    Biostimulant,
    Other
}

public enum PesticideApplicationMethod
{
    Spray,
    Soil,
    Drench,
    Granule,
    Systemic
}

public enum SupportedLanguage
{
    [EnumMember(Value = "el")]
    Greek,

    [EnumMember(Value = "en")]
    English
}

public static class EnumStorageValueExtensions
{
    public static string ToStorageValue<TEnum>(this TEnum enumValue)
        where TEnum : struct, Enum
    {
        var member = typeof(TEnum).GetMember(enumValue.ToString())[0];
        var enumMemberAttribute = member.GetCustomAttribute<EnumMemberAttribute>();

        return enumMemberAttribute?.Value ?? enumValue.ToString();
    }

    public static TEnum FromStorageValue<TEnum>(string storageValue)
        where TEnum : struct, Enum
    {
        foreach (var member in typeof(TEnum).GetMembers(BindingFlags.Public | BindingFlags.Static))
        {
            var enumMemberAttribute = member.GetCustomAttribute<EnumMemberAttribute>();
            var candidateValue = enumMemberAttribute?.Value ?? member.Name;

            if (string.Equals(candidateValue, storageValue, StringComparison.OrdinalIgnoreCase))
            {
                return Enum.Parse<TEnum>(member.Name, ignoreCase: true);
            }
        }

        throw new InvalidOperationException($"Unsupported storage value '{storageValue}' for enum '{typeof(TEnum).Name}'.");
    }

    public static IReadOnlyList<string> GetStorageValues<TEnum>()
        where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>()
            .Select(static value => value.ToStorageValue())
            .ToArray();
    }
}