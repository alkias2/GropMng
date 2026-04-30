using GropMng.Web.Areas.Admin.Models.SoilMix;

namespace GropMng.Web.Areas.Admin.Factories.SoilMix;

/// <summary>
/// Defines contracts for preparing SoilMix admin models and persisting changes.
/// </summary>
public interface ISoilMixModelFactory
{
    Task<SoilMixSearchModel> PrepareSearchModelAsync(SoilMixSearchModel? searchModel = null, CancellationToken cancellationToken = default);

    Task<SoilMixListModel> PrepareListModelAsync(SoilMixSearchModel searchModel, CancellationToken cancellationToken = default);

    Task<SoilMixModel> PrepareCreateModelAsync(CancellationToken cancellationToken = default);

    Task<SoilMixModel?> PrepareEditModelAsync(int soilMixId, CancellationToken cancellationToken = default);

    Task SaveCreateAsync(SoilMixModel model, CancellationToken cancellationToken = default);

    Task<bool> SaveEditAsync(SoilMixModel model, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SoilMixIngredientRowModel>> PrepareIngredientRowsAsync(int soilMixId, CancellationToken cancellationToken = default);

    Task<SoilMixIngredientModel> PrepareIngredientCreateModelAsync(int soilMixId, CancellationToken cancellationToken = default);

    Task AddIngredientAsync(SoilMixIngredientModel model, CancellationToken cancellationToken = default);

    Task UpdateIngredientAsync(int soilMixId, int id, decimal percentageByVolume, string? notes, CancellationToken cancellationToken = default);

    Task DeleteIngredientAsync(int soilMixId, int soilMixIngredientId, CancellationToken cancellationToken = default);
}
