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

        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction(nameof(Dashboard));

            return View();
        }

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

        [Authorize]
        public async Task<IActionResult> DashboardWateringPanel(
            int? spotId,
            CancellationToken cancellationToken = default)
        {
            var query = new Models.Dashboard.DashboardQueryModel { SpotId = spotId };
            var model = await _dashboardModelFactory.PrepareWateringTabAsync(query, cancellationToken);
            return PartialView("_DashboardWateringPanel", model);
        }

        [Authorize]
        public async Task<IActionResult> DashboardFertilizingPanel(
            int? spotId,
            CancellationToken cancellationToken = default)
        {
            var query = new Models.Dashboard.DashboardQueryModel { SpotId = spotId };
            var model = await _dashboardModelFactory.PrepareFertilizingTabAsync(query, cancellationToken);
            return PartialView("_DashboardFertilizingPanel", model);
        }

        [Authorize]
        public async Task<IActionResult> DashboardDiseasesPanel(CancellationToken cancellationToken)
        {
            var model = await _dashboardModelFactory.PrepareDiseaseTabAsync(cancellationToken);
            return PartialView("_DashboardDiseasesPanel", model);
        }

        #endregion
    }
}
