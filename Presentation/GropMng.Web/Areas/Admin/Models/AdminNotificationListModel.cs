namespace GropMng.Web.Areas.Admin.Models;

public class AdminNotificationListModel
{
    public int Id { get; set; }

    public string ProblemName { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public string PlantInstanceName { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public bool IsResolved { get; set; }

    public bool ShowResolved { get; set; }

    /// <summary>
    /// The DiseaseKnowledge entry that was created from this notification.
    /// Used to render an "Edit Knowledge" link for resolved notifications.
    /// </summary>
    public int? DiseaseKnowledgeId { get; set; }
}
