using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Controllers
{
    /// <summary>
    /// Represents the HomeController component.
    /// Defines responsibilities and data relevant to its role in the GropMng solution.
    /// </summary>
    public class HomeController : Controller
    {
        #region Fields

        private readonly ILogger<HomeController> _logger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the HomeController class.
        /// </summary>
        /// <param name="logger">Logger used for diagnostics and runtime telemetry.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Dashboard placeholder - to be implemented
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        #endregion
    }
}
