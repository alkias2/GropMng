namespace GropMng.Web.Initialization.Options;

/// <summary>
/// Configuration values for owner dashboard behavior.
/// </summary>
public class DashboardOptions
{
    public const string SectionName = "Dashboard";

    /// <summary>
    /// Upcoming actions horizon (in days) for watering/fertilizing sections.
    /// </summary>
    public int UpcomingHorizonDays { get; set; } = 14;
}
