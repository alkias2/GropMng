using GropMng.Web.Areas.Admin.Models.Fertilizer;

namespace GropMng.Web.Areas.Admin.Factories.Fertilizer;

/// <summary>
/// Defines factory methods for preparing Fertilizer admin models and persisting changes.
/// </summary>
public interface IFertilizerModelFactory
{
    Task<FertilizerSearchModel> PrepareSearchModelAsync(FertilizerSearchModel? searchModel = null, CancellationToken cancellationToken = default);
    Task<FertilizerListModel> PrepareListModelAsync(FertilizerSearchModel searchModel, CancellationToken cancellationToken = default);
    Task<FertilizerModel> PrepareCreateModelAsync(CancellationToken cancellationToken = default);
    Task<FertilizerModel?> PrepareEditModelAsync(int fertilizerId, CancellationToken cancellationToken = default);
    Task SaveCreateAsync(FertilizerModel model, CancellationToken cancellationToken = default);
    Task<bool> SaveEditAsync(FertilizerModel model, CancellationToken cancellationToken = default);
}
