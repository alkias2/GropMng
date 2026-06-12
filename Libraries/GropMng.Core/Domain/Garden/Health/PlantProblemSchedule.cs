using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.Health;

public partial class PlantProblemSchedule : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int PlantProblemRecordId { get; set; }

    public required string ActionName { get; set; }

    public int FrequencyValue { get; set; }

    public ScheduleFrequencyUnit FrequencyUnit { get; set; } = ScheduleFrequencyUnit.Days;

    public string? DosageNotes { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly NextDueDate { get; set; }

    public ScheduleStatus ScheduleStatus { get; set; } = ScheduleStatus.Active;

    public PlantProblemRecord PlantProblemRecord { get; set; } = null!;
}