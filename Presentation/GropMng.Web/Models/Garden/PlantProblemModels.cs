namespace GropMng.Web.Models.Garden;

/// <summary>
/// ViewModel for the Problems tab content area.
/// </summary>
public class ProblemsTabViewModel
{
    public int PlantInstanceId { get; init; }
    public string PlantInstanceName { get; init; } = string.Empty;
    public List<ProblemRecordCardModel> Records { get; init; } = new();
}

/// <summary>
/// ViewModel for a single problem record card displayed in the problems tab.
/// </summary>
public class ProblemRecordCardModel
{
    public int Id { get; init; }
    public string ProblemName { get; init; } = string.Empty;
    public int? DiseaseKnowledgeId { get; init; }
    public string? DiseaseKnowledgeCommonName { get; init; }
    public DateOnly DetectedDate { get; init; }
    public string SeverityDisplay { get; init; } = string.Empty;
    public string SeverityBadgeClass { get; init; } = string.Empty;
    public string ProblemStatusDisplay { get; init; } = string.Empty;
    public string ProblemStatusBadgeClass { get; init; } = string.Empty;
    public string InfoSourceDisplay { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateOnly? ResolvedDate { get; init; }
    public List<ScheduleLineModel> Schedules { get; init; } = new();
}

/// <summary>
/// ViewModel for a single schedule line displayed inside a problem card.
/// </summary>
public class ScheduleLineModel
{
    public int Id { get; init; }
    public string ActionName { get; init; } = string.Empty;
    public int FrequencyValue { get; init; }
    public string FrequencyUnitDisplay { get; init; } = string.Empty;
    public DateOnly NextDueDate { get; init; }
    public string ScheduleStatusDisplay { get; init; } = string.Empty;
    public string ScheduleStatusBadgeClass { get; init; } = string.Empty;
}