using GropMng.Web.Areas.Admin.Models.Disease;

namespace GropMng.Web.Areas.Admin.Factories.Disease;

/// <summary>
/// Defines contracts for preparing Disease admin models and persisting changes.
/// </summary>
public interface IDiseaseModelFactory
{
    Task<DiseaseSearchModel> PrepareSearchModelAsync(DiseaseSearchModel? searchModel = null, CancellationToken cancellationToken = default);

    Task<DiseaseListModel> PrepareListModelAsync(DiseaseSearchModel searchModel, CancellationToken cancellationToken = default);

    Task<DiseaseModel> PrepareCreateModelAsync(CancellationToken cancellationToken = default);

    Task<DiseaseModel?> PrepareEditModelAsync(int diseaseId, CancellationToken cancellationToken = default);

    Task SaveCreateAsync(DiseaseModel model, CancellationToken cancellationToken = default);

    Task<bool> SaveEditAsync(DiseaseModel model, CancellationToken cancellationToken = default);

    // RemedyLink sub-operations
    Task<IReadOnlyList<DiseaseRemedyLinkRowModel>> PrepareRemedyLinkRowsAsync(int diseaseId, CancellationToken cancellationToken = default);

    Task<DiseaseRemedyLinkModel> PrepareRemedyLinkCreateModelAsync(int diseaseId, CancellationToken cancellationToken = default);

    Task AddRemedyLinkAsync(DiseaseRemedyLinkModel model, CancellationToken cancellationToken = default);

    Task DeleteRemedyLinkAsync(int diseaseId, int remedyLinkId, CancellationToken cancellationToken = default);
}
