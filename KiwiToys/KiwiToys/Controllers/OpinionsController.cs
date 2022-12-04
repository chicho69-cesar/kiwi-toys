using Microsoft.AspNetCore.Mvc;

namespace KiwiToys.Controllers {
    public class OpinionsController : Controller {
        [HttpGet]
        public IActionResult Index() {
            return View();
        }
    }
}