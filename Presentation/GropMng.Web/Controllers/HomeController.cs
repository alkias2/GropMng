using GropMng.Web.Factories.Dashboard;
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
            var query = new Models.Dashboard.DashboardQueryModel { SpotId = spotId };
            var model = await _dashboardModelFactory.PrepareDashboardModelAsync(query, cancellationToken);
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> DashboardWateringPanel(
            int? spotId,
            CancellationToken cancellationToken = default)
        {
            var query = new Models.Dashboard.DashboardQueryModel { SpotId = spotId };
            var model = await _dashboardModelFactory.PrepareDashboardModelAsync(query, cancellationToken);
            return PartialView("_DashboardWateringPanel", model.WateringTab);
        }

        [Authorize]
        public async Task<IActionResult> DashboardFertilizingPanel(
            int? spotId,
            CancellationToken cancellationToken = default)
        {
            var query = new Models.Dashboard.DashboardQueryModel { SpotId = spotId };
            var model = await _dashboardModelFactory.PrepareDashboardModelAsync(query, cancellationToken);
            return PartialView("_DashboardFertilizingPanel", model.FertilizingTab);
        }

        [Authorize]
        public async Task<IActionResult> DashboardDiseasesPanel(CancellationToken cancellationToken)
        {
            var model = await _dashboardModelFactory.PrepareDashboardModelAsync(null, cancellationToken);
            return PartialView("_DashboardDiseasesPanel", model.DiseaseTab);
        }

        #endregion
    }
}
