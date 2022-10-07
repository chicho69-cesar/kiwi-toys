using KiwiToys.Data;
using Microsoft.AspNetCore.Mvc;

namespace KiwiToys.Controllers {
    public class AccountsController : Controller {
        private readonly DataContext _context;

        public AccountsController(DataContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Login() {
            if (User.Identity.IsAuthenticated) {
                return RedirectToAction("Index", "Home");
            }

            return View(/*new LoginViewModel()*/);
        }

        [HttpGet]
        public IActionResult NotAuthorized() {
            return View();
        }

        public dynamic GetViewBag() {
            return ViewBag;
        }
    }
}