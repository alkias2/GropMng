using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Web.Models.Dashboard;

public class OwnerDashboardModel
{
    public int ActivePlantsCount { get; set; }

    public int ActionsTodayCount { get; set; }

    public int OverdueActionsCount { get; set; }

    public int ActiveDiseaseCasesCount { get; set; }

    public IList<DashboardActionModel> TodayActions { get; set; } = new List<DashboardActionModel>();

    public IList<DashboardActivityModel> RecentActivity { get; set; } = new List<DashboardActivityModel>();
}

public class DashboardActionModel
{
    public int PlantInstanceId { get; set; }

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
