using GropMng.Core.Domain.Garden.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Models.Garden;

public class ProblemRecordEditModel
{
    public int? Id { get; set; }

    public int PlantInstanceId { get; set; }

    public string PlantInstanceName { get; set; } = string.Empty;

    public string ProblemName { get; set; } = string.Empty;

    public int? DiseaseKnowledgeId { get; set; }

    public DateOnly DetectedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public string Severity { get; set; } = Core.Domain.Garden.Enums.Severity.Medium.ToString();

    public string ProblemStatus { get; set; } = Core.Domain.Garden.Enums.ProblemStatus.Active.ToString();

    public string InfoSource { get; set; } = Core.Domain.Garden.Enums.InfoSource.OwnKnowledge.ToString();

    public string? Notes { get; set; }

    public bool NotifyAdmin { get; set; }

    public List<SelectListItem> SeverityOptions { get; set; } = new();
    public List<SelectListItem> ProblemStatusOptions { get; set; } = new();
    public List<SelectListItem> InfoSourceOptions { get; set; } = new();
}