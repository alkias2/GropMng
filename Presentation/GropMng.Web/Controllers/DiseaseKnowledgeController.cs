using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Core.Interfaces.Services.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Controllers;

/// <summary>
/// Provides user-facing search and info panel endpoints for the disease knowledge base.
/// </summary>
[Authorize]
[Route("my-garden/disease-knowledge")]
public class DiseaseKnowledgeController : Controller
{
    #region Fields

    private readonly IDiseaseKnowledgeService _diseaseKnowledgeService;
    private readonly IRepository<DiseaseKnowledgePhoto> _photoRepository;
    private readonly IRepository<DiseaseKnowledgePlant> _plantLinkRepository;
    private readonly IRepository<Core.Domain.Garden.Plants.Plant> _plantRepository;
    private readonly IPictureService _pictureService;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="DiseaseKnowledgeController"/> class.
    /// </summary>
    public DiseaseKnowledgeController(
        IDiseaseKnowledgeService diseaseKnowledgeService,
        IRepository<DiseaseKnowledgePhoto> photoRepository,
        IRepository<DiseaseKnowledgePlant> plantLinkRepository,
        IRepository<Core.Domain.Garden.Plants.Plant> plantRepository,
        IPictureService pictureService)
    {
        _diseaseKnowledgeService = diseaseKnowledgeService ?? throw new ArgumentNullException(nameof(diseaseKnowledgeService));
        _photoRepository = photoRepository ?? throw new ArgumentNullException(nameof(photoRepository));
        _plantLinkRepository = plantLinkRepository ?? throw new ArgumentNullException(nameof(plantLinkRepository));
        _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
        _pictureService = pictureService ?? throw new ArgumentNullException(nameof(pictureService));
    }

    #endregion

    #region Public

    /// <summary>
    /// Returns JSON array of matching disease knowledge entries for autocomplete.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(string q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(Array.Empty<object>());

        var results = await _diseaseKnowledgeService.SearchAsync(q, cancellationToken: cancellationToken);

        var items = results.Take(10).Select(d => new
        {
            id = d.Id,
            commonName = d.CommonName,
            scientificName = d.ScientificName
        });

        return Json(items);
    }

    /// <summary>
    /// Returns partial HTML with Description and TreatmentGuidelines for the info panel.
    /// </summary>
    [HttpGet("info-panel/{id:int}")]
    public async Task<IActionResult> InfoPanel(int id, CancellationToken cancellationToken)
    {
        var knowledge = await _diseaseKnowledgeService.GetByIdAsync(id, cancellationToken);

        ViewBag.CommonName = knowledge.CommonName;
        ViewBag.Description = knowledge.Description;
        ViewBag.TreatmentGuidelines = knowledge.TreatmentGuidelines;

        return PartialView("_DiseaseInfoPanel");
    }

    /// <summary>
    /// Returns the detail modal partial with full disease knowledge information including photos and linked plants.
    /// </summary>
    [HttpGet("detail/{id:int}")]
    public async Task<IActionResult> DetailModal(int id, CancellationToken cancellationToken)
    {
        var knowledge = await _diseaseKnowledgeService.GetByIdAsync(id, cancellationToken);

        // Load photos ordered by display order
        var photos = await _photoRepository.GetAllAsync(
            query => query.Where(p => p.DiseaseKnowledgeId == id && !p.IsDeleted)
                .OrderBy(p => p.DisplayOrder),
            cancellationToken: cancellationToken);

        var mainPhotoUrl = (string?)null;
        var extraPhotoUrls = new List<string>();

        foreach (var photo in photos)
        {
            var url = await _pictureService.GetPictureUrlAsync(photo.PictureId, targetSize: 400);
            if (mainPhotoUrl is null)
                mainPhotoUrl = url;
            else
                extraPhotoUrls.Add(url);
        }

        // Load linked plant names
        var plantLinks = await _plantLinkRepository.GetAllAsync(
            query => query.Where(p => p.DiseaseKnowledgeId == id && !p.IsDeleted),
            cancellationToken: cancellationToken);

        var linkedPlantIds = plantLinks.Select(p => p.PlantId).ToHashSet();
        var linkedPlants = new List<string>();

        if (linkedPlantIds.Count > 0)
        {
            var plants = await _plantRepository.GetAllAsync(
                query => query.Where(p => linkedPlantIds.Contains(p.Id) && !p.IsDeleted),
                cancellationToken: cancellationToken);

            linkedPlants = plants
                .OrderBy(p => p.ScientificName)
                .Select(p => p.ScientificName)
                .ToList();
        }

        ViewBag.CommonName = knowledge.CommonName;
        ViewBag.ScientificName = knowledge.ScientificName;
        ViewBag.Description = knowledge.Description;
        ViewBag.TreatmentGuidelines = knowledge.TreatmentGuidelines;
        ViewBag.MainPhotoUrl = mainPhotoUrl;
        ViewBag.ExtraPhotoUrls = extraPhotoUrls;
        ViewBag.LinkedPlants = linkedPlants;

        return PartialView("_DetailModal");
    }

    #endregion
}
