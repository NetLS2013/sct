using BlockApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlockApp.Controllers
{
    public class ErrorController : Controller
    {
        /// <summary>
        /// Error page action.
        /// </summary>
        /// <param name="id">
        /// Error code.
        /// </param>
        /// <returns>
        /// The <see cref="IActionResult"/>.
        /// </returns>
        public IActionResult Index()
        { 
            return View("Error", new ErrorViewModel { HttpErrorCode = HttpContext.Response.StatusCode });
        }
    }
}