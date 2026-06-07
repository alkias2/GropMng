using GropMng.Web.Areas.Admin.Models.Plant;

namespace GropMng.Web.Areas.Admin.Factories.Plant;

/// <summary>
/// Prepares view models for Plant administration screens.
/// </summary>
public interface IPlantModelFactory
{
    /// <summary>
    /// Initializes a search model for the initial Plant index GET.
    /// Populates the grid definition and available filter options via localization.
    /// </summary>
    Task<PlantSearchModel> PrepareSearchModelAsync(PlantSearchModel? searchModel = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the paged Plant query and returns a DataTables list model.
    /// </summary>
    Task<PlantListModel> PrepareListModelAsync(PlantSearchModel searchModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares a create model including lookup collections.
    /// </summary>
    Task<PlantModel> PrepareCreateModelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares an edit model for the target plant including lookup collections.
    /// </summary>
    Task<PlantModel?> PrepareEditModelAsync(int plantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a new plant from a validated admin model.
    /// </summary>
    Task SaveCreateAsync(PlantModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves updates for an existing plant from a validated admin model.
    /// </summary>
    Task<bool> SaveEditAsync(PlantModel model, CancellationToken cancellationToken = default);
}
