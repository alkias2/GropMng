using GropMng.Web.Models.Dashboard;

namespace GropMng.Web.Factories.Dashboard;

/// <summary>
/// Defines the contract for preparing view models for the owner dashboard.
/// </summary>
public interface IDashboardModelFactory
{
    Task<DashboardWateringTabModel> PrepareWateringTabAsync(
        DashboardQueryModel? query,
        CancellationToken ct);

    Task<DashboardFertilizingTabModel> PrepareFertilizingTabAsync(
        DashboardQueryModel? query,
        CancellationToken ct);

    Task<DashboardDiseaseTabModel> PrepareDiseaseTabAsync(
        CancellationToken ct);

    /// <summary>
    /// Prepares the full owner dashboard model, including today's watering, fertilizing,
    /// and active disease tab data.
    /// </summary>
    Task<OwnerDashboardModel> PrepareDashboardModelAsync(
        DashboardQueryModel? query = null,
        CancellationToken cancellationToken = default);
}
