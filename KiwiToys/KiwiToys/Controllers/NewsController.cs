using Microsoft.AspNetCore.Mvc;

namespace KiwiToys.Controllers {
    public class NewsController : Controller {
        [HttpGet]
        public IActionResult Index() {
            return View();
        }
    }
}