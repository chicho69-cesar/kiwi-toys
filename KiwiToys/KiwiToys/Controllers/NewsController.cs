using KiwiToys.Common;
using KiwiToys.Data;
using KiwiToys.Data.Entities;
using KiwiToys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KiwiToys.Controllers {
    public class NewsController : Controller {
        private readonly DataContext _context;

        public NewsController(DataContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page) {
            int pageSize = 2;

            IQueryable<News> query = _context.News
                .OrderByDescending(n => n.Date);

            var model = new NewsViewModel {
                News = await PaginatedList<News>
                    .CreateAsync(query, page ?? 1, pageSize)
            };

            return View(model);
        }
    }
}