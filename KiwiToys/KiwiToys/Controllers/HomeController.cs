using AutoMapper;
using KiwiToys.Common;
using KiwiToys.Data.Entities;
using KiwiToys.Data;
using KiwiToys.Helpers;
using KiwiToys.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Vereyon.Web;
using Microsoft.EntityFrameworkCore;

namespace KiwiToys.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IOrdersHelper _ordersHelper;
        private readonly IFlashMessage _flashMessage;
        private readonly IMapper _mapper;

        public HomeController(
            ILogger<HomeController> logger,
            DataContext context,
            IUserHelper userHelper,
            IOrdersHelper ordersHelper,
            IFlashMessage flashMessage,
            IMapper mapper
        ) {
            _logger = logger;
            _context = context;
            _userHelper = userHelper;
            _ordersHelper = ordersHelper;
            _flashMessage = flashMessage;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "NameDesc" : "";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "PriceDesc" : "Price";

            if (searchString != null) {
                pageNumber = 1;
            } else {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            IQueryable<Product> query = _context.Products
               .Include(p => p.ProductImages)
               .Include(p => p.ProductCategories)
               .ThenInclude(pc => pc.Category);

            if (!string.IsNullOrEmpty(searchString)) {
                query = query
                    .Where(p => (
                        p.Name.ToLower()
                            .Contains(searchString.ToLower()) ||
                        p.ProductCategories
                            .Any(pc => pc.Category.Name.ToLower().Contains(searchString.ToLower()))) &&
                        p.Stock > 0
                    );
            } else {
                query = query
                    .Where(p => p.Stock > 0);
            }

            query = sortOrder switch {
                "NameDesc" => query
                    .OrderByDescending(p => p.Name),
                "Price" => query
                    .OrderBy(p => p.Price),
                "PriceDesc" => query
                    .OrderByDescending(p => p.Price),
                _ => query
                    .OrderBy(p => p.Name),
            };

            int pageSize = 9;

            var model = new HomeViewModel {
                Products = await PaginatedList<Product>.CreateAsync(query, pageNumber ?? 1, pageSize),
                Categories = await _context.Categories.ToListAsync(),
            };

            User user = await _userHelper
                .GetUserAsync(User.Identity.Name);

            if (user != null) {
                model.Quantity = await _context.TemporalSales
                    .Where(ts => ts.User.Id == user.Id)
                    .SumAsync(ts => ts.Quantity);
            }

            return View(model);
        }

        /*TODO: Hacer que el agregar al carrito sea a traves de AJAX
        para evitar que la pagina se recarge*/
        [HttpGet]
        public async Task<IActionResult> Add(int? id) {
            if (id == null) {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated) {
                return RedirectToAction("Login", "Account");
            }

            Product product = await _context.Products.FindAsync(id);
            if (product == null) {
                return NotFound();
            }

            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null) {
                return NotFound();
            }

            var temporalSale = new TemporalSale {
                Product = product,
                Quantity = 1,
                User = user
            };

            _context.TemporalSales.Add(temporalSale);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) {
                return NotFound();
            }

            string categories = "";
            foreach (ProductCategory category in product.ProductCategories)
                categories += $"{category.Category.Name}, ";
            categories = categories.Substring(0, categories.Length - 2);

            var model = new AddProductToCartViewModel {
                Categories = categories,
                Description = product.Description,
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                ProductImages = product.ProductImages,
                Quantity = 1,
                Stock = product.Stock
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(AddProductToCartViewModel model) {
            if (!User.Identity.IsAuthenticated) {
                return RedirectToAction("Login", "Account");
            }

            Product product = await _context.Products
                .FindAsync(model.Id);

            if (product == null) {
                return NotFound();
            }

            User user = await _userHelper
                .GetUserAsync(User.Identity.Name);

            if (user == null) {
                return NotFound();
            }

            var temporalSale = new TemporalSale {
                Product = product,
                Quantity = model.Quantity,
                Remarks = model.Remarks,
                User = user
            };

            _context.TemporalSales.Add(temporalSale);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ShowCart() {
            User user = await _userHelper
                .GetUserAsync(User.Identity.Name);

            if (user == null) {
                return NotFound();
            }

            List<TemporalSale> temporalSales = await _context.TemporalSales
                .Include(ts => ts.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(ts => ts.User.Id == user.Id)
                .ToListAsync();

            var model = new ShowCartViewModel {
                User = user,
                TemporalSales = temporalSales,
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DecreaseQuantity(int? id) {
            if (id == null) {
                return NotFound();
            }

            TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
            if (temporalSale == null) {
                return NotFound();
            }

            if (temporalSale.Quantity > 1) {
                temporalSale.Quantity--;
                _context.TemporalSales.Update(temporalSale);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ShowCart));
        }

        [HttpGet]
        public async Task<IActionResult> IncreaseQuantity(int? id) {
            if (id == null) {
                return NotFound();
            }

            TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
            if (temporalSale == null) {
                return NotFound();
            }

            temporalSale.Quantity++;
            _context.TemporalSales.Update(temporalSale);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ShowCart));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
            if (temporalSale == null) {
                return NotFound();
            }

            _context.TemporalSales.Remove(temporalSale);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ShowCart));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
            if (temporalSale == null) {
                return NotFound();
            }

            var model = new EditTemporalSaleViewModel {
                Id = temporalSale.Id,
                Quantity = temporalSale.Quantity,
                Remarks = temporalSale.Remarks,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditTemporalSaleViewModel model) {
            if (id != model.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
                    temporalSale.Quantity = model.Quantity;
                    temporalSale.Remarks = model.Remarks;

                    _context.Update(temporalSale);
                    await _context.SaveChangesAsync();
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
                    return View(model);
                }

                return RedirectToAction(nameof(ShowCart));
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult OrderSuccess() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShowCart(ShowCartViewModel model) {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null) {
                return NotFound();
            }

            model.User = user;
            model.TemporalSales = await _context.TemporalSales
                .Include(ts => ts.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(ts => ts.User.Id == user.Id)
                .ToListAsync();

            Response response = await _ordersHelper.ProcessOrderAsync(model);
            if (response.IsSuccess) {
                return RedirectToAction(nameof(OrderSuccess));
            }

            _flashMessage.Danger(response.Message);
            return View(model);
        }

        [HttpGet]
        public IActionResult Privacy() {
            return View();
        }

        [Route("error/404")]
        public IActionResult Error404() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}