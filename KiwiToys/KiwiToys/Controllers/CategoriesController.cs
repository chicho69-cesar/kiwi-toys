using KiwiToys.Data.Entities;
using KiwiToys.Data;
using KiwiToys.Helpers;
using KiwiToys.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Vereyon.Web;

namespace KiwiToys.Controllers {
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller {
        private readonly DataContext _context;
        private readonly IFlashMessage _flashMessage;

        public CategoriesController(DataContext context, IFlashMessage flashMessage) {
            _context = context;
            _flashMessage = flashMessage;
        }

        [HttpGet]
        public async Task<IActionResult> Index() {
            return View(await _context.Categories
                .Include(c => c.ProductCategories)
                .ToListAsync());
        }

        [NoDirectAccess]
        public async Task<IActionResult> Delete(int? id) {
            Category category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            try {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _flashMessage.Info("Categoria borrada");
            } catch {
                _flashMessage.Danger("No se puede borrar la categoría porque tiene registros relacionados.");
            }

            return RedirectToAction(nameof(Index));
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(int id = 0) {
            if (id == 0) {
                return View(new Category());
            } else {
                Category category = await _context.Categories
                    .FindAsync(id);

                if (category == null) {
                    return NotFound();
                }

                return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, Category category) {
            if (ModelState.IsValid) {
                try {
                    if (id == 0) {
                        _context.Add(category);
                        await _context.SaveChangesAsync();
                        _flashMessage.Info("Categoria añadida");
                    } else {
                        _context.Update(category);
                        await _context.SaveChangesAsync();
                        _flashMessage.Info("Categoria actualizada");
                    }
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        _flashMessage.Danger("Ya existe una categoría con el mismo nombre.");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }

                    return View(category);
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);

                    return View(category);
                }

                return Json(new {
                    isValid = true,
                    html = ModalHelper
                        .RenderRazorViewToString(
                            this,
                            "_ViewAll",
                            _context.Categories
                                .Include(c => c.ProductCategories)
                                .ToList()
                        )
                });
            }

            return Json(new {
                isValid = false,
                html = ModalHelper
                    .RenderRazorViewToString(this, "AddOrEdit", category)
            });
        }
    }
}