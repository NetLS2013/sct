using Microsoft.AspNetCore.Mvc;

namespace BlockApp.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Landing page.
        /// </summary>
        /// <returns>
        /// The <see cref="IActionResult"/>.
        /// </returns>
        public IActionResult Index()
        {
            ViewData["Message"] = "Your home page.";
            
            return View();
        }
    }
}