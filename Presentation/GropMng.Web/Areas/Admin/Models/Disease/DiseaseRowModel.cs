namespace GropMng.Web.Areas.Admin.Models.Disease;

/// <summary>
/// Represents a single row in the disease list DataTable.
/// </summary>
public class DiseaseRowModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string DiseaseTypeLocalized { get; set; } = string.Empty;

    public string? AffectedParts { get; set; }
}
