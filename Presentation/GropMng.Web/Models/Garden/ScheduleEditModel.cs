using GropMng.Core.Domain.Garden.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Models.Garden;

public class ScheduleEditModel
{
    public int? Id { get; set; }

    public int PlantProblemRecordId { get; set; }

    public string ActionName { get; set; } = string.Empty;

    public int FrequencyValue { get; set; } = 7;

    public string FrequencyUnit { get; set; } = Core.Domain.Garden.Enums.ScheduleFrequencyUnit.Days.ToString();

    public string? DosageNotes { get; set; }

    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateOnly? NextDueDate { get; set; }

    public string ScheduleStatus { get; set; } = Core.Domain.Garden.Enums.ScheduleStatus.Active.ToString();

    public List<SelectListItem> FrequencyUnitOptions { get; set; } = new();
    public List<SelectListItem> ScheduleStatusOptions { get; set; } = new();
}