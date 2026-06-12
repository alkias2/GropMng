using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Models;

public class DiseaseKnowledgeEditModel
{
    public int? Id { get; set; }

    public string CommonName { get; set; } = string.Empty;

    public string? ScientificName { get; set; }

    public string Description { get; set; } = string.Empty;

    public string TreatmentGuidelines { get; set; } = string.Empty;

    public List<int> SelectedPlantIds { get; set; } = new();

    public MultiSelectList AvailablePlants { get; set; } = new(new List<SelectListItem>());

    public List<DiseaseKnowledgePhotoModel> ExistingPhotos { get; set; } = new();

    public List<IFormFile>? NewPhotos { get; set; }

    public int? FromNotificationId { get; set; }
}

public class DiseaseKnowledgePhotoModel
{
    public int Id { get; init; }

    public int PictureId { get; init; }

    public string PictureUrl { get; init; } = string.Empty;

    public int DisplayOrder { get; init; }

    public string? Caption { get; init; }
}