using GropMng.Web.Areas.Admin.Models.Pesticide;

namespace GropMng.Web.Areas.Admin.Factories.Pesticide;

/// <summary>
/// Defines factory methods for preparing Pesticide admin models and persisting changes.
/// </summary>
public interface IPesticideModelFactory
{
    Task<PesticideSearchModel> PrepareSearchModelAsync(PesticideSearchModel? searchModel = null, CancellationToken cancellationToken = default);
    Task<PesticideListModel> PrepareListModelAsync(PesticideSearchModel searchModel, CancellationToken cancellationToken = default);
    Task<PesticideModel> PrepareCreateModelAsync(CancellationToken cancellationToken = default);
    Task<PesticideModel?> PrepareEditModelAsync(int pesticideId, CancellationToken cancellationToken = default);
    Task SaveCreateAsync(PesticideModel model, CancellationToken cancellationToken = default);
    Task<bool> SaveEditAsync(PesticideModel model, CancellationToken cancellationToken = default);
}
