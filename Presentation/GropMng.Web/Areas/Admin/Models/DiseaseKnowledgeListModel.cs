namespace GropMng.Web.Areas.Admin.Models;

/// <summary>
/// Represents a row in the disease knowledge base DataTable grid.
/// </summary>
public class DiseaseKnowledgeListModel
{
    /// <summary>
    /// The knowledge base entry identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The common name of the disease.
    /// </summary>
    public string CommonName { get; set; } = string.Empty;

    /// <summary>
    /// The scientific name of the disease, if any.
    /// </summary>
    public string? ScientificName { get; set; }

    /// <summary>
    /// The number of linked plant types.
    /// </summary>
    public int LinkedPlantCount { get; set; }

    /// <summary>
    /// The number of attached photos.
    /// </summary>
    public int PhotoCount { get; set; }

    /// <summary>
    /// The creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}