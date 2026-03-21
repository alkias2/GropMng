using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.Preferences;

public partial class UserPreference : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public LengthUnitType LengthUnit { get; set; } = LengthUnitType.Centimetre;

    public VolumeUnitType VolumeUnit { get; set; } = VolumeUnitType.Litre;

    public TemperatureUnitType TemperatureUnit { get; set; } = TemperatureUnitType.Celsius;

    public SupportedLanguage DefaultLanguage { get; set; } = SupportedLanguage.Greek;
}
