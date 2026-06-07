using GropMng.Web.Models.Dashboard;

namespace GropMng.Web.Factories.Dashboard;

/// <summary>
/// Defines the contract for preparing view models for the owner dashboard.
/// </summary>
public interface IDashboardModelFactory
{
    Task<DashboardCountersModel> PrepareCountersAsync(CancellationToken ct);

    Task<DashboardWateringTabModel> PrepareWateringTabAsync(
        DashboardQueryModel? query,
        CancellationToken ct);

    Task<DashboardFertilizingTabModel> PrepareFertilizingTabAsync(
        DashboardQueryModel? query,
        CancellationToken ct);

    Task<DashboardDiseaseTabModel> PrepareDiseaseTabAsync(
        CancellationToken ct);

}
