using GropMng.Core.Domain.Garden.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using GropMng.Core.Domain.Garden.Care;

namespace GropMng.Web.Models.Dashboard;

public class OwnerDashboardModel
{
    public DashboardQueryModel Query { get; set; } = new();

    public DashboardWateringTabModel WateringTab { get; set; } = new();

    public DashboardFertilizingTabModel FertilizingTab { get; set; } = new();

    public DashboardDiseaseTabModel DiseaseTab { get; set; } = new();

    public IList<SelectListItem> AvailableGardenSpots { get; set; } = new List<SelectListItem>();
}

public class DashboardWateringTabModel
{
    public int TodayOverdueCount { get; set; }

    public int UpcomingCount { get; set; }

    public IList<DashboardActionModel> TodayOverdueActions { get; set; } = new List<DashboardActionModel>();

    public IList<DashboardActionGroupModel> UpcomingActionGroups { get; set; } = new List<DashboardActionGroupModel>();

    public IList<SelectListItem> AvailableGardenSpots { get; set; } = new List<SelectListItem>();
}

public class DashboardFertilizingTabModel
{
    public int TodayOverdueCount { get; set; }

    public int UpcomingCount { get; set; }

    public IList<DashboardActionModel> TodayOverdueActions { get; set; } = new List<DashboardActionModel>();

    public IList<DashboardActionGroupModel> UpcomingActionGroups { get; set; } = new List<DashboardActionGroupModel>();

    public IList<SelectListItem> AvailableGardenSpots { get; set; } = new List<SelectListItem>();
}

public class DashboardActionGroupModel
{
    public DateOnly DueDate { get; set; }

    public int DeltaDaysFromToday { get; set; }

    public string GroupLabel { get; set; } = string.Empty;

    public IList<DashboardActionModel> Actions { get; set; } = new List<DashboardActionModel>();
}

public class DashboardQueryModel
{
    public int? SpotId { get; set; }
}

public class DashboardDiseaseTabModel
{
    // Backward-compatible list consumed by the current dashboard UI.
    public IList<DashboardDiseaseModel> ActiveCases { get; set; } = new List<DashboardDiseaseModel>();

    // Explicitly grouped disease cases diagnosed today or earlier.
    public int TodayCount { get; set; }
    public IList<DashboardDiseaseModel> TodayCases { get; set; } = new List<DashboardDiseaseModel>();

    // Explicitly grouped disease cases with a future diagnosed date.
    public int UpcomingCount { get; set; }
    public IList<DashboardDiseaseModel> UpcomingCases { get; set; } = new List<DashboardDiseaseModel>();
}

public class DashboardActionModel
{
    public int PlantInstanceId { get; set; }

    public int GardenSpotId { get; set; }

    public string PlantName { get; set; } = string.Empty;

    public string? Nickname { get; set; }

    public string LocationName { get; set; } = string.Empty;

    public string GardenSpotName { get; set; } = string.Empty;

    public DashboardActionType ActionType { get; set; }

    public DateOnly DueDate { get; set; }

    public DashboardDueStatus DueStatus { get; set; }

    public int DeltaDaysFromToday { get; set; }

    public byte FrequencyDays { get; set; }

    public GardenSeason Season { get; set; }

    public string PlantMainImageUrl { get; set; } = string.Empty;

    // Amount populated from the corresponding schedule (null if not set)
    public decimal? WaterAmountL { get; set; }
    public decimal? FertilizerQuantity { get; set; }
    public FertilizerQuantityUnit? FertilizerQuantityUnit { get; set; }
    public string? FertilizerName { get; set; }
}

public class DashboardDiseaseModel
{
    public int PlantInstanceId { get; set; }

    public string PlantName { get; set; } = string.Empty;

    public string? Nickname { get; set; }

    public string DiseaseName { get; set; } = string.Empty;

    public DateOnly DiagnosedOn { get; set; }

    public string LocationName { get; set; } = string.Empty;

    public string GardenSpotName { get; set; } = string.Empty;

    public PlantDiseaseSeverity? Severity { get; set; }
}

public class DashboardActivityModel
{
    public int PlantInstanceId { get; set; }

    public string PlantName { get; set; } = string.Empty;

    public string? Nickname { get; set; }

    public DashboardActivityType ActivityType { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public string Summary { get; set; } = string.Empty;
}

public enum DashboardActionType
{
    Watering,
    Fertilizing
}

public enum DashboardDueStatus
{
    Overdue,
    Today,
    Upcoming
}

public enum DashboardActivityType
{
    Watering,
    Fertilizing,
    Repotting
}
