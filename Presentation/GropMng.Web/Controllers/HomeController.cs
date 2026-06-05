using GropMng.Web.Factories.Dashboard;
using GropMng.Web.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDashboardModelFactory _dashboardModelFactory;

        public HomeController(IDashboardModelFactory dashboardModelFactory)
        {
            _dashboardModelFactory = dashboardModelFactory;
        }

        #region Methods

        /// <summary>
        /// Displays the landing page for unauthenticated users,
        /// and redirects authenticated users to the dashboard.
        /// </summary>
        /// <returns>The landing page view, or a redirect to the dashboard.</returns>
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction(nameof(Dashboard));

            return View();
        }

        /// <summary>
        /// Renders the owner dashboard with counters, watering data, and available garden spots.
        /// </summary>
        /// <param name="spotId">Optional garden spot identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The dashboard view with the populated <see cref="OwnerDashboardModel"/>.</returns>
        [Authorize]
        public async Task<IActionResult> Dashboard(
            int? spotId,
            CancellationToken cancellationToken = default)
        {
            var query = new DashboardQueryModel { SpotId = spotId };

            var counters = await _dashboardModelFactory.PrepareCountersAsync(cancellationToken);
            var watering = await _dashboardModelFactory.PrepareWateringTabAsync(query, cancellationToken);

            var model = new OwnerDashboardModel
            {
                Query = query,
                Counters = counters,
                WateringTab = watering,
                AvailableGardenSpots = watering.AvailableGardenSpots
            };

            return View(model);
        }

        /// <summary>
        /// Returns a partial view containing the watering panel for AJAX updates.
        /// </summary>
        /// <param name="spotId">Optional garden spot identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The "_DashboardWateringPanel" partial view.</returns>
        [Authorize]
        public async Task<IActionResult> DashboardWateringPanel(
            int? spotId,
            CancellationToken cancellationToken = default)
        {
            var query = new Models.Dashboard.DashboardQueryModel { SpotId = spotId };
            var model = await _dashboardModelFactory.PrepareWateringTabAsync(query, cancellationToken);
            return PartialView("_DashboardWateringPanel", model);
        }

        /// <summary>
        /// Returns a partial view containing the fertilizing panel for AJAX updates.
        /// </summary>
        /// <param name="spotId">Optional garden spot identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The "_DashboardFertilizingPanel" partial view.</returns>
        [Authorize]
        public async Task<IActionResult> DashboardFertilizingPanel(
            int? spotId,
            CancellationToken cancellationToken = default)
        {
            var query = new Models.Dashboard.DashboardQueryModel { SpotId = spotId };
            var model = await _dashboardModelFactory.PrepareFertilizingTabAsync(query, cancellationToken);
            return PartialView("_DashboardFertilizingPanel", model);
        }

        /// <summary>
        /// Returns a partial view containing the diseases panel for AJAX updates.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The "_DashboardDiseasesPanel" partial view.</returns>
        [Authorize]
        public async Task<IActionResult> DashboardDiseasesPanel(CancellationToken cancellationToken)
        {
            var model = await _dashboardModelFactory.PrepareDiseaseTabAsync(cancellationToken);
            return PartialView("_DashboardDiseasesPanel", model);
        }

        #endregion
    }
}
