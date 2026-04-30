using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Controllers
{
    public class HomeController : Controller
    {
        #region Methods

        /// <summary>
        /// Public landing page. Authenticated owners are redirected to their dashboard.
        /// </summary>
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction(nameof(Dashboard));

            return View();
        }

        /// <summary>
        /// Placeholder owner dashboard — will be replaced in feature/owner-dashboard-core.
        /// </summary>
        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        #endregion
    }
}
