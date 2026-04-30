using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Domain.Garden.Locations;

namespace GropMng.Core.Domain.Garden.Plants;

public partial class PlantInstance : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int PlantId { get; set; }

    public int GardenSpotId { get; set; }

    public int? ContainerId { get; set; }

    public int? SoilMixId { get; set; }

    public string? Nickname { get; set; }

    public DateOnly? PlantedDate { get; set; }

    public int? AgeYears
    {
        get
        {
            if (!PlantedDate.HasValue)
            {
                return null;
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var years = today.Year - PlantedDate.Value.Year;

            if (today < PlantedDate.Value.AddYears(years))
            {
                years--;
            }

            return years;
        }
    }

    public PlantSizeCategory? SizeCategory { get; set; }

    public decimal? HeightCm { get; set; }

    public decimal? SpreadCm { get; set; }

    public PlantHealthStatus HealthStatus { get; set; } = PlantHealthStatus.Good;

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    public Plant Plant { get; set; } = null!;

    public GardenSpot GardenSpot { get; set; } = null!;

    public Container? Container { get; set; }

    public SoilMix? SoilMix { get; set; }

    public IList<PlantPhoto> Photos { get; set; } = new List<PlantPhoto>();

    public IList<PlantNote> NotesEntries { get; set; } = new List<PlantNote>();

    public IList<WateringSchedule> WateringSchedules { get; set; } = new List<WateringSchedule>();

    public IList<FertilizingSchedule> FertilizingSchedules { get; set; } = new List<FertilizingSchedule>();

    public IList<PlantDiseaseRecord> DiseaseRecords { get; set; } = new List<PlantDiseaseRecord>();

    public IList<Care.WateringLog> WateringLogs { get; set; } = new List<Care.WateringLog>();

    public IList<Care.FertilizingLog> FertilizingLogs { get; set; } = new List<Care.FertilizingLog>();

    public IList<Care.RepottingLog> RepottingLogs { get; set; } = new List<Care.RepottingLog>();
}
