using GropMng.Core.Domain.Garden.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Models.Dashboard;

public class OwnerDashboardModel
{
    public DashboardWateringTabModel WateringTab { get; set; } = new();

    public DashboardFertilizingTabModel FertilizingTab { get; set; } = new();

    public DashboardDiseaseTabModel DiseaseTab { get; set; } = new();
}

public class DashboardWateringTabModel
{
    public IList<DashboardActionModel> Actions { get; set; } = new List<DashboardActionModel>();

    public IList<SelectListItem> AvailableGardenSpots { get; set; } = new List<SelectListItem>();
}

public class DashboardFertilizingTabModel
{
    public IList<DashboardActionModel> Actions { get; set; } = new List<DashboardActionModel>();

    public IList<SelectListItem> AvailableGardenSpots { get; set; } = new List<SelectListItem>();
}

public class DashboardDiseaseTabModel
{
    public IList<DashboardDiseaseModel> ActiveCases { get; set; } = new List<DashboardDiseaseModel>();
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

    public byte FrequencyDays { get; set; }

    public GardenSeason Season { get; set; }

    public string PlantMainImageUrl { get; set; } = string.Empty;
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
